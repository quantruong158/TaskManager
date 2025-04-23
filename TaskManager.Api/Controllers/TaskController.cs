using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Models;
using TaskManager.Api.Models.DTOs;
using TaskManager.Api.Services;
using TaskManager.Api.Exceptions;
using TaskManager.Api.Security;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;

        public TaskController(ITaskService taskService, IUnitOfWork unitOfWork, ILoggingService loggingService)
        {
            _taskService = taskService;
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
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
        public async Task<ActionResult<int>> CreateTask([FromBody] CreateTaskRequestDto request)
        {
            try
            {
                await _unitOfWork.BeginAsync();

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

                await _loggingService.LogActivityAsync(new ActivityLog
                {
                    UserId = User.GetUserId(),
                    Action = "Create",
                    TargetTable = "Tasks",
                    TargetId = taskId,
                    Timestamp = DateTime.UtcNow
                });

                await _unitOfWork.CommitAsync();

                return Ok(taskId);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateTask(int id, [FromBody] UpdateTaskRequestDto request)
        {
            try
            {
                await _unitOfWork.BeginAsync();

                var userId = User.GetUserId();
                var task = new WorkTask
                {
                    Title = request.Title,
                    Description = request.Description,
                    Priority = request.Priority,
                    AssignedTo = request.AssignedTo
                };

                await _taskService.UpdateTaskAsync(id, task, userId);

                await _loggingService.LogActivityAsync(new ActivityLog
                {
                    UserId = User.GetUserId(),
                    Action = "Update",
                    TargetTable = "Tasks",
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

        [HttpPut("{id}/status")]
        public async Task<ActionResult> ChangeStatus(int id, [FromBody] ChangeTaskStatusRequestDto request)
        {
            try
            {
                await _unitOfWork.BeginAsync();

                var userId = User.GetUserId();
                await _taskService.ChangeTaskStatusAsync(id, request.NewStatusId, userId);

                await _loggingService.LogActivityAsync(new ActivityLog
                {
                    UserId = User.GetUserId(),
                    Action = "ChangeStatus",
                    TargetTable = "Tasks",
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

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTask(int id)
        {
            try
            {
                await _unitOfWork.BeginAsync();
                await _taskService.DeleteTaskAsync(id);

                await _loggingService.LogActivityAsync(new ActivityLog
                {
                    UserId = User.GetUserId(),
                    Action = "Delete",
                    TargetTable = "Tasks",
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
    }
}