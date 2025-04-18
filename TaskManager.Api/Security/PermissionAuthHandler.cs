using Microsoft.AspNetCore.Authorization;

namespace TaskManager.Api.Security
{
    // Permission requirement
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }

    // Permission handler
    public class PermissionAuthHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            PermissionRequirement requirement)
        {
            // Admin role always has all permissions
            if (context.User.IsInRole("Admin"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Check if user has the specific permission
            if (context.User.HasPermission(requirement.Permission))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    // Custom policy provider
    public static class AuthorizationExtensions
    {
        public static void AddPermissionPolicies(this AuthorizationOptions options)
        {
            // Task-related permissions
            options.AddPolicy("CanViewAllTasks", policy => 
                policy.AddRequirements(new PermissionRequirement("tasks.view_all")));
                
            options.AddPolicy("CanCreateTask", policy => 
                policy.AddRequirements(new PermissionRequirement("tasks.create")));
                
            options.AddPolicy("CanEditTask", policy => 
                policy.AddRequirements(new PermissionRequirement("tasks.edit")));
                
            options.AddPolicy("CanDeleteTask", policy => 
                policy.AddRequirements(new PermissionRequirement("tasks.delete")));
                
            options.AddPolicy("CanAssignTask", policy => 
                policy.AddRequirements(new PermissionRequirement("tasks.assign")));

            // User management permissions
            options.AddPolicy("CanManageUsers", policy => 
                policy.AddRequirements(new PermissionRequirement("users.manage")));
                
            options.AddPolicy("CanAssignRoles", policy => 
                policy.AddRequirements(new PermissionRequirement("roles.assign")));
        }
    }
}