using System.Threading.Tasks;
using SummerSplashWeb.Models;

namespace SummerSplashWeb.Services
{
    public interface IAuthService
    {
        Task<User?> LoginAsync(string email, string password);
        Task<bool> RegisterAsync(string firstName, string lastName, string email, string password);
        Task<bool> RequestPasswordResetAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<string> GenerateVerificationCodeAsync(string email);
        Task<bool> VerifyCodeAsync(string email, string code);
        Task<bool> SendVerificationCodeAsync(string email);
        Task LogoutAsync();
        User? CurrentUser { get; }
        bool IsAuthenticated { get; }
    }
}
