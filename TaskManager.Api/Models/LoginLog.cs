namespace TaskManager.Api.Models;

public class LoginLog
{
    public int LogId { get; set; }
    public required string Email { get; set; }
    public bool IsSuccess { get; set; }
    public DateTime Timestamp { get; set; }
    public required string UserAgent { get; set; }
    public required string AttemptIp { get; set; }
}
