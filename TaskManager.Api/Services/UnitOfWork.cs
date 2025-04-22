using System.Data;
using Microsoft.Data.SqlClient;

namespace TaskManager.Api.Services
{
    public interface IUnitOfWork : IDisposable
    {
        SqlConnection Connection { get; }
        IDbTransaction Transaction { get; }
        Task BeginAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDatabase _database;
        private SqlConnection? _connection;
        private IDbTransaction? _transaction;

        public UnitOfWork(IDatabase database)
        {
            _database = database;
        }

        public SqlConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = _database.CreateConnection();
                }
                return _connection;
            }
        }

        public IDbTransaction Transaction
        {
            get
            {
                if (_transaction == null)
                {
                    throw new InvalidOperationException("Transaction is not started. Call BeginAsync first.");
                }
                return _transaction;
            }
        }

        public async Task BeginAsync()
        {
            if (_connection == null)
            {
                _connection = _database.CreateConnection();
            }

            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync();
            }

            _transaction = _connection.BeginTransaction();
        }

        public async Task CommitAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await ((SqlTransaction)_transaction).CommitAsync();
                }
            }
            finally
            {
                Dispose();
            }
        }

        public async Task RollbackAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await ((SqlTransaction)_transaction).RollbackAsync();
                }
            }
            finally
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _connection?.Dispose();
            _transaction = null;
            _connection = null;
        }
    }
}