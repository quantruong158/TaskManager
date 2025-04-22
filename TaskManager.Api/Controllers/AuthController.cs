using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Models.Auth;
using TaskManager.Api.Services;
using TaskManager.Api.Models;
using TaskManager.Api.Exceptions;

namespace TaskManager.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILoggingService _loggingService;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public AuthController(
            IAuthService authService, 
            ITokenService tokenService, 
            IConfiguration configuration, 
            ILoggingService loggingService,
            IUnitOfWork unitOfWork)
        {
            _authService = authService;
            _tokenService = tokenService;
            _configuration = configuration;
            _loggingService = loggingService;
            _unitOfWork = unitOfWork;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                await _unitOfWork.BeginAsync();

                var log = new LoginLog
                {
                    LogId = 0,
                    Email = request.Email,
                    IsSuccess = false,
                    AttemptIp = GetIpAddress(),
                    Timestamp = DateTime.UtcNow,
                    UserAgent = GetUserAgent()
                };

                var user = await _authService.AuthenticateAsync(request.Email, request.Password);

                if (user == null)
                {
                    await _loggingService.LogLoginAttemptAsync(log);
                    throw new UnauthorizedException("Invalid email or password");
                }

                var roles = await _authService.GetUserRolesAsync(user.UserId);
                var permissions = await _authService.GetUserPermissionsAsync(user.UserId);

                // Log successful login
                log.IsSuccess = true;
                await _loggingService.LogLoginAttemptAsync(log);

                // Generate tokens
                var token = _tokenService.GenerateToken(user, roles, permissions);
                var ipAddress = GetIpAddress();
                var refreshToken = _tokenService.GenerateRefreshToken(user.UserId, ipAddress);
                
                // Save refresh token to database
                await _authService.SaveRefreshTokenAsync(refreshToken);

                await _unitOfWork.CommitAsync();

                // Get token expiration in seconds
                int tokenExpirationInMinutes = _configuration.GetSection("JwtSettings").GetValue<int>("DurationInMinutes");
                int expiresIn = tokenExpirationInMinutes * 60;

                return Ok(new AuthResponse
                {
                    AccessToken = token,
                    RefreshToken = refreshToken.Token,
                    ExpiresIn = expiresIn
                });
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                await _unitOfWork.BeginAsync();
                await _authService.RegisterAsync(request);
                await _unitOfWork.CommitAsync();

                return Ok(new RegisterResponse
                {
                    Success = true,
                    Message = "Registration successful"
                });
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                await _unitOfWork.BeginAsync();
                var ipAddress = GetIpAddress();

                // Validate refresh token
                var (user, refreshToken) = await _authService.ValidateRefreshTokenAsync(request.RefreshToken);

                if (user == null || refreshToken == null)
                    throw new UnauthorizedException("Invalid token");

                // Get user roles and permissions
                var roles = await _authService.GetUserRolesAsync(user.UserId);
                var permissions = await _authService.GetUserPermissionsAsync(user.UserId);

                // Generate new tokens
                var newAccessToken = _tokenService.GenerateToken(user, roles, permissions);
                var newRefreshToken = _tokenService.GenerateRefreshToken(user.UserId, ipAddress);

                // Save new refresh token and revoke old one
                await _authService.SaveRefreshTokenAsync(newRefreshToken);
                await _authService.RevokeRefreshTokenAsync(
                    refreshToken.Token,
                    ipAddress,
                    "Replaced by new token",
                    newRefreshToken.Token);

                await _unitOfWork.CommitAsync();

                // Get token expiration in seconds
                int tokenExpirationInMinutes = _configuration.GetSection("JwtSettings").GetValue<int>("DurationInMinutes");
                int expiresIn = tokenExpirationInMinutes * 60;

                return Ok(new AuthResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken.Token,
                    ExpiresIn = expiresIn
                });
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                await _unitOfWork.BeginAsync();
                var ipAddress = GetIpAddress();

                // Validate and retrieve the refresh token
                var (user, refreshToken) = await _authService.ValidateRefreshTokenAsync(request.RefreshToken);

                if (user == null || refreshToken == null)
                    throw new NotFoundException("Token not found");

                // Verify that the token belongs to the current user
                var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

                if (refreshToken.UserId != currentUserId && !User.IsInRole("Admin"))
                    throw new ForbiddenException("You are not authorized to revoke this token");

                // Revoke token
                await _authService.RevokeRefreshTokenAsync(refreshToken.Token, ipAddress, "Revoked without replacement");
                await _unitOfWork.CommitAsync();

                return Ok(new { message = "Token revoked" });
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        // Helper method to get IP address
        private string GetIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "unknown";
        }

        private string GetUserAgent()
        {
            return Request.Headers["User-Agent"].ToString();
        }
    }
}