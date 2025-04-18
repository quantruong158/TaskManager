namespace TaskManager.Api.Models.Auth
{
    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class RegisterRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Name { get; set; }
    }

    public class AuthResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; } // Token expiration time in seconds
    }

    public class RefreshTokenRequest
    {
        public required string RefreshToken { get; set; }
    }
    
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}