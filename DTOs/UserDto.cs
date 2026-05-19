using P4_Backend_Car_App.Types;

namespace P4_Backend_Car_App.DTOs
{
    public class UserDto
    {
        public string FullName { get; set; }

        public string Email { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public Role Role { get; set; } = Role.Customer;
    }
}