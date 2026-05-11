

namespace P4_Backend_Car_App.Interfaces;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(IFormFile image, string folderName);
}