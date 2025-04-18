namespace TaskManager.Api.Models
{
    public class RolePermission
    {
        public int RolePermissionId { get; set; }
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
        public Role Role { get; set; } = default!;
        public Permission Permission { get; set; } = default!;
    }
}
