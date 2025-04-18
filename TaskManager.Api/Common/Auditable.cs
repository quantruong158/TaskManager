using TaskManager.Api.Models;

namespace TaskManager.Api.Common
{
    public abstract class Auditable
    {
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public User Creator { get; set; } = default!;
        public DateTime UpdatedAt { get; set; }
        public int UpdatedBy { get; set; }
        public User Updater { get; set; } = default!;
    }
}
