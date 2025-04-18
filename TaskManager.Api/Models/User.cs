using TaskManager.Api.Common;

namespace TaskManager.Api.Models
{
    public class User : Auditable
    {
        public int UserId { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public required string Name { get; set; }
        public bool IsActive { get; set; }
        public ICollection<UserRole> UserRoles { get; set; } = [];

    }
}
