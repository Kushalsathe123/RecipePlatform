using RecipePlatform.UserManagementService.Data.Entities;

namespace RecipePlatform.UserManagementService.Data.Repositories
{
    public interface INewUserRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int userId);  // New method to get user by ID
        Task AddUserAsync(User user);
        //Task<bool> DeleteUserAsync(int userId);    // New method to delete user by ID
    }
}