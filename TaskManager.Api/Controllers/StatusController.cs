using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Models;
using TaskManager.Api.Security;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/status")]
public class StatusController : ControllerBase
{
    private readonly IStatusService _statusService;
    public StatusController(IStatusService statusService)
    {
        _statusService = statusService;
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Status>>> GetStatuses()
    {
        var statuses = await _statusService.GetAllStatusesAsync();
        return Ok(statuses);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<Status>> GetStatus(int id)
    {
        var status = await _statusService.GetStatusByIdAsync(id);
        return Ok(status);
    }

    [Authorize("AdminOnly")]
    [HttpPost]
    public async Task<ActionResult<int>> CreateStatus([FromBody] CreateStatusRequest req)
    {
        var status = new Status
        {
            Name = req.Name,
            IsActive = req.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = User.GetUserId()
        };

        var statusId = await _statusService.CreateStatusAsync(status);
        return Ok(statusId);
    }

    [Authorize("AdminOnly")]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest req)
    {
        var status = new Status
        {
            StatusId = id,
            Name = req.Name,
            IsActive = req.IsActive,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = User.GetUserId()
        };

        await _statusService.UpdateStatusAsync(id, status);
        return NoContent();
    }

    [Authorize("AdminOnly")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteStatus(int id)
    {
        await _statusService.DeleteStatusAsync(id);
        return NoContent();
    }

    public class CreateStatusRequest
    {
        public required string Name { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateStatusRequest
    {
        public required string Name { get; set; }
        public bool IsActive { get; set; }
    }
}
