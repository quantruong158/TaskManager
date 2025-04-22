using Dapper;
using TaskManager.Api.Models;

namespace TaskManager.Api.Services
{
    public interface ICommentService
    {
        Task<IEnumerable<Comment>> GetCommentsByTaskIdAsync(int taskId);
        Task<Comment?> GetCommentByIdAsync(int commentId);
        Task<Comment> CreateCommentAsync(Comment comment);
        Task<bool> UpdateCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(int commentId);
    }

    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CommentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Comment>> GetCommentsByTaskIdAsync(int taskId)
        {
            const string sql = @"
                SELECT c.*, u.*, t.*
                FROM Comments c
                INNER JOIN Users u ON c.UserId = u.UserId
                INNER JOIN Tasks t ON c.TaskId = t.TaskId
                WHERE c.TaskId = @TaskId
                ORDER BY c.CreatedAt ASC";

            var comments = await _unitOfWork.Connection.QueryAsync<Comment, User, WorkTask, Comment>(
                sql,
                (comment, user, task) =>
                {
                    comment.User = user;
                    comment.Task = task;
                    return comment;
                },
                new { TaskId = taskId },
                splitOn: "UserId,TaskId"
            );

            return comments;
        }

        public async Task<Comment?> GetCommentByIdAsync(int commentId)
        {
            const string sql = @"
                SELECT c.*, u.*, t.*
                FROM Comments c
                INNER JOIN Users u ON c.UserId = u.UserId
                INNER JOIN Tasks t ON c.TaskId = t.TaskId
                WHERE c.CommentId = @CommentId";

            var comments = await _unitOfWork.Connection.QueryAsync<Comment, User, WorkTask, Comment>(
                sql,
                (comment, user, task) =>
                {
                    comment.User = user;
                    comment.Task = task;
                    return comment;
                },
                new { CommentId = commentId },
                splitOn: "UserId,TaskId"
            );

            return comments.FirstOrDefault();
        }

        public async Task<Comment> CreateCommentAsync(Comment comment)
        {
            const string sql = @"
                INSERT INTO Comments (TaskId, UserId, Content, CreatedAt, CreatedBy)
                VALUES (@TaskId, @UserId, @Content, @CreatedAt, @CreatedBy);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            var id = await _unitOfWork.Connection.QuerySingleAsync<int>(sql, comment, _unitOfWork.Transaction);
            comment.CommentId = id;
            return comment;
        }

        public async Task<bool> UpdateCommentAsync(Comment comment)
        {
            const string sql = @"
                UPDATE Comments 
                SET Content = @Content,
                    UpdatedAt = @UpdatedAt,
                    UpdatedBy = @UpdatedBy
                WHERE CommentId = @CommentId";

            var affected = await _unitOfWork.Connection.ExecuteAsync(sql, comment, _unitOfWork.Transaction);
            return affected > 0;
        }

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            const string sql = "DELETE FROM Comments WHERE CommentId = @CommentId";

            var affected = await _unitOfWork.Connection.ExecuteAsync(sql, new { CommentId = commentId }, _unitOfWork.Transaction);
            return affected > 0;
        }
    }
}