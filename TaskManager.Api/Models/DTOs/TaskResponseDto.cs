namespace TaskManager.Api.Models.DTOs
{
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
}