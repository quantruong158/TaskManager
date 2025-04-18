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
        private readonly IDatabase _db;

        public LoggingService(IDatabase db)
        {
            _db = db;
        }

        public async Task<IEnumerable<TaskStatusHistoryDto>> GetTaskStatusHistoryAsync()
        {
            const string sql = @"
                SELECT h.HistoryId, h.ChangedAt, h.ChangedBy, h.TaskId, t.Title,
                       s.StatusId, s.Name as StatusName,
                       u.Name as ChangedByName
                FROM Task_Status_History h
                INNER JOIN Tasks t ON h.TaskId = t.TaskId
                INNER JOIN Statuses s ON h.StatusId = s.StatusId
                INNER JOIN Users u ON h.ChangedBy = u.UserId
                ORDER BY h.ChangedAt DESC";

            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var histories = await conn.QueryAsync<TaskStatusHistoryDto, TaskDto, StatusDto, string, TaskStatusHistoryDto>(
                sql,
                (history, task, status, changedByName) =>
                {
                    history.Task = task;
                    history.Status = status;
                    history.ChangedBy = changedByName;
                    return history;
                },
                splitOn: "TaskId,StatusId,ChangedByName"
            );

            return histories;
        }

        public async Task<IEnumerable<ActivityLog>> GetActivityLogsAsync()
        {
            const string sql = @"
                SELECT l.LogId, l.UserId, l.Action, l.TargetId, l.Timestamp,
                       u.Name as UserName, u.Email
                FROM Activity_Logs l
                INNER JOIN Users u ON l.UserId = u.UserId
                ORDER BY l.Timestamp DESC";

            using var conn = _db.CreateConnection();
            return await conn.QueryAsync<ActivityLog>(sql);
        }

        public async Task<IEnumerable<LoginLog>> GetLoginLogsAsync()
        {
            const string sql = @"
                SELECT LogId, Email, IsSuccess, Timestamp, UserAgent, AttemptIp
                FROM Login_Logs
                ORDER BY Timestamp DESC";

            using var conn = _db.CreateConnection();
            return await conn.QueryAsync<LoginLog>(sql);
        }

        public async Task LogActivityAsync(ActivityLog log)
        {
            const string sql = @"
                INSERT INTO Activity_Logs (UserId, Action, TargetId, Timestamp)
                VALUES (@UserId, @Action, @TargetId, @Timestamp)";

            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync(sql, log);
        }

        public async Task LogLoginAttemptAsync(LoginLog log)
        {
            const string sql = @"
                INSERT INTO Login_Logs (Email, IsSuccess, Timestamp, UserAgent, AttemptIp)
                VALUES (@Email, @IsSuccess, @Timestamp, @UserAgent, @AttemptIp)";

            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync(sql, log);
        }

        public async Task LogTaskStatusChangeAsync(int taskId, int statusId, int changedBy)
        {
            const string sql = @"
                INSERT INTO Task_Status_History (TaskId, StatusId, ChangedAt, ChangedBy)
                VALUES (@TaskId, @StatusId, @ChangedAt, @ChangedBy)";

            using var conn = _db.CreateConnection();
            await conn.ExecuteAsync(sql, new
            {
                TaskId = taskId,
                StatusId = statusId,
                ChangedAt = DateTime.UtcNow,
                ChangedBy = changedBy
            });
        }
    }
}