namespace P4_Backend_Car_App.Interfaces
{
    public interface ITokenService
    {
        public string CreateToken(int userId, string email, string username, int time);

        public int VerifyTokenAndGetId(string token);
    }
}
