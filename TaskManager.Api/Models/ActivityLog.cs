namespace TaskManager.Api.Models
{
    public class ActivityLog
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = default!;
        public required string Action { get; set; }
        public required string TargetTable { get; set; }
        public int TargetId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
