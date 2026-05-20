using CloudinaryDotNet.Actions;
using System.Security.Claims;
using Role = P4_Backend_Car_App.Types.Role;

namespace P4_Backend_Car_App.Interfaces
{
    public interface ITokenService
    {
        public string CreateToken(int userId, string email, string username, Role Role ,int time);
    }
}
