

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P4_Backend_Car_App.Data;
using P4_Backend_Car_App.Interfaces;
using P4_Backend_Car_App.Models;
using P4_Backend_Car_App.Types;
using Microsoft.AspNetCore.RateLimiting;
using P4_Backend_Car_App.DTOs.User;
using P4_Backend_Car_App.DTOs;



namespace P4_Backend_Car_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IMailService _mailService;
        private const string AuthCookieName = "car_app_token";

        public UserController(AppDbContext context,ITokenService tokenService, IMailService mailService)
        {
            _context = context;
            _tokenService = tokenService;
            _mailService = mailService;
        }

        // CREATE USER
        [EnableRateLimiting("registerPolicy")]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] RegisterDto dto, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var username = dto.Username.Trim();
            var email = dto.Email.Trim();

            var normalizedUsername = username.ToUpper();
            var normalizedEmail = email.ToUpper();

            bool exists = await _context.Users.AnyAsync(
                x => x.NormalizedUsername == normalizedUsername ||
                     x.NormalizedEmail == normalizedEmail, ct);

            if (exists)
            {
                return BadRequest(new { message = "Username or Email already exists" });
            }

            if (dto.Password.Length < 8 ||
                !dto.Password.Any(char.IsUpper) ||
                !dto.Password.Any(char.IsDigit))
            {
                return BadRequest(new
                {
                    message = "Password must be 8+ chars, include uppercase and number"
                });
            }

            var code = Random.Shared.Next(100000, 999999).ToString();

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = email,
                Username = username,
                NormalizedEmail = normalizedEmail,
                NormalizedUsername = normalizedUsername,
                Role = Role.Customer,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),


                 // EMAIL VERIFICATION
                IsEmailConfirmed = false,
                EmailVerificationCode = code,
                CodeExpiry = DateTime.UtcNow.AddMinutes(10)
            };

            _context.Users.Add(user);

            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                return BadRequest(new { message = "Username or Email already exists" });
            }

            // 📧 SEND EMAIL
            await _mailService.SendEmailAsync(
                email,
                "Verify your account",
                $"Your verification code is: <b>{code}</b>",
                isHtml: true
            );

            return Ok(new
            {
                message = "User created. Please verify your email."
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

            return Ok(users);
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

            return Ok(user);
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

            return Ok(new { message = "User updated by admin" });
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

            return Ok(new { message = "User updated" });
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

            Response.Cookies.Delete(AuthCookieName);

            await _context.SaveChangesAsync(ct);

            return Ok(new { message = "User deactivated" });
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
                return BadRequest(new { message = "Please verify your email first" });
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

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                IsEssential = true,
                Expires = DateTime.UtcNow.AddDays(1)
            };

            Response.Cookies.Append(AuthCookieName, token, cookieOptions);

            return Ok(new
            {
                message = "Login successful",
                data = new
                {
                    username = user.Username,
                    role = user.Role
                }
            });
        }
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailDto dto, CancellationToken ct)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == dto.Email, ct);

            if (user == null)
                return BadRequest("User not found");

            if (user.IsEmailConfirmed)
                return BadRequest("Already verified");

            if (user.EmailVerificationCode != dto.Code ||
                user.CodeExpiry < DateTime.UtcNow)
            {
                return BadRequest("Invalid or expired code");
            }

            user.IsEmailConfirmed = true;
            user.EmailVerificationCode = null;
            user.CodeExpiry = null;

            await _context.SaveChangesAsync(ct);

            return Ok(new { message = "Email verified successfully" });
        }
    }
}