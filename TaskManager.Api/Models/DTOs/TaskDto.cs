namespace TaskManager.Api.Models.DTOs
{
    public class TaskDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public class TaskResponseDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string Priority { get; set; } = string.Empty;
        public StatusDto Status { get; set; } = default!;

        // Assignee information
        public int? AssignedTo { get; set; }
        public string AssigneeName { get; set; } = string.Empty;

        // Audit information
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class CreateTaskRequestDto
    {
        public required string Title { get; set; }
        public string Description { get; set; } = string.Empty;
        public required string Priority { get; set; }
        public int StatusId { get; set; }
        public int AssignedTo { get; set; }
    }

    public class UpdateTaskRequestDto
    {
        public required string Title { get; set; }
        public string Description { get; set; } = string.Empty;
        public required string Priority { get; set; }
        public int? AssignedTo { get; set; }
    }

    public class ChangeTaskStatusRequestDto
    {
        public required int NewStatusId { get; set; }
    }

    public class TaskCountByStatusDto
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int TaskCount { get; set; }
    }

    public class  TaskCountByStatusResponseDto
    {
        public int TotalCount { get; set; }
        public IEnumerable<TaskCountByStatusDto> TaskCounts { get; set; } = [];
    }
    public class TaskCountByPriorityDto
    {
        public string Priority { get; set; } = string.Empty;
        public int TaskCount { get; set; }
    }
    
    public class TaskCountByPriorityResponseDto
    {
        public int TotalCount { get; set; }
        public IEnumerable<TaskCountByPriorityDto> TaskCounts { get; set; } = [];
    }
}
