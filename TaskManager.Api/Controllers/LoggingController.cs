using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Models;
using TaskManager.Api.Models.DTOs;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/logs")]
    public class LoggingController : ControllerBase
    {
        private readonly ILoggingService _loggingService;
        private readonly IUnitOfWork _unitOfWork;

        public LoggingController(ILoggingService loggingService, IUnitOfWork unitOfWork)
        {
            _loggingService = loggingService;
            _unitOfWork = unitOfWork;
        }

        [HttpGet("task-status")]
        public async Task<ActionResult<IEnumerable<TaskStatusHistoryDto>>> GetTaskStatusHistory()
        {
            var history = await _loggingService.GetTaskStatusHistoryAsync();
            return Ok(history);
        }

        [HttpGet("activity")]
        [Authorize("AdminOnly")]
        public async Task<ActionResult<IEnumerable<ActivityLogResponseDto>>> GetActivityLogs()
        {
            var logs = await _loggingService.GetActivityLogsAsync();
            return Ok(logs);
        }

        [HttpGet("login")]
        public async Task<ActionResult<IEnumerable<LoginLog>>> GetLoginLogs()
        {
            var logs = await _loggingService.GetLoginLogsAsync();
            return Ok(logs);
        }
    }
}