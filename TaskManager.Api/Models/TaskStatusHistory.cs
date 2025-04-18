namespace TaskManager.Api.Models
{
    public class TaskStatusHistory
    {
        public int HistoryId { get; set; }
        public int TaskId { get; set; }
        public int StatusId { get; set; }
        public Status Status { get; set; } = default!;
        public DateTime ChangedAt { get; set; }
        public int ChangedBy { get; set; }
        public User Changer { get; set; } = default!;
    }
}
