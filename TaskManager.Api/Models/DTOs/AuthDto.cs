namespace TaskManager.Api.Models.DTOs
{
    public class LoginRequestDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class RegisterRequestDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Name { get; set; }
    }

    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; } // Token expiration time in seconds
    }

    public class RefreshTokenRequestDto
    {
        public required string RefreshToken { get; set; }
    }

    public class RegisterResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}
