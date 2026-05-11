



public class CarCreateWithImageDto
{
    public string Name { get; set; }
    public int ManufacturerId { get; set; }
    public int EngineCapacityId { get; set; }
    public string FuelType { get; set; }
    public string Transmission { get; set; }
    public decimal Price { get; set; }
    public int Year { get; set; }

    public IFormFile Image { get; set; }   // 👈 important
}