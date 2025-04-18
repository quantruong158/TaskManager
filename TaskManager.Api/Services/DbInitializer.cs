using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TaskManager.Api.Services
{
    public interface IDbInitializer
    {
        Task InitializeAsync();
    }

    public class DbInitializer : IDbInitializer
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DbInitializer> _logger;
        private readonly string _connectionString;

        public DbInitializer(IConfiguration configuration, ILogger<DbInitializer> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") 
                ?? throw new ArgumentNullException("DefaultConnection", "Connection string not found");
        }

        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Starting database initialization");
                await EnsureDatabaseCreatedAsync();
                await RunScriptAsync();
                _logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the database");
                throw;
            }
        }

        private async Task EnsureDatabaseCreatedAsync()
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            var databaseName = builder.InitialCatalog;
            
            // Save the catalog name and then set it to master to check if our DB exists
            builder.InitialCatalog = "master";
            
            using var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync();
            
            // Check if database exists
            var checkDbCommand = $"SELECT COUNT(*) FROM sys.databases WHERE name = '{databaseName}'";
            using var command = new SqlCommand(checkDbCommand, connection);
            var dbExists = (int)await command.ExecuteScalarAsync() > 0;
            
            if (!dbExists)
            {
                _logger.LogInformation($"Creating database {databaseName}");
                var createDbCommand = $"CREATE DATABASE [{databaseName}]";
                using var createCommand = new SqlCommand(createDbCommand, connection);
                await createCommand.ExecuteNonQueryAsync();
                _logger.LogInformation($"Database {databaseName} created successfully");
            }
            else
            {
                _logger.LogInformation($"Database {databaseName} already exists");
            }
        }

        private async Task RunScriptAsync()
        {
            var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "InitializeDatabase.sql");
            var script = await File.ReadAllTextAsync(scriptPath);

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            // Execute the script
            using var command = new SqlCommand(script, connection);
            await command.ExecuteNonQueryAsync();
        }
    }
}