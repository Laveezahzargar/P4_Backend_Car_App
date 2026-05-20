using P4_Backend_Car_App.Types;
using System.ComponentModel.DataAnnotations;

namespace P4_Backend_Car_App.DTOs.User
{
    public class RegisterDto
    {
            [Required]
            [MaxLength(100)]
            public string FullName { get; set; }

            [Required]
            [EmailAddress]
            [MaxLength(150)]
            public string Email { get; set; }

            [Required]
            [MinLength(3)]
            [MaxLength(50)]
            public string Username { get; set; }

            [Required]
            [MinLength(8)]
            public string Password { get; set; }
    }
}