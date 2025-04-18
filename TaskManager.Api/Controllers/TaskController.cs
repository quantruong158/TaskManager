using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Models;
using TaskManager.Api.Models.DTOs;
using TaskManager.Api.Services;
using TaskManager.Api.Exceptions;
using System.Security.Claims;
using TaskManager.Api.Security;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
        
        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetAllTasks()
        {
            var tasks = await _taskService.GetAllTasksAsync();
            return Ok(tasks);
        }

        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetMyTasks()
        {
            var userId = User.GetUserId();
            var tasks = await _taskService.GetMyTasksAsync(userId);
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskResponseDto>> GetTaskById(int id)
        {            
            var task = await _taskService.GetTaskByIdAsync(id);
            return Ok(task);
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateTask([FromBody] CreateTaskRequest request)
        {
            var userId = User.GetUserId();
            var task = new WorkTask
            {
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                StatusId = request.StatusId,
                AssignedTo = request.AssignedTo
            };

            var taskId = await _taskService.CreateTaskAsync(task, userId);
            return Ok(taskId);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateTask(int id, [FromBody] UpdateTaskRequest request)
        {
            var userId = User.GetUserId();
            var task = new WorkTask
            {
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                AssignedTo = request.AssignedTo
            };

            await _taskService.UpdateTaskAsync(id, task, userId);
            return NoContent();
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult> ChangeStatus(int id, [FromBody] ChangeTaskStatusRequest request)
        {
            var userId = User.GetUserId();

            await _taskService.ChangeTaskStatusAsync(id, request.NewStatusId, userId);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTask(int id)
        {
            await _taskService.DeleteTaskAsync(id);
            return NoContent();
        }
    }

    public class CreateTaskRequest
    {
        public required string Title { get; set; }
        public string Description { get; set; } = string.Empty;
        public required string Priority { get; set; }
        public int StatusId { get; set; }
        public int AssignedTo { get; set; }
    }

    public class UpdateTaskRequest
    {
        public required string Title { get; set; }
        public string Description { get; set; } = string.Empty;
        public required string Priority { get; set; }
        public int? AssignedTo { get; set; }
    }

    public class ChangeTaskStatusRequest
    {
        public required int NewStatusId { get; set; }
    }
}