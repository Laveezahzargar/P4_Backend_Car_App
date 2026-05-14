using CloudinaryDotNet.Actions;


using Role = P4_Backend_Car_App.Types.Role;

namespace P4_Backend_Car_App.Interfaces
{
    public interface ITokenService
    {
        public string CreateToken(int userId, string email, string username, Role Role ,int time);
        public int VerifyTokenAndGetId(string token);
    }
}
