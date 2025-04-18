using TaskManager.Api.Common;

namespace TaskManager.Api.Models
{
    public class Comment : Auditable
    {
        public int CommentId { get; set; }
        public int TaskId { get; set; }
        public WorkTask Task { get; set; } = default!;
        public int UserId { get; set; }
        public User User { get; set; } = default!;
        public required string Content { get; set; }

    }
}
