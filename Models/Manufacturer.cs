using System.ComponentModel.DataAnnotations;

namespace P4_Backend_Car_App.Models
{
    public class Manufacturer
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public string NormalizedName { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Car> Cars { get; set; } = new List<Car>();
    }
}