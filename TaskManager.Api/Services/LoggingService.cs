using Dapper;
using TaskManager.Api.Models;
using TaskManager.Api.Models.DTOs;

namespace TaskManager.Api.Services
{
    public interface ILoggingService
    {
        Task<IEnumerable<TaskStatusHistoryDto>> GetTaskStatusHistoryAsync();
        Task<IEnumerable<ActivityLog>> GetActivityLogsAsync();
        Task<IEnumerable<LoginLog>> GetLoginLogsAsync();
        Task LogActivityAsync(ActivityLog log);
        Task LogLoginAttemptAsync(LoginLog log);
        Task LogTaskStatusChangeAsync(int taskId, int statusId, int changedBy);
    }

    public class LoggingService : ILoggingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LoggingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<TaskStatusHistoryDto>> GetTaskStatusHistoryAsync()
        {
            using var connection = _unitOfWork.Connection;
            await connection.OpenAsync();

            const string sql = @"
                SELECT h.HistoryId, h.ChangedAt, h.ChangedBy, h.TaskId, t.Title,
                       s.StatusId, s.Name as StatusName,
                       u.Name as ChangedByName
                FROM Task_Status_History h
                INNER JOIN Tasks t ON h.TaskId = t.TaskId
                INNER JOIN Statuses s ON h.StatusId = s.StatusId
                INNER JOIN Users u ON h.ChangedBy = u.UserId
                ORDER BY h.ChangedAt DESC";

            return await connection.QueryAsync<TaskStatusHistoryDto, TaskDto, StatusDto, string, TaskStatusHistoryDto>(
                sql,
                (history, task, status, changedByName) =>
                {
                    history.Task = task;
                    history.Status = status;
                    history.ChangedBy = changedByName;
                    return history;
                },
                splitOn: "TaskId,StatusId,ChangedByName");
        }

        public async Task<IEnumerable<ActivityLog>> GetActivityLogsAsync()
        {
            using var connection = _unitOfWork.Connection;
            await connection.OpenAsync();

            const string sql = @"
                SELECT * FROM Activity_Logs
                ORDER BY Timestamp DESC";

            return await connection.QueryAsync<ActivityLog>(sql);
        }

        public async Task<IEnumerable<LoginLog>> GetLoginLogsAsync()
        {
            using var connection = _unitOfWork.Connection;
            await connection.OpenAsync();

            const string sql = @"
                SELECT * FROM Login_Logs
                ORDER BY Timestamp DESC";

            return await connection.QueryAsync<LoginLog>(sql);
        }

        public async Task LogActivityAsync(ActivityLog log)
        {
            const string sql = @"
                INSERT INTO Activity_Logs (UserId, Action, TargetTable, TargetId, Timestamp)
                VALUES (@UserId, @Action, @TargetTable, @TargetId, @Timestamp)";

            await _unitOfWork.Connection.ExecuteAsync(sql, log, _unitOfWork.Transaction);
        }

        public async Task LogLoginAttemptAsync(LoginLog log)
        {
            const string sql = @"
                INSERT INTO Login_Logs (Email, IsSuccess, Timestamp, UserAgent, AttemptIp)
                VALUES (@Email, @IsSuccess, @Timestamp, @UserAgent, @AttemptIp)";

            await _unitOfWork.Connection.ExecuteAsync(sql, log, _unitOfWork.Transaction);
        }

        public async Task LogTaskStatusChangeAsync(int taskId, int statusId, int changedBy)
        {
            const string sql = @"
                INSERT INTO Task_Status_History (TaskId, StatusId, ChangedAt, ChangedBy)
                VALUES (@TaskId, @StatusId, @ChangedAt, @ChangedBy)";

            await _unitOfWork.Connection.ExecuteAsync(sql, new
            {
                TaskId = taskId,
                StatusId = statusId,
                ChangedAt = DateTime.UtcNow,
                ChangedBy = changedBy
            }, _unitOfWork.Transaction);
        }
    }
}