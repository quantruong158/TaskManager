using System.Data;
using Dapper;
using TaskManager.Api.Models;
using TaskManager.Api.Exceptions;
using TaskManager.Api.Models.DTOs;

namespace TaskManager.Api.Services
{
    public interface IAuthService
    {
        Task<User?> AuthenticateAsync(string email, string password);
        Task<User> RegisterAsync(RegisterRequestDto request);
        Task<IEnumerable<string>> GetUserRolesAsync(int userId);
        Task<IEnumerable<string>> GetUserPermissionsAsync(int userId);
        Task<RefreshToken> SaveRefreshTokenAsync(RefreshToken refreshToken);
        Task<(User? User, RefreshToken? RefreshToken)> ValidateRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token, string ipAddress, string? reason = null, string? replacedByToken = null);
        Task RevokeAllUserRefreshTokensAsync(int userId, string ipAddress, string reason);
    }

    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            var user = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Email = @Email AND IsActive = 1",
                new { Email = email },
                _unitOfWork.Transaction);
    
            if (user == null)
                return null;

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            return isPasswordValid ? user : null;
        }

        public async Task<User> RegisterAsync(RegisterRequestDto request)
        {
            // Check if user with the same email already exists
            var existingUser = await _unitOfWork.Connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Email = @Email",
                new { Email = request.Email },
                _unitOfWork.Transaction);

            if (existingUser != null)
                throw new BadRequestException("User with this email already exists");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var now = DateTime.UtcNow;

            // Create new user
            var sql = @"
                INSERT INTO Users (Email, PasswordHash, Name, IsActive, CreatedAt, CreatedBy) 
                VALUES (@Email, @PasswordHash, @Name, 1, @Now, 'System');
                SELECT * FROM Users WHERE UserId = SCOPE_IDENTITY();";

            var parameters = new DynamicParameters();
            parameters.Add("Email", request.Email);
            parameters.Add("PasswordHash", passwordHash);
            parameters.Add("Name", request.Name);
            parameters.Add("Now", now);

            var newUser = await _unitOfWork.Connection.QueryFirstAsync<User>(sql, parameters, _unitOfWork.Transaction);

            // Assign default user role
            await _unitOfWork.Connection.ExecuteAsync(@"
                INSERT INTO UserRoles (UserId, RoleId, CreatedAt, CreatedBy)
                SELECT @UserId, RoleId, @Now, 'System'
                FROM Roles WHERE RoleName = 'User'",
                new { UserId = newUser.UserId, Now = now },
                _unitOfWork.Transaction);

            return newUser;
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
        {
            return await _unitOfWork.Connection.QueryAsync<string>(
                @"SELECT r.RoleName 
                FROM UserRoles ur 
                JOIN Roles r ON ur.RoleId = r.RoleId 
                WHERE ur.UserId = @UserId",
                new { UserId = userId },
                _unitOfWork.Transaction);
        }

        public async Task<IEnumerable<string>> GetUserPermissionsAsync(int userId)
        {
            return await _unitOfWork.Connection.QueryAsync<string>(
                @"SELECT DISTINCT p.PermissionName
                FROM UserRoles ur
                JOIN RolePermissions rp ON ur.RoleId = rp.RoleId
                JOIN Permissions p ON rp.PermissionId = p.PermissionId
                WHERE ur.UserId = @UserId",
                new { UserId = userId },
                _unitOfWork.Transaction);
        }

        public async Task<RefreshToken> SaveRefreshTokenAsync(RefreshToken refreshToken)
        {
            var sql = @"
                INSERT INTO RefreshTokens (UserId, Token, ExpiryDate, Created, CreatedByIp)
                VALUES (@UserId, @Token, @ExpiryDate, @Created, @CreatedByIp);
                SELECT SCOPE_IDENTITY();";

            var tokenId = await _unitOfWork.Connection.ExecuteScalarAsync<int>(sql, new
            {
                refreshToken.UserId,
                refreshToken.Token,
                refreshToken.ExpiryDate,
                refreshToken.Created,
                refreshToken.CreatedByIp
            }, _unitOfWork.Transaction);

            refreshToken.TokenId = tokenId;
            return refreshToken;
        }

        public async Task<(User? User, RefreshToken? RefreshToken)> ValidateRefreshTokenAsync(string token)
        {
            using var connection = _unitOfWork.Connection;
            await connection.OpenAsync();

            var refreshToken = await connection.QueryFirstOrDefaultAsync<RefreshToken>(
                @"SELECT * FROM RefreshTokens WHERE Token = @Token",
                new { Token = token });

            if (refreshToken == null)
                return (null, null);

            var user = await connection.QueryFirstOrDefaultAsync<User>(
                @"SELECT * FROM Users WHERE UserId = @UserId",
                new { refreshToken.UserId });

            return (user, refreshToken);
        }

        public async Task RevokeRefreshTokenAsync(string token, string ipAddress, string? reason = null, string? replacedByToken = null)
        {
            var sql = @"
                UPDATE RefreshTokens
                SET Revoked = @Now,
                    RevokedByIp = @IpAddress,
                    ReasonRevoked = @Reason,
                    ReplacedByToken = @ReplacedByToken
                WHERE Token = @Token AND Revoked IS NULL";

            await _unitOfWork.Connection.ExecuteAsync(sql, new
            {
                Now = DateTime.UtcNow,
                IpAddress = ipAddress,
                Reason = reason,
                ReplacedByToken = replacedByToken,
                Token = token
            }, _unitOfWork.Transaction);
        }

        public async Task RevokeAllUserRefreshTokensAsync(int userId, string ipAddress, string reason)
        {
            var sql = @"
                UPDATE RefreshTokens
                SET Revoked = @Now,
                    RevokedByIp = @IpAddress,
                    ReasonRevoked = @Reason
                WHERE UserId = @UserId AND Revoked IS NULL";

            await _unitOfWork.Connection.ExecuteAsync(sql, new
            {
                Now = DateTime.UtcNow,
                IpAddress = ipAddress,
                Reason = reason,
                UserId = userId
            }, _unitOfWork.Transaction);
        }
    }
}