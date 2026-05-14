
using System.ComponentModel.DataAnnotations;
using P4_Backend_Car_App.Types;
using Role = P4_Backend_Car_App.Types.Role;


namespace P4_Backend_Car_App.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public  Role Role { get; set; }
        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;
    }
}