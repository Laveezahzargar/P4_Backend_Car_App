

using P4_Backend_Car_App.Types;

namespace P4_Backend_Car_App.DTOs.User
{
    public class AdminUserUpdateDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public Role Role { get; set; }
        public bool IsActive { get; set; }
        public string? Password { get; set; }
    }
}
