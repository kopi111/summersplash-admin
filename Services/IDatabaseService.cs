using System.Data;

namespace SummerSplashWeb.Services
{
    public interface IDatabaseService
    {
        IDbConnection CreateConnection();
        Task<bool> TestConnectionAsync();
        Task InitializeDatabaseAsync();
    }
}
