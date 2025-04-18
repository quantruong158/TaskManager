namespace TaskManager.Api.Models.DTOs
{
    public class TaskStatusHistoryDto
    {
        public int HistoryId { get; set; }
        public TaskDto Task { get; set; } = default!;
        public StatusDto Status { get; set; } = default!;
        public DateTime ChangedAt { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
    }
}