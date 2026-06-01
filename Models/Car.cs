namespace P4_Backend_Car_App.Models
{
    public class Car
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        // Relationships
        public int ManufacturerId { get; set; }
        public Manufacturer? Manufacturer { get; set; }

        public int EngineCapacityId { get; set; }
        public EngineCapacity? EngineCapacity { get; set; }

        // Enums
        public FuelType FuelType { get; set; }
        public Transmission Transmission { get; set; }

        // Car Details
        public decimal Price { get; set; }
        public int Year { get; set; }

        public string? ImageUrl { get; set; }

        // 🔥 Add these for consistency with your project
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}