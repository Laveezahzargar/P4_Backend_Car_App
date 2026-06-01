namespace P4_Backend_Car_App.Models
{
    public class EngineCapacity
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string NormalizedName { get; set; } = string.Empty;

        public int CapacityCc { get; set; }   // ✅ better than string

        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public List<Car> Cars { get; set; } = new();
    }
}