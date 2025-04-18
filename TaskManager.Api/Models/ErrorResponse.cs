namespace TaskManager.Api.Models
{
    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? Type { get; set; }
    }
}