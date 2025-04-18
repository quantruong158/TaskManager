namespace TaskManager.Api.Models.DTOs
{
    public static class DtoExtensions
    {
        public static StatusDto ToDto(this Status status)
        {
            return new StatusDto
            {
                StatusId = status.StatusId,
                StatusName = status.Name
            };
        }

        public static TaskResponseDto ToDto(this WorkTask task)
        {
            return new TaskResponseDto
            {
                TaskId = task.TaskId,
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority,
                Status = task.Status.ToDto(),
                AssignedTo = task.AssignedTo,
                AssigneeName = task.Assignee?.Name ?? string.Empty,
                CreatedAt = task.CreatedAt,
                CreatedBy = task.CreatedBy.ToString(),
                UpdatedAt = task.UpdatedAt,
                UpdatedBy = task.UpdatedBy.ToString()
            };
        }

        public static RoleDto ToDto(this Role role)
        {
            return new RoleDto
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName
            };
        }

        public static UserDto ToDto(this User user)
        {
            return new UserDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Name = user.Name,
                IsActive = user.IsActive,
                Roles = user.UserRoles?.Select(ur => ur.Role.ToDto()).ToList() ?? new List<RoleDto>(),
                CreatedAt = user.CreatedAt,
                CreatedBy = user.CreatedBy.ToString(),
                UpdatedAt = user.UpdatedAt,
                UpdatedBy = user.UpdatedBy.ToString()
            };
        }
    }
}