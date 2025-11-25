using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Dapper;

namespace SummerSplashWeb.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SummerSplashDB")
                ?? "Server=localhost;Database=SummerSplashDB;Integrated Security=true;TrustServerCertificate=true;";
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task InitializeDatabaseAsync()
        {
            using var connection = CreateConnection();

            // Check if database exists and create tables if needed
            var tableExists = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users'"
            );

            if (tableExists == 0)
            {
                // Database schema needs to be initialized
                // This would typically run the schema.sql script
                throw new Exception("Database not initialized. Please run the schema.sql script first.");
            }
        }
    }
}
