using Microsoft.EntityFrameworkCore;
using RecipePlatform.UserManagementService.Data.Entities;

namespace RecipePlatform.UserManagementService.Data.Repositories
{
    public class DeleteUserRepository : IDeleteUserRepository
    {
        private readonly ApplicationDbContext _context;

        public DeleteUserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(int userId)   // Implementation of GetUserByIdAsync
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<bool> DeleteUserAsync(int userId)  // Implementation of DeleteUserAsync
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}