

namespace P4_Backend_Car_App.DTOs;

    public class CarDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int ManufacturerId { get; set; }

        public string Manufacturer { get; set; } = string.Empty;

        public int EngineCapacityId { get; set; }

        public string EngineCapacity { get; set; } = string.Empty;

        public string FuelType { get; set; } = string.Empty;

        public string Transmission { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Year { get; set; }

        public string? ImageUrl { get; set; }
}