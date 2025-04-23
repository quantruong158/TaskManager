using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Models.DTOs;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("api/statistics")]
    [Authorize]
    public class StatisticsController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITaskService _taskService;
        public StatisticsController(IUserService userService, ITaskService taskService)
        {
            _userService = userService;
            _taskService = taskService;
        }

        [HttpGet("users/count")]
        public async Task<ActionResult<int>> GetUserCount()
        {
            var count = await _userService.GetTotalNumberOfUsers();
            return Ok(count);
        }

        [HttpGet("tasks/count")]
        public async Task<ActionResult<TaskCountResponseDto>> GetTaskCount()
        {
            var count = await _taskService.GetNumberOfTasksOfEachStatus();

            return Ok(count);
        }
    }
}
