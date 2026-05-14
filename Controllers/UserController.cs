

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P4_Backend_Car_App.Data;
using P4_Backend_Car_App.DTOs;
using P4_Backend_Car_App.Interfaces;
using P4_Backend_Car_App.Models;
using P4_Backend_Car_App.Services;

namespace P4_Backend_Car_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;

        public UserController(AppDbContext context,ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromForm] UserDto dto)
        {
            // check username
            bool exists =
                await _context.Users.AnyAsync(
                    x => x.Username == dto.Username);

            if (exists)
            {
                return BadRequest(
                    "Username already exists");
            }

            User user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Username = dto.Username,

                // HASH PASSWORD
                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        dto.Password)
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "User added successfully", data = user.Id });
        }

        // READ ALL
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users =
                await _context.Users.ToListAsync();

            return Ok(new { statusCode = 200, message = "Users retrieved successfully", data = users});
        }

        // READ BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user =
                await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(new { statusCode = 200, message = "user retrieved successfully", data = user });
        }

        // UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id,[FromForm] UserDto dto)
        {
            var user =
                await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Username = dto.Username;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        dto.Password);
            }

            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "user updated successfully", data = user.Id });
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user =
                await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "user deleted successfully", data = user.Id });
        }

        //Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(
                    x => x.Username == dto.Username);

            if (user == null)
            {
                return BadRequest(
                    new
                    {
                        statusCode = 400,
                        message = "Invalid username"
                    });
            }

            bool validPassword =
                BCrypt.Net.BCrypt.Verify(
                    dto.Password,
                    user.PasswordHash);

            if (!validPassword)
            {
                return BadRequest(
                    new
                    {
                        statusCode = 400,
                        message = "Invalid password"
                    });
            }

            var token = _tokenService.CreateToken(user.Id, user.Email, user.Username, 60 * 24);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,

                Expires = DateTime.UtcNow.AddDays(1)
            };

            // SAVE TOKEN IN COOKIE
            Response.Cookies.Append(
                "car_app_token",
                token,
                cookieOptions);

            return Ok(
                new
                {
                    statusCode = 200,
                    message = "Login successful",
                    data = new
                    {
                        user.Id,
                        user.FullName
                    }
                });
        }
    }
}