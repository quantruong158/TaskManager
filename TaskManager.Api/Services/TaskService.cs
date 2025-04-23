using Dapper;
using TaskManager.Api.Models;
using TaskManager.Api.Models.DTOs;
using TaskManager.Api.Exceptions;
using System.Data;

namespace TaskManager.Api.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync();
        Task<IEnumerable<TaskResponseDto>> GetMyTasksAsync(int userId);
        Task<TaskResponseDto> GetTaskByIdAsync(int id);
        Task<int> CreateTaskAsync(WorkTask task, int userId);
        Task UpdateTaskAsync(int id, WorkTask task, int userId);
        Task DeleteTaskAsync(int id);
        Task ChangeTaskStatusAsync(int id, int newStatusId, int userId);
        Task<int> GetTotalNumberOfTasks();
        Task<TaskCountResponseDto> GetNumberOfTasksOfEachStatus();
    }

    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;

        public TaskService(IUnitOfWork unitOfWork, ILoggingService loggingService)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
        }

        public async Task<TaskCountResponseDto> GetNumberOfTasksOfEachStatus()
        {
            using var connection = _unitOfWork.Connection;
            await connection.OpenAsync();
            var sql = @"
                SELECT s.StatusId, s.Name as StatusName, COUNT(t.TaskId) as TaskCount
                FROM Statuses s
                LEFT JOIN Tasks t ON t.StatusId = s.StatusId
                GROUP BY s.StatusId, s.Name, s.[Order]
                ORDER BY s.[Order]";
            var taskCounts = await connection.QueryAsync<TaskCountByStatusDto>(sql);
            return new TaskCountResponseDto { TotalCount = taskCounts.Sum(t => t.TaskCount), TaskCounts = taskCounts };
        }

        public async Task<int> GetTotalNumberOfTasks()
        {
            using var connection = _unitOfWork.Connection;
            await connection.OpenAsync();

            var sql = "SELECT COUNT(*) FROM Tasks";
            var totalCount = await connection.ExecuteScalarAsync<int>(sql);
            return totalCount;
        }

        public async Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync()
        {
            using var connection = _unitOfWork.Connection;
            await connection.OpenAsync();

            return await connection.QueryAsync<TaskResponseDto, StatusDto, TaskResponseDto>("""
                SELECT 
                    t.TaskId,
                    t.Title,
                    t.Description,
                    t.Priority,
                    t.AssignedTo,
                    u.Name as AssigneeName,
                    t.CreatedAt,
                    creator.Name as CreatedBy,
                    t.UpdatedAt,
                    updater.Name as UpdatedBy,
                    s.StatusId,
                    s.Name as StatusName
                FROM Tasks t
                LEFT JOIN Statuses s ON t.StatusId = s.StatusId
                LEFT JOIN Users u ON t.AssignedTo = u.UserId
                LEFT JOIN Users creator ON t.CreatedBy = creator.UserId
                LEFT JOIN Users updater ON t.UpdatedBy = updater.UserId
                """, (taskDto, statusDto) =>
            {
                taskDto.Status = statusDto;
                return taskDto;
            }, splitOn: "StatusId");
        }

        public async Task<IEnumerable<TaskResponseDto>> GetMyTasksAsync(int userId)
        {
            using var connection = _unitOfWork.Connection;
            await connection.OpenAsync();

            return await connection.QueryAsync<TaskResponseDto>("""
                SELECT 
                    t.TaskId,
                    t.Title,
                    t.Description,
                    t.Priority,
                    t.AssignedTo,
                    u.Name as AssigneeName,
                    s.StatusId,
                    s.Name as Status,
                    t.CreatedAt,
                    creator.Name as CreatedBy,
                    t.UpdatedAt,
                    updater.Name as UpdatedBy
                FROM Tasks t
                LEFT JOIN Statuses s ON t.StatusId = s.StatusId
                LEFT JOIN Users u ON t.AssignedTo = u.UserId
                LEFT JOIN Users creator ON t.CreatedBy = creator.UserId
                LEFT JOIN Users updater ON t.UpdatedBy = updater.UserId
                WHERE t.AssignedTo = @UserId
                """, new { UserId = userId });
        }

        public async Task<TaskResponseDto> GetTaskByIdAsync(int id)
        {
            using var connection = _unitOfWork.Connection;
            await connection.OpenAsync();

            var task = (await connection.QueryAsync<TaskResponseDto, StatusDto, TaskResponseDto>("""
                SELECT 
                    t.TaskId,
                    t.Title,
                    t.Description,
                    t.Priority,
                    t.AssignedTo,
                    u.Name as AssigneeName,
                    t.CreatedAt,
                    creator.Name as CreatedBy,
                    t.UpdatedAt,
                    updater.Name as UpdatedBy,
                    s.StatusId,
                    s.Name as StatusName
                FROM Tasks t
                LEFT JOIN Statuses s ON t.StatusId = s.StatusId
                LEFT JOIN Users u ON t.AssignedTo = u.UserId
                LEFT JOIN Users creator ON t.CreatedBy = creator.UserId
                LEFT JOIN Users updater ON t.UpdatedBy = updater.UserId
                WHERE t.TaskId = @TaskId
                """, (taskDto, statusDto) =>
            {
                taskDto.Status = statusDto;
                return taskDto;
            }, splitOn: "StatusId", param: new { TaskId = id })).FirstOrDefault();

            if (task == null)
                throw new NotFoundException($"Task with ID {id} not found");

            return task;
        }

        public async Task<int> CreateTaskAsync(WorkTask task, int userId)
        {
            var sql = "INSERT INTO Tasks (Title, Description, Priority, StatusId, CreatedAt, CreatedBy) " +
                     "VALUES (@Title, @Description, @Priority, @StatusId, @Now, @CreatedBy);" +
                     "SELECT CAST(SCOPE_IDENTITY() as int)";

            var parameters = new { 
                task.Title, 
                task.Description, 
                task.Priority,
                StatusId = 1,
                Now = DateTime.UtcNow,
                CreatedBy = userId
            };

            var taskId = await _unitOfWork.Connection.ExecuteScalarAsync<int>(sql, parameters, _unitOfWork.Transaction);

            await _loggingService.LogTaskStatusChangeAsync(taskId, 1, userId);
            
            if (taskId == 0)
                throw new AppException("Failed to create task");

            return taskId;
        }

        public async Task UpdateTaskAsync(int id, WorkTask task, int userId)
        {
            // Check if task exists and get current state
            var existingTask = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<WorkTask>(
                "SELECT * FROM Tasks WHERE TaskId = @TaskId",
                new { TaskId = id },
                _unitOfWork.Transaction);
            
            if (existingTask == null)
                throw new NotFoundException($"Task with ID {id} not found");
                
            // Update task
            var updateSql = @"
                UPDATE Tasks SET 
                    Title = @Title, 
                    Description = @Description, 
                    Priority = @Priority,
                    AssignedTo = @AssignedTo, 
                    UpdatedAt = @Now,
                    UpdatedBy = @UpdatedBy
                WHERE TaskId = @TaskId";
            
            var parameters = new 
            {
                TaskId = id,
                task.Title, 
                task.Description, 
                task.Priority,
                task.AssignedTo,
                Now = DateTime.UtcNow,
                UpdatedBy = userId
            };
            
            await _unitOfWork.Connection.ExecuteAsync(updateSql, parameters, _unitOfWork.Transaction);
        }

        public async Task DeleteTaskAsync(int id)
        {
            var result = await _unitOfWork.Connection.ExecuteAsync(
                "DELETE FROM Tasks WHERE TaskId = @Id",
                new { Id = id },
                _unitOfWork.Transaction);

            if (result == 0)
                throw new NotFoundException($"Task with ID {id} not found");
        }

        public async Task ChangeTaskStatusAsync(int id, int newStatusId, int userId)
        {
            var existingTask = await _unitOfWork.Connection.ExecuteScalarAsync<int?>(
                "SELECT 1 FROM Tasks WHERE TaskId = @TaskId",
                new { TaskId = id },
                _unitOfWork.Transaction);

            if (existingTask == null)
            {
                throw new NotFoundException($"Task with ID {id} not found");
            }

            var sql = @"UPDATE Tasks
                        SET StatusId = @NewStatusId
                        WHERE TaskId = @Id";

            var parameters = new
            {
                NewStatusId = newStatusId,
                Id = id
            };

            await _unitOfWork.Connection.ExecuteAsync(sql, parameters, _unitOfWork.Transaction);
            await _loggingService.LogTaskStatusChangeAsync(id, newStatusId, userId);
        }
    }
}