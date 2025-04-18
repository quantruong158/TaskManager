using TaskManager.Api.Common;

namespace TaskManager.Api.Models;

public class Status : Auditable
{
    public int StatusId { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; }
    public int Order { get; set; }
}
