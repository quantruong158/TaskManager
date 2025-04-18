using Dapper;
using TaskManager.Api.Models;
using TaskManager.Api.Exceptions;
using TaskManager.Api.Models.DTOs;

namespace TaskManager.Api.Services
{
    public class PaginatedResult<T>
    {
        public required IEnumerable<T> Items { get; set; }
        public required int TotalCount { get; set; }
        public required int PageNumber { get; set; }
        public required int PageSize { get; set; }
        public required int TotalPages { get; set; }
        public required bool HasNextPage { get; set; }
        public required bool HasPreviousPage { get; set; }
    }

    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersWithRolesAsync();
        Task<UserDto> GetUserByIdAsync(int id);
        Task<int> CreateUserAsync(User user, string password, List<int> roleIds);
        Task UpdateUserAsync(int id, User user);
        Task DeleteUserAsync(int id);
    }

    public class UserService : IUserService
    {
        private readonly IDatabase _database;
        private readonly IAuthService _authService;

        public UserService(IDatabase database, IAuthService authService)
        {
            _database = database;
            _authService = authService;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersWithRolesAsync()
        {
            using var connection = _database.CreateConnection();
            var userDict = new Dictionary<int, User>();

            var sql = $@"
                SELECT u.*, r.* 
                FROM Users u
                LEFT JOIN UserRoles ur ON u.UserId = ur.UserId
                LEFT JOIN Roles r ON ur.RoleId = r.RoleId";

            var users = await connection.QueryAsync<User, Role, User>(
                sql,
                (user, role) =>
                {
                    if (!userDict.TryGetValue(user.UserId, out var userEntry))
                    {
                        userEntry = user;
                        userEntry.UserRoles = [];
                        userDict.Add(user.UserId, userEntry);
                    }

                    if (role != null)
                    {
                        userEntry.UserRoles.Add(new UserRole { Role = role });
                    }

                    return userEntry;
                },
                splitOn: "RoleId"
            );

            return users.Select(user => user.ToDto());
        }

        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            using var connection = _database.CreateConnection();
            var userDict = new Dictionary<int, User>();

            var users = await connection.QueryAsync<User, Role, User>(
                @"SELECT u.*, r.* 
                FROM Users u
                LEFT JOIN UserRoles ur ON u.UserId = ur.UserId
                LEFT JOIN Roles r ON ur.RoleId = r.RoleId
                WHERE u.UserId = @Id",
                (user, role) =>
                {
                    if (!userDict.TryGetValue(user.UserId, out var userEntry))
                    {
                        userEntry = user;
                        userEntry.UserRoles = new List<UserRole>();
                        userDict.Add(user.UserId, userEntry);
                    }

                    if (role != null)
                    {
                        userEntry.UserRoles.Add(new UserRole { Role = role });
                    }

                    return userEntry;
                },
                new { Id = id },
                splitOn: "RoleId"
            );

            var result = userDict.Values.FirstOrDefault();
            if (result == null)
                throw new NotFoundException("User not found");

            return result.ToDto();
        }

        public async Task<int> CreateUserAsync(User user, string password, List<int> roleIds)
        {
            if (roleIds == null || roleIds.Count == 0)
            {
                throw new BadRequestException("At least one role must be specified");
            }

            using var connection = _database.CreateConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();
            
            try
            {
                // Check if email already exists
                var existingUser = await connection.QueryFirstOrDefaultAsync<User>(
                    "SELECT * FROM Users WHERE Email = @Email",
                    new { user.Email },
                    transaction: transaction);

                if (existingUser != null)
                    throw new BadRequestException("Email already registered");

                // Validate that all role IDs exist
                var existingRoleIds = await connection.QueryAsync<int>(
                    "SELECT RoleId FROM Roles WHERE RoleId IN @RoleIds",
                    new { RoleIds = roleIds },
                    transaction: transaction);

                var invalidRoleIds = roleIds.Except(existingRoleIds);
                if (invalidRoleIds.Any())
                {
                    throw new BadRequestException($"Invalid role IDs: {string.Join(", ", invalidRoleIds)}");
                }

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

                var sql = @"
                    INSERT INTO Users (Email, PasswordHash, Name, IsActive, CreatedAt, CreatedBy)
                    VALUES (@Email, @PasswordHash, @Name, @IsActive, @CreatedAt, @CreatedBy);
                    SELECT SCOPE_IDENTITY();";

                var now = DateTime.UtcNow;
                user.CreatedAt = now;
                user.IsActive = true;

                var parameters = new
                {
                    user.Email,
                    PasswordHash = passwordHash,
                    user.Name,
                    user.IsActive,
                    user.CreatedAt,
                    user.CreatedBy
                };

                var userId = await connection.ExecuteScalarAsync<int>(sql, parameters, transaction: transaction);

                // Insert all user roles
                var userRoles = roleIds.Select(roleId => new
                {
                    UserId = userId,
                    RoleId = roleId
                });

                await connection.ExecuteAsync(
                    @"INSERT INTO UserRoles (UserId, RoleId)
                    VALUES (@UserId, @RoleId)",
                    userRoles,
                    transaction: transaction);

                await transaction.CommitAsync();
                return userId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateUserAsync(int id, User user)
        {
            using var connection = _database.CreateConnection();
            
            // Check if user exists
            var existingUser = await GetUserByIdAsync(id);

            // Check if new email is not taken by another user
            if (!string.Equals(existingUser.Email, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailExists = await connection.QueryFirstOrDefaultAsync<User>(
                    "SELECT * FROM Users WHERE Email = @Email AND UserId != @Id",
                    new { user.Email, Id = id });

                if (emailExists != null)
                    throw new BadRequestException("Email already taken");
            }

            var result = await connection.ExecuteAsync(@"
                UPDATE Users 
                SET Email = @Email,
                    Name = @Name,
                    IsActive = @IsActive,
                    UpdatedAt = @UpdatedAt,
                    UpdatedBy = @UpdatedBy
                WHERE UserId = @Id",
                new
                {
                    Id = id,
                    user.Email,
                    user.Name,
                    user.IsActive,
                    UpdatedAt = DateTime.UtcNow,
                    user.UpdatedBy
                });

            if (result == 0)
                throw new NotFoundException("User not found");
        }

        public async Task DeleteUserAsync(int id)
        {
            using var connection = _database.CreateConnection();
            await connection.OpenAsync();
            var result = await connection.ExecuteAsync(
                "DELETE FROM Users WHERE UserId = @Id",
                new { Id = id });

            if (result == 0)
                throw new NotFoundException("User not found");
        }
    }
}