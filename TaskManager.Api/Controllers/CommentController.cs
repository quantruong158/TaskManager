using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Models;
using TaskManager.Api.Models.DTOs;
using TaskManager.Api.Services;
using TaskManager.Api.Security;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("api/comments")]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ILoggingService _loggingService;
        private readonly IUnitOfWork _unitOfWork;

        public CommentController(ICommentService commentService, ILoggingService loggingService, IUnitOfWork unitOfWork)
        {
            _commentService = commentService;
            _loggingService = loggingService;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CommentResponseDto>>> GetCommentsByTaskId([FromQuery] int? taskId)
        {
            if (taskId is null)
            {
                return BadRequest("Task ID is required.");
            }

            var comments = await _commentService.GetCommentsByTaskIdAsync(taskId.Value);
            return Ok(comments.Select(c => c.ToDto()));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CommentResponseDto>> GetComment(int id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
                return NotFound();

            return Ok(comment.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateComment(CommentRequestDto request)
        {
            try
            {
                await _unitOfWork.BeginAsync();
                
                var comment = new Comment
                {
                    TaskId = request.TaskId,
                    Content = request.Content,
                    UserId = User.GetUserId(),
                    CreatedBy = User.GetUserId(),
                    CreatedAt = DateTime.UtcNow
                };

                var createdComment = await _commentService.CreateCommentAsync(comment);
                
                await _loggingService.LogActivityAsync(new ActivityLog 
                { 
                    UserId = User.GetUserId(),
                    Action = "Create",
                    TargetTable = "Comments",
                    TargetId = createdComment.CommentId,
                    Timestamp = DateTime.UtcNow
                });

                await _unitOfWork.CommitAsync();
                return Ok(createdComment.CommentId);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(int id, CommentRequestDto request)
        {
            try
            {
                await _unitOfWork.BeginAsync();

                var existingComment = await _commentService.GetCommentByIdAsync(id);
                if (existingComment == null)
                    return NotFound();

                if (existingComment.UserId != User.GetUserId())
                    return Forbid();

                existingComment.Content = request.Content;
                existingComment.UpdatedBy = User.GetUserId();
                existingComment.UpdatedAt = DateTime.UtcNow;

                var result = await _commentService.UpdateCommentAsync(existingComment);
                if (!result)
                    return NotFound();

                await _loggingService.LogActivityAsync(new ActivityLog 
                { 
                    UserId = User.GetUserId(),
                    TargetTable = "Comments",
                    Action = "Update",
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
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                await _unitOfWork.BeginAsync();

                var existingComment = await _commentService.GetCommentByIdAsync(id);
                if (existingComment == null)
                    return NotFound();

                if (existingComment.UserId != User.GetUserId())
                    return Forbid();

                var result = await _commentService.DeleteCommentAsync(id);
                if (!result)
                    return NotFound();

                await _loggingService.LogActivityAsync(new ActivityLog 
                { 
                    UserId = User.GetUserId(),
                    TargetTable = "Comments",
                    Action = "Delete",
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