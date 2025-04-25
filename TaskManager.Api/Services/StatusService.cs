using Dapper;
using TaskManager.Api.Exceptions;
using TaskManager.Api.Models;

namespace TaskManager.Api.Services
{
    public interface IStatusService
    {
        Task<IEnumerable<Status>> GetAllStatusesAsync();
        Task<Status> GetStatusByIdAsync(int id);
        Task<int> CreateStatusAsync(Status status);
        Task UpdateStatusAsync(int id, Status status);
        Task DeleteStatusAsync(int id);
    }

    public class StatusService : IStatusService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StatusService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Status>> GetAllStatusesAsync()
        {
            using var connection = _unitOfWork.Connection;
            await connection.OpenAsync();

            return await connection.QueryAsync<Status>(
                "SELECT StatusId, Name, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsActive, [Order] FROM Statuses ORDER BY [Order]");
        }

        public async Task<Status> GetStatusByIdAsync(int id)
        {
            using var connection = _unitOfWork.Connection;
            await connection.OpenAsync();

            const string sql = @"
                SELECT StatusId, Name, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsActive, [Order] 
                FROM Statuses 
                WHERE StatusId = @StatusId";

            var status = await connection.QueryFirstOrDefaultAsync<Status>(sql, new { StatusId = id });
            
            if (status == null)
                throw new AppException($"Status with ID {id} not found");

            return status;
        }

        public async Task<int> CreateStatusAsync(Status status)
        {
            var sql = @"
                INSERT INTO Statuses (Name, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsActive, [Order])
                VALUES (@Name, @CreatedAt, @CreatedBy, @CreatedAt, @CreatedBy, @IsActive, @Order);
                SELECT SCOPE_IDENTITY();";

            var parameters = new
            {
                status.Name,
                status.CreatedAt,
                status.CreatedBy,
                IsActive = true,
                status.Order
            };

            var statusId = await _unitOfWork.Connection.ExecuteScalarAsync<int>(sql, parameters, _unitOfWork.Transaction);
            return statusId;
        }

        public async Task UpdateStatusAsync(int id, Status status)
        {
            const string sql = @"
                UPDATE Statuses 
                SET Name = @Name, 
                    UpdatedAt = @UpdatedAt,
                    UpdatedBy = @UpdatedBy,
                    IsActive = @IsActive,
                    [Order] = @Order
                WHERE StatusId = @StatusId";

            var parameters = new
            {
                StatusId = id,
                status.Name,
                status.UpdatedAt,
                status.UpdatedBy,
                status.IsActive,
                status.Order
            };

            var rowsAffected = await _unitOfWork.Connection.ExecuteAsync(sql, parameters, _unitOfWork.Transaction);

            if (rowsAffected == 0)
                throw new AppException($"Status with ID {id} not found");
        }

        public async Task DeleteStatusAsync(int id)
        {
            const string sql = @"
                DELETE FROM Statuses
                WHERE StatusId = @StatusId";

            var parameters = new
            {
                StatusId = id
            };

            var rowsAffected = await _unitOfWork.Connection.ExecuteAsync(sql, parameters, _unitOfWork.Transaction);

            if (rowsAffected == 0)
                throw new AppException($"Status with ID {id} not found");
        }
    }
}
