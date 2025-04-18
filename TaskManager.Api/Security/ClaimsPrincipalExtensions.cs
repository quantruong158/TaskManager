using System.Security.Claims;

namespace TaskManager.Api.Security
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal principal)
        {
            var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out int userId) ? userId : 0;
        }

        public static string GetUserEmail(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        }

        public static string GetUserName(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
        }

        public static bool HasPermission(this ClaimsPrincipal principal, string permission)
        {
            return principal.HasClaim(c => c.Type == "permission" && c.Value == permission);
        }

        public static bool HasAnyPermission(this ClaimsPrincipal principal, params string[] permissions)
        {
            return permissions.Any(permission => principal.HasPermission(permission));
        }

        public static bool HasAllPermissions(this ClaimsPrincipal principal, params string[] permissions)
        {
            return permissions.All(permission => principal.HasPermission(permission));
        }

        public static bool IsAdmin(this ClaimsPrincipal principal)
        {
            return principal.IsInRole("Admin");
        }
    }
}