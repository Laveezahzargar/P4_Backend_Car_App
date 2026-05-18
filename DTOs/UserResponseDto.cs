
using P4_Backend_Car_App.Types;

namespace P4_Backend_Car_App.DTOs
{
    public class UserResponseDto
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string Username { get; set; }

        public Role Role { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}