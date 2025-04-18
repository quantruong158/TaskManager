using System.Data;
using Dapper;
using TaskManager.Api.Models;
using TaskManager.Api.Models.Auth;
using TaskManager.Api.Exceptions;

namespace TaskManager.Api.Services
{
    public interface IAuthService
    {
        Task<User?> AuthenticateAsync(string email, string password);
        Task<User> RegisterAsync(RegisterRequest request);
        Task<IEnumerable<string>> GetUserRolesAsync(int userId);
        Task<IEnumerable<string>> GetUserPermissionsAsync(int userId);
        Task<RefreshToken> SaveRefreshTokenAsync(RefreshToken refreshToken);
        Task<(User? User, RefreshToken? RefreshToken)> ValidateRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token, string ipAddress, string? reason = null, string? replacedByToken = null);
        Task RevokeAllUserRefreshTokensAsync(int userId, string ipAddress, string reason);
        Task LogLoginAttempt(LoginLog log);
    }

    public class AuthService : IAuthService
    {
        private readonly IDatabase _database;

        public AuthService(IDatabase database)
        {
            _database = database;
        }

        public async Task<User?> AuthenticateAsync(string email, string password)
        {
            using var connection = _database.CreateConnection();
            
            var user = await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Email = @Email AND IsActive = 1", 
                new { Email = email });
            
            if (user == null)
                return null;
                
            var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            
            return isPasswordValid ? user : null;
        }

        public async Task<User> RegisterAsync(RegisterRequest request)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            
            using var connection = _database.CreateConnection();
            
            if (connection.State == ConnectionState.Closed)
                connection.Open();
                
            using var transaction = connection.BeginTransaction();
            
            try
            {
                // Check if user with the same email already exists
                var existingUser = await connection.QueryFirstOrDefaultAsync<User>(
                    "SELECT * FROM Users WHERE Email = @Email", 
                    new { Email = request.Email },
                    transaction: transaction);
                
                if (existingUser != null)
                    throw new BadRequestException("User with this email already exists");
                
                // Create new user
                var sql = @"
                    INSERT INTO Users (Email, PasswordHash, Name, IsActive, CreatedAt, CreatedBy) 
                    VALUES (@Email, @PasswordHash, @Name, 1, @Now, 'System');
                    SELECT * FROM Users WHERE UserId = SCOPE_IDENTITY();";
                
                var now = DateTime.UtcNow;
                
                var parameters = new DynamicParameters();
                parameters.Add("Email", request.Email);
                parameters.Add("PasswordHash", passwordHash);
                parameters.Add("Name", request.Name);
                parameters.Add("Now", now);
                parameters.Add("CreatedBy", "System");
                
                var newUser = await connection.QueryFirstAsync<User>(sql, parameters, transaction: transaction);
                
                // Assign default user role
                await connection.ExecuteAsync(@"
                    INSERT INTO UserRoles (UserId, RoleId, CreatedAt, CreatedBy)
                    SELECT @UserId, RoleId, @Now, 'System'
                    FROM Roles WHERE RoleName = 'User'",
                    new { UserId = newUser.UserId, Now = now },
                    transaction: transaction);
                
                transaction.Commit();
                return newUser;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new AppException("An error occurred while registering the user", ex);
            }
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
        {
            using var connection = _database.CreateConnection();
            
            var roles = await connection.QueryAsync<string>(@"
                SELECT r.RoleName
                FROM UserRoles ur
                JOIN Roles r ON ur.RoleId = r.RoleId
                WHERE ur.UserId = @UserId",
                new { UserId = userId });
            
            return roles;
        }

        public async Task<IEnumerable<string>> GetUserPermissionsAsync(int userId)
        {
            using var connection = _database.CreateConnection();
            
            var permissions = await connection.QueryAsync<string>(@"
                SELECT p.PermissionName
                FROM UserRoles ur
                JOIN Roles r ON ur.RoleId = r.RoleId
                JOIN RolePermissions rp ON r.RoleId = rp.RoleId
                JOIN Permissions p ON rp.PermissionId = p.PermissionId
                WHERE ur.UserId = @UserId",
                new { UserId = userId });
            
            return permissions;
        }

        public async Task<RefreshToken> SaveRefreshTokenAsync(RefreshToken refreshToken)
        {
            using var connection = _database.CreateConnection();
            
            if (connection.State == ConnectionState.Closed)
                connection.Open();
                
            using var transaction = connection.BeginTransaction();
            
            try
            {
                var sql = @"
                    INSERT INTO RefreshTokens (UserId, Token, ExpiryDate, Created, CreatedByIp)
                    VALUES (@UserId, @Token, @ExpiryDate, @Created, @CreatedByIp);
                    SELECT SCOPE_IDENTITY();";
                
                var tokenId = await connection.ExecuteScalarAsync<int>(sql, new
                {
                    refreshToken.UserId,
                    refreshToken.Token,
                    refreshToken.ExpiryDate,
                    refreshToken.Created,
                    refreshToken.CreatedByIp
                }, transaction);
                
                refreshToken.TokenId = tokenId;
                
                transaction.Commit();
                return refreshToken;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new AppException("Failed to save refresh token", ex);
            }
        }

        public async Task<(User? User, RefreshToken? RefreshToken)> ValidateRefreshTokenAsync(string token)
        {
            using var connection = _database.CreateConnection();
            
            var sql = @"
                SELECT rt.*, u.*
                FROM RefreshTokens rt
                JOIN Users u ON rt.UserId = u.UserId
                WHERE rt.Token = @Token";
            
            var result = await connection.QueryAsync<RefreshToken, User, (RefreshToken, User)>(
                sql,
                (refreshToken, user) => (refreshToken, user),
                new { Token = token },
                splitOn: "UserId"
            );

            var (refreshToken, user) = result.FirstOrDefault();
            
            if (refreshToken == null || user == null)
                throw new UnauthorizedException("Invalid refresh token");
                
            if (!user.IsActive)
                throw new UnauthorizedException("User account is disabled");
                
            if (refreshToken.IsExpired)
                throw new UnauthorizedException("Refresh token has expired");
                
            if (refreshToken.Revoked != null)
                throw new UnauthorizedException("Refresh token has been revoked");
            
            return (user, refreshToken);
        }
        
        public async Task RevokeRefreshTokenAsync(string token, string ipAddress, string? reason = null, string? replacedByToken = null)
        {
            using var connection = _database.CreateConnection();
            
            if (connection.State == ConnectionState.Closed)
                connection.Open();
                
            using var transaction = connection.BeginTransaction();
            
            try
            {
                var sql = @"
                    UPDATE RefreshTokens
                    SET Revoked = @Now,
                        RevokedByIp = @IpAddress,
                        ReasonRevoked = @Reason,
                        ReplacedByToken = @ReplacedByToken
                    WHERE Token = @Token AND Revoked IS NULL";
                
                await connection.ExecuteAsync(sql, new
                {
                    Now = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    Reason = reason,
                    ReplacedByToken = replacedByToken,
                    Token = token
                }, transaction);
                
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new AppException("Failed to revoke refresh token", ex);
            }
        }

        public async Task RevokeAllUserRefreshTokensAsync(int userId, string ipAddress, string reason)
        {
            using var connection = _database.CreateConnection();
            
            if (connection.State == ConnectionState.Closed)
                connection.Open();
                
            using var transaction = connection.BeginTransaction();
            
            try
            {
                var sql = @"
                    UPDATE RefreshTokens
                    SET Revoked = @Now,
                        RevokedByIp = @IpAddress,
                        ReasonRevoked = @Reason
                    WHERE UserId = @UserId AND Revoked IS NULL";
                
                await connection.ExecuteAsync(sql, new
                {
                    Now = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    Reason = reason,
                    UserId = userId
                }, transaction);
                
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new AppException("Failed to revoke user refresh tokens", ex);
            }
        }

        public async Task LogLoginAttempt(LoginLog log)
        {
            using var connection = _database.CreateConnection();

            if (connection.State == ConnectionState.Closed)
                connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                var sql = @"
                    INSERT INTO Login_Logs (Email, IsSuccess , Timestamp, UserAgent, AttemptIp)
                    VALUES (@Email, @IsSuccess , @Timestamp, @UserAgent, @AttemptIp);
                    SELECT SCOPE_IDENTITY();";

                var tokenId = await connection.ExecuteScalarAsync<int>(sql, new
                {
                    log.Email,
                    log.IsSuccess,
                    log.Timestamp,
                    log.UserAgent,
                    log.AttemptIp
                }, transaction);


                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new AppException("Failed to save refresh token", ex);
            }
        }
    }
}