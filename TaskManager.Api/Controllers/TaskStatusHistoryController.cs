using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Models.DTOs;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/task-status-history")]
    public class TaskStatusHistoryController : ControllerBase
    {
        private readonly ITaskStatusHistoryService _historyService;

        public TaskStatusHistoryController(ITaskStatusHistoryService historyService)
        {
            _historyService = historyService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskStatusHistoryDto>>> GetTaskStatusHistory()
        {
            var history = await _historyService.GetTaskStatusHistoryAsync();
            return Ok(history);
        }
    }
}