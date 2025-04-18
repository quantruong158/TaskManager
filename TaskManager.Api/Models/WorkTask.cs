using TaskManager.Api.Common;

namespace TaskManager.Api.Models
{
    public class WorkTask : Auditable
    {
        public int TaskId { get; set; }
        public required string Title { get; set; }
        public string Description { get; set; } = string.Empty;
        public required string Priority { get; set; }
        public int StatusId { get; set; }
        public Status Status { get; set; } = default!;
        public int? AssignedTo { get; set; }
        public User Assignee { get; set; } = default!;
    }
}
