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

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {

            var result = await _userService.GetAllUsersWithRolesAsync();
            return Ok(result);
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
            var user = new User
            {
                Email = request.Email,
                Name = request.Name,
                PasswordHash = "",
                CreatedBy = User.GetUserId()
            };

            var userId = await _userService.CreateUserAsync(user, request.Password, request.RoleIds);
            return Ok(userId);
        }

        [HttpPut("{id}")]
        [Authorize("AdminOnly")]
        public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            var user = new User
            {
                Email = request.Email,
                Name = request.Name,
                PasswordHash = "",
                IsActive = request.IsActive,
                UpdatedBy = User.GetUserId()
            };

            await _userService.UpdateUserAsync(id, user);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize("AdminOnly")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }
    }

    public class CreateUserRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Name { get; set; }
        public required List<int> RoleIds { get; set; } = [];
    }

    public class UpdateUserRequest
    {
        public required string Email { get; set; }
        public required string Name { get; set; }
        public bool IsActive { get; set; }
    }
}