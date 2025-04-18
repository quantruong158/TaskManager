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
    }

    public class TaskService : ITaskService
    {
        private readonly IDatabase _database;

        public TaskService(IDatabase database)
        {
            _database = database;
        }

        public async Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync()
        {
            using var connection = _database.CreateConnection();
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
            using var connection = _database.CreateConnection();
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
            using var connection = _database.CreateConnection();
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
            }, splitOn: "StatusId", param: new
            {
                TaskId = id
            })).FirstOrDefault();

            if (task == null)
                throw new NotFoundException($"Task with ID {id} not found");

            return task;
        }

        public async Task<int> CreateTaskAsync(WorkTask task, int userId)
        {
            task.AssignedTo = null;
            using var connection = _database.CreateConnection();
            
            if (connection.State == ConnectionState.Closed)
                connection.Open();
                
            using var transaction = connection.BeginTransaction();
            
            try
            {
                var sql = "INSERT INTO Tasks (Title, Description, Priority, StatusId, AssignedTo, CreatedAt, CreatedBy) " +
                         "VALUES (@Title, @Description, @Priority, @StatusId, @AssignedTo, @Now, @CreatedBy);" +
                         "SELECT CAST(SCOPE_IDENTITY() as int)";
                Console.WriteLine(userId);
                var parameters = new { 
                    task.Title, 
                    task.Description, 
                    task.Priority,
                    StatusId = 1,
                    task.AssignedTo,
                    Now = DateTime.UtcNow,
                    CreatedBy = userId
                };
                
                var taskId = await connection.ExecuteScalarAsync<int>(sql, parameters, transaction);
                
                if (taskId == 0)
                    throw new AppException("Failed to create task");
                
                transaction.Commit();
                return taskId;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new AppException("Failed to create task", ex);
            }
        }

        public async Task UpdateTaskAsync(int id, WorkTask task, int userId)
        {
            using var connection = _database.CreateConnection();
            
            if (connection.State == ConnectionState.Closed)
                connection.Open();
                
            using var transaction = connection.BeginTransaction();
            
            try
            {
                // Check if task exists and get current state
                var existingTask = await connection.QueryFirstOrDefaultAsync<WorkTask>(
                    "SELECT * FROM Tasks WHERE TaskId = @TaskId",
                    new { TaskId = id },
                    transaction);
                
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
                
                await connection.ExecuteAsync(updateSql, parameters, transaction);
                
                transaction.Commit();
            }
            catch (Exception ex) when (!(ex is NotFoundException))
            {
                transaction.Rollback();
                throw new AppException("Failed to update task", ex);
            }
        }

        public async Task DeleteTaskAsync(int id)
        {
            using var connection = _database.CreateConnection();
            var result = await connection.ExecuteAsync(
                "DELETE FROM Tasks WHERE TaskId = @Id",
                new { Id = id });

            if (result == 0)
                throw new NotFoundException($"Task with ID {id} not found");
        }

        public async Task ChangeTaskStatusAsync(int id, int newStatusId, int userId)
        {
            using var connection = _database.CreateConnection();
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            try
            {
                var existingTask = await connection.ExecuteScalarAsync<int?>(
                    "SELECT 1 FROM Tasks WHERE TaskId = @TaskId",
                    new { TaskId = id },
                    transaction);

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

                await connection.ExecuteAsync(sql, parameters, transaction);

                var historySql = @"INSERT INTO Task_Status_History (TaskId, StatusId, ChangedAt, ChangedBy)
                                   VALUES (@TaskId, @StatusId, @ChangedAt, @ChangedBy)";

                var historyParameters = new
                {
                    TaskId = id,
                    StatusId = newStatusId,
                    ChangedAt = DateTime.UtcNow,
                    ChangedBy = userId
                };

                await connection.ExecuteAsync(historySql, historyParameters, transaction);
                transaction.Commit();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

        }
    }
}