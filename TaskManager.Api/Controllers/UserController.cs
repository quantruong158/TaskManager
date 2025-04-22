using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Models;
using TaskManager.Api.Models.DTOs;
using TaskManager.Api.Security;
using TaskManager.Api.Services;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;

        public UserController(IUserService userService, IUnitOfWork unitOfWork)
        {
            _userService = userService;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _userService.GetAllUsersWithRolesAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(user);
        }

        [HttpPost]
        [Authorize("AdminOnly")]
        public async Task<ActionResult<int>> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                await _unitOfWork.BeginAsync();

                var user = new User
                {
                    Email = request.Email,
                    Name = request.Name,
                    PasswordHash = "",
                    CreatedBy = User.GetUserId()
                };

                var userId = await _userService.CreateUserAsync(user, request.Password, request.RoleIds);
                await _unitOfWork.CommitAsync();
                return Ok(userId);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        [HttpPut("{id}")]
        [Authorize("AdminOnly")]
        public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                await _unitOfWork.BeginAsync();

                var user = new User
                {
                    Email = request.Email,
                    Name = request.Name,
PasswordHash = "",
                    IsActive = request.IsActive,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = User.GetUserId()
                };

                await _userService.UpdateUserAsync(id, user);
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
        [Authorize("AdminOnly")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {
                await _unitOfWork.BeginAsync();
                await _userService.DeleteUserAsync(id);
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

    public class CreateUserRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Name { get; set; }
        public List<int> RoleIds { get; set; } = new List<int>();
    }

    public class UpdateUserRequest
    {
        public required string Email { get; set; }
        public required string Name { get; set; }
        public bool IsActive { get; set; }
    }
}