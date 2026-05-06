using Microsoft.VisualBasic.FileIO;
using P4_Backend_Car_App.Types;

namespace P4_Backend_Car_App.Models
{
    public class Car
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int ManufacturerId { get; set; }
        public Manufacturer? Manufacturer { get; set; }

        public int EngineCapacityId { get; set; }
        public EngineCapacity? EngineCapacity { get; set; }

        public FuelType FuelType { get; set; }
        public Transmission Transmission { get; set; }

        public decimal Price { get; set; }

        public int Year { get; set; }
    }
}
