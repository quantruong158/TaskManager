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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILoggingService _loggingService;

    public StatusController(IStatusService statusService, IUnitOfWork unitOfWork, ILoggingService loggingService)
    {
        _statusService = statusService;
        _unitOfWork = unitOfWork;
        _loggingService = loggingService;
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
        try
        {
            await _unitOfWork.BeginAsync();

            var status = new Status
            {
                Name = req.Name,
                IsActive = req.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.GetUserId()
            };

            var statusId = await _statusService.CreateStatusAsync(status);

            await _loggingService.LogActivityAsync(new ActivityLog 
            { 
                UserId = User.GetUserId(),
                Action = "Create",
                TargetTable = "Statuses",
                TargetId = statusId,
                Timestamp = DateTime.UtcNow
            });

            await _unitOfWork.CommitAsync();
            return Ok(statusId);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    [Authorize("AdminOnly")]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest req)
    {
        try
        {
            await _unitOfWork.BeginAsync();

            var status = new Status
            {
                StatusId = id,
                Name = req.Name,
                IsActive = req.IsActive,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = User.GetUserId()
            };

            await _statusService.UpdateStatusAsync(id, status);

            await _loggingService.LogActivityAsync(new ActivityLog 
            { 
                UserId = User.GetUserId(),
                Action = "Update",
                TargetTable = "Statuses",
                TargetId = id,
                Timestamp = DateTime.UtcNow
            });

            await _unitOfWork.CommitAsync();
            return NoContent();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    [Authorize("AdminOnly")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteStatus(int id)
    {
        try
        {
            await _unitOfWork.BeginAsync();
            await _statusService.DeleteStatusAsync(id);

            await _loggingService.LogActivityAsync(new ActivityLog 
            { 
                UserId = User.GetUserId(),
                Action = "Delete",
                TargetTable = "Statuses",
                TargetId = id,
                Timestamp = DateTime.UtcNow
            });
            
            await _unitOfWork.CommitAsync();
            return NoContent();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
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
