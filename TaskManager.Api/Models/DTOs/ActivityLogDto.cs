namespace TaskManager.Api.Models.DTOs
{
    public class ActivityLogResponseDto
    {
        public int LogId { get; set; }
        public UserLogDto User { get; set; } = default!;
        public required string Action { get; set; }
        public required string TargetTable { get; set; }
        public int TargetId { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class UserLogDto
    {
        public int UserId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
    }
}