namespace TaskManager.Api.Models
{
    public class Role
    {
        public int RoleId { get; set; }
        public required string RoleName { get; set; }
        public string Description { get; set; } = string.Empty;

        // Navigation property for UserRoles
        public ICollection<UserRole> UserRoles { get; set; } = [];

        // Navigation property for RolePermissions
        public ICollection<RolePermission> RolePermissions { get; set; } = [];
    }
}
