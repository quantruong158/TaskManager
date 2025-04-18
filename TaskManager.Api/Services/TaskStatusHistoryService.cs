using Dapper;
using TaskManager.Api.Models;
using TaskManager.Api.Models.DTOs;

namespace TaskManager.Api.Services
{
    public interface ITaskStatusHistoryService
    {
        Task<IEnumerable<TaskStatusHistoryDto>> GetTaskStatusHistoryAsync();
    }

    public class TaskStatusHistoryService : ITaskStatusHistoryService
    {
        private readonly IDatabase _db;

        public TaskStatusHistoryService(IDatabase db)
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
    }
}