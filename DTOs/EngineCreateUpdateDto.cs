
using P4_Backend_Car_App.Types;

namespace P4_Backend_Car_App.DTOs;

public class EngineCreateUpdateDto
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Capacity { get; set; } = string.Empty;
}