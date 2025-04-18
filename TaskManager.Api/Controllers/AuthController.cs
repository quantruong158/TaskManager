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
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        
        public AuthController(IAuthService authService, ITokenService tokenService, IConfiguration configuration)
        {
            _authService = authService;
            _tokenService = tokenService;
            _configuration = configuration;
        }
        
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
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
                await _authService.LogLoginAttempt(log);
                throw new UnauthorizedException("Invalid email or password");
            }
            var roles = await _authService.GetUserRolesAsync(user.UserId);
            var permissions = await _authService.GetUserPermissionsAsync(user.UserId);

            // Log successful login
            log.IsSuccess = true;
            await _authService.LogLoginAttempt(log);

            // Generate access token
            var token = _tokenService.GenerateToken(user, roles, permissions);
            
            // Generate refresh token
            var ipAddress = GetIpAddress();
            var refreshToken = _tokenService.GenerateRefreshToken(user.UserId, ipAddress);
            
            // Save refresh token to database
            await _authService.SaveRefreshTokenAsync(refreshToken);

            // Get token expiration in seconds
            int tokenExpirationInMinutes = _configuration.GetSection("JwtSettings").GetValue<int>("DurationInMinutes");
            int expiresIn = tokenExpirationInMinutes * 60; // Convert to seconds
            
            return Ok(new AuthResponse
            {
                AccessToken = token,
                RefreshToken = refreshToken.Token,
                ExpiresIn = expiresIn
            });
        }
        
        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
        {
            await _authService.RegisterAsync(request);
            
            return Ok(new RegisterResponse
            {
                Success = true,
                Message = "Registration successful"
            });
        }
        
        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
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
            
            // Save new refresh token
            await _authService.SaveRefreshTokenAsync(newRefreshToken);
            
            // Revoke old refresh token
            await _authService.RevokeRefreshTokenAsync(
                refreshToken.Token, 
                ipAddress, 
                "Replaced by new token", 
                newRefreshToken.Token);

            // Get token expiration in seconds
            int tokenExpirationInMinutes = _configuration.GetSection("JwtSettings").GetValue<int>("DurationInMinutes");
            int expiresIn = tokenExpirationInMinutes * 60; // Convert to seconds
            
            return Ok(new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                ExpiresIn = expiresIn
            });
        }
        
        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
        {
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
            
            return Ok(new { message = "Token revoked" });
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