using System.ComponentModel.DataAnnotations;

namespace P4_Backend_Car_App.DTOs.User
{
    public class LoginDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }
    }
}
