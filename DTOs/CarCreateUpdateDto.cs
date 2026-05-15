

using P4_Backend_Car_App.Types;

namespace P4_Backend_Car_App.DTOs;

public class CarCreateUpdateDto
{
    public string Name { get; set; } = string.Empty;

    public int ManufacturerId { get; set; }

    public int EngineCapacityId { get; set; }

    public FuelType FuelType { get; set; }

    public Transmission Transmission { get; set; }

    public decimal Price { get; set; }

    public int Year { get; set; }

    public IFormFile? Image { get; set; }
}