using System.Collections.Generic;
using System.Threading.Tasks;
using SummerSplashWeb.Models;

namespace SummerSplashWeb.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int userId);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> ApproveUserAsync(int userId);
        Task<bool> AssignPositionAsync(int userId, string position);
        Task<bool> DeactivateUserAsync(int userId);
        Task<bool> ActivateUserAsync(int userId);
        Task<List<User>> GetUsersByPositionAsync(string position);
        Task<List<User>> GetPendingApprovalsAsync();
        Task<List<User>> GetPendingUsersAsync();
        Task<bool> DeleteUserAsync(int userId);
    }
}
