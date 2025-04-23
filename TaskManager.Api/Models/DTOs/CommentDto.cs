namespace TaskManager.Api.Models.DTOs
{
    public class CreateCommentRequestDto
    {
        public int TaskId { get; set; }
        public required string Content { get; set; }
    }

    public class CommentResponseDto
    {
        public int CommentId { get; set; }
        public int TaskId { get; set; }
        public required string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public UserCommentDto User { get; set; } = default!;
    }

    public class UserCommentDto
    {
        public int UserId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
    }
}