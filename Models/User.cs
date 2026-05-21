
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Role = P4_Backend_Car_App.Types.Role;


namespace P4_Backend_Car_App.Models
{
    [Index(nameof(NormalizedEmail), IsUnique = true)]
    [Index(nameof(NormalizedUsername), IsUnique = true)]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; }

        [Required, MaxLength(100), EmailAddress]
        public string Email { get; set; }

        [Required, MaxLength(100)]
        public string NormalizedEmail { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; }

        [Required, MaxLength(50)]
        public string NormalizedUsername { get; set; }

        [Required, MaxLength(255)]
        [JsonIgnore]
        public string PasswordHash { get; set; }

        [JsonIgnore]
        public Role Role { get; set; } = Role.Customer;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEnd { get; set; }

        [Required]
        public bool IsEmailConfirmed { get; set; } = false;
        public string? EmailVerificationCode { get; set; }
        public DateTime? CodeExpiry { get; set; }

        public DateTime? LastLoginAt { get; set; }

        [MaxLength(45)]
        public string? LastLoginIp { get; set; }

        public bool IsActive { get; set; } = true;

        //public string? RefreshToken { get; set; }
        //public DateTime? RefreshTokenExpiry { get; set; }

        //[Timestamp]
        //public byte[] RowVersion { get; set; }
    }

}
