

using CloudinaryDotNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using P4_Backend_Car_App.Data;
using P4_Backend_Car_App.DTOs;
using P4_Backend_Car_App.DTOs.User;
using P4_Backend_Car_App.Interfaces;
using P4_Backend_Car_App.Models;
using P4_Backend_Car_App.Types;



namespace P4_Backend_Car_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IMailService _mailService;
        private readonly IMemoryCache _cache;

        public UserController(AppDbContext context,ITokenService tokenService, IMailService mailService, IMemoryCache cache)
        {
            _context = context;
            _tokenService = tokenService;
            _mailService = mailService;
            _cache = cache;
        }
        [EnableRateLimiting("registerPolicy")]
        [HttpPost("SendVerificationCode")]
        public async Task<IActionResult> SendVerificationCode([FromBody] EmailDto dto)
        {
            var email = dto.Email.Trim().ToUpper();

            var code = Random.Shared.Next(100000, 999999).ToString();

           // var hashedCode = BCrypt.Net.BCrypt.HashPassword(code);
            _cache.Set($"otp:{email}", code, TimeSpan.FromMinutes(10));

            await _mailService.SendEmailAsync(
                dto.Email,
                "Verify your account",
                $"Your code is: <b>{code}</b>",
                true
            );

            return Ok(true);
        }
        [EnableRateLimiting("registerPolicy")]
        [HttpPost("VerifyAndCreate")]
        public async Task<IActionResult> VerifyAndCreate([FromBody] RegisterDto dto, CancellationToken ct)
        {
            var email = dto.Email.Trim();
            var normalizedEmail = email.ToUpper();

            _cache.TryGetValue($"otp:{normalizedEmail}", out string cachedCode);

            // 🔥 ATTEMPT LIMIT
            var attemptsKey = $"otp_attempts:{normalizedEmail}";
            int attempts = _cache.Get<int>(attemptsKey);

            if (attempts >= 5)
            {
                return BadRequest(new { message = "Too many attempts. Try again later." });
            }

            // 🔥 EXPIRED
            if (string.IsNullOrEmpty(cachedCode))
            {
                return BadRequest(new { message = "Code expired" });
            }

            // 🔥 WRONG CODE
            if (cachedCode != dto.Code)
            {
                _cache.Set(attemptsKey, attempts + 1, TimeSpan.FromMinutes(10));
                return BadRequest(new { message = "Invalid code" });
            }

            // 🔥 SUCCESS → RESET ATTEMPTS
            _cache.Remove(attemptsKey);

            var exists = await _context.Users.AnyAsync(
                x => x.NormalizedEmail == normalizedEmail, ct);

            if (exists)
            {
                return BadRequest(new { message = "User already exists" });
            }

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = email,
                Username = dto.Username.Trim(),
                NormalizedEmail = normalizedEmail,
                NormalizedUsername = dto.Username.ToUpper(),
                Role = Role.Customer,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IsEmailConfirmed = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(ct);

            var tokenExpiryMinutes = 60 * 24;

            var token = _tokenService.CreateToken(
                user.Id,
                user.Email,
                user.Username,
                user.Role,
                tokenExpiryMinutes);

            _cache.Remove($"otp:{normalizedEmail}");

            return Ok(new
            {
                message = "Registration successful",
                token = token,
                data = new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.Role
                }
            });
        }
        // 🔐 GET ALL USERS (ADMIN ONLY)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers(CancellationToken ct)
        {
            var users = await _context.Users
                .Select(x => new AdminResponseDto
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    Email = x.Email,
                    Username = x.Username,
                    Role = x.Role,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    LastLoginAt = x.LastLoginAt
                })
                .ToListAsync(ct);

            return Ok(new { statusCode = 200, message = "Users retrieved sucessfully ", data = users });
        }
        // 🔐 GET USER BY ID
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst("id");
            if (userIdClaim == null)
                return Unauthorized();

            int userIdFromToken = int.Parse(userIdClaim.Value);

            if (userIdFromToken != id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }
            var user = await _context.Users
                .Where(x => x.Id == id)
                .Select(x => new UserProfileDto
                {
                    FullName = x.FullName,
                    Email = x.Email,
                    Username = x.Username
                })
                .FirstOrDefaultAsync(ct);

            if (user == null)
                return NotFound();

            return Ok(new { statusCode = 200, message = "User retrieved sucessfully ", data = user });
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("admin/{id}")]
        public async Task<IActionResult> AdminUpdateUser(int id, AdminUserUpdateDto dto, CancellationToken ct)
        {
            var user = await _context.Users.FindAsync([id], ct);

            if (user == null)
                return NotFound();

            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var normalizedEmail = dto.Email.Trim().ToUpper();

                bool exists = await _context.Users.AnyAsync(x =>
                    x.Id != id && x.NormalizedEmail == normalizedEmail, ct);

                if (exists)
                    return BadRequest(new { message = "Email already exists" });

                user.Email = dto.Email.Trim();
                user.NormalizedEmail = normalizedEmail;
            }

            if (!string.IsNullOrWhiteSpace(dto.Username))
            {
                var normalizedUsername = dto.Username.Trim().ToUpper();

                bool exists = await _context.Users.AnyAsync(x =>
                    x.Id != id && x.NormalizedUsername == normalizedUsername, ct);

                if (exists)
                    return BadRequest(new { message = "Username already exists" });

                user.Username = dto.Username.Trim();
                user.NormalizedUsername = normalizedUsername;
            }

            // Admin can change role
            user.Role = dto.Role;

            // Admin can activate/deactivate
            user.IsActive = dto.IsActive;

            // Optional password reset
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                if (dto.Password.Length < 8 ||
                    !dto.Password.Any(char.IsUpper) ||
                    !dto.Password.Any(char.IsDigit))
                {
                    return BadRequest(new { message = "Weak password" });
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return Ok(new { statusCode = 200, message = "User updated by admin", data= user.Id });
        }

        // 🔐 UPDATE USER self
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto dto, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst("id");
            if (userIdClaim == null)
                return Unauthorized();

            int userIdFromToken = int.Parse(userIdClaim.Value);

            if (userIdFromToken != id && !User.IsInRole("Admin"))
                return Forbid();

            var user = await _context.Users.FindAsync([id], ct);

            if (user == null)
                return NotFound();

            // Normalize only if provided
            var normalizedEmail = dto.Email?.Trim().ToUpper();
            var normalizedUsername = dto.Username?.Trim().ToUpper();

            // Check uniqueness only for changed fields
            bool exists = await _context.Users.AnyAsync(x =>
                x.Id != id &&
                (
                    (normalizedEmail != null && x.NormalizedEmail == normalizedEmail) ||
                    (normalizedUsername != null && x.NormalizedUsername == normalizedUsername)
                ), ct);

            if (exists)
            {
                return BadRequest(new { message = "Username or Email already exists" });
            }

            // Update only provided fields
            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                user.Email = dto.Email.Trim();
                user.NormalizedEmail = user.Email.ToUpper();
            }

            if (!string.IsNullOrWhiteSpace(dto.Username))
            {
                user.Username = dto.Username.Trim();
                user.NormalizedUsername = user.Username.ToUpper();
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                if (dto.Password.Length < 8 ||
                    !dto.Password.Any(char.IsUpper) ||
                    !dto.Password.Any(char.IsDigit))
                {
                    return BadRequest(new { message = "Weak password" });
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return Ok(new {StatusCode=200, message = "User updated", data= user.Id });
        }

        // 🔐 SOFT DELETE
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst("id");
            if (userIdClaim == null)
                return Unauthorized();

            int userIdFromToken = int.Parse(userIdClaim.Value);

            if (userIdFromToken != id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }
            var user = await _context.Users.FindAsync([id], ct);

            if (user == null)
                return NotFound();

            user.IsActive = false;

   

            await _context.SaveChangesAsync(ct);

            return Ok(new { statusCode = 200, message = "User deactivated", data=user.Id });
        }

        //Login
        [EnableRateLimiting("loginPolicy")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
        {

                ct.ThrowIfCancellationRequested();
                var normalizedUsername = dto.Username.Trim().ToUpper();

                var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.NormalizedUsername == normalizedUsername, ct);

                string fakeHash = "$2a$11$7EqJtq98hPqEX7fNZaFWoO7EqJtq98hPqEX7fNZaFWoO7EqJtq98hPq";

                if (user == null)
                {
                    BCrypt.Net.BCrypt.Verify(dto.Password, fakeHash);
                    return BadRequest(new { message = "Invalid credentials" });
                }
                if (!user.IsActive)
                {
                    return BadRequest(new { message = "Invalid credentials" });
                }
                if (!user.IsEmailConfirmed)
                {
                    return BadRequest(new
                    {
                        message = "EMAIL_NOT_VERIFIED",
                        email = user.Email
                    });
                }

            if (user.LockoutEnd > DateTime.UtcNow)
                {
                    return BadRequest(new { message = "Account locked. Try later." });
                }

                bool validPassword = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

                if (!validPassword)
                {
                    user.FailedLoginAttempts++;

                    if (user.FailedLoginAttempts >= 5)
                    {
                        user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                        user.FailedLoginAttempts = 0;
                    }

                    user.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync(ct);

                    return BadRequest(new { message = "Invalid credentials" });
                }
                if (user.FailedLoginAttempts != 0)
                {
                    user.FailedLoginAttempts = 0;
                }

                user.LockoutEnd = null;
                user.LastLoginAt = DateTime.UtcNow;
                user.LastLoginIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(ct);

                var tokenExpiryMinutes = 60 * 24; // move to config later

                var token = _tokenService.CreateToken(
                    user.Id,
                    user.Email,
                    user.Username,
                    user.Role,
                    tokenExpiryMinutes);

            return Ok(new
            {
                message = "Login successful",
                token = token,
                data = new
                {
                    username = user.Username,
                    email = user.Email,
                    role = user.Role
                }
            });
        }
        [HttpPost("VerifyLoginOtp")]
        public async Task<IActionResult> VerifyLoginOtp([FromBody] VerifyOtpDto dto)
        {
            var email = dto.Email.Trim().ToUpper();

            _cache.TryGetValue($"otp:{email}", out string cachedCode);

            var attemptsKey = $"otp_attempts:{email}";
            int attempts = _cache.Get<int>(attemptsKey);

            if (attempts >= 5)
            {
                return BadRequest(new { message = "Too many attempts. Try again later." });
            }

            if (string.IsNullOrEmpty(cachedCode))
            {
                _cache.Remove(attemptsKey);
                return BadRequest(new { message = "Code expired" });
            }

            if (cachedCode != dto.Code)
            {
                _cache.Set(attemptsKey, attempts + 1, TimeSpan.FromMinutes(10));
                return BadRequest(new { message = "Invalid code" });
            }

            // ✅ SUCCESS
            _cache.Remove(attemptsKey);

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.NormalizedEmail == email);

            if (user == null)
                return BadRequest(new { message = "User not found" });

            user.IsEmailConfirmed = true;

            await _context.SaveChangesAsync();

            _cache.Remove($"otp:{email}");

            return Ok(new
            {
                username = user.Username,
                role = user.Role
            });
        }
    }
}