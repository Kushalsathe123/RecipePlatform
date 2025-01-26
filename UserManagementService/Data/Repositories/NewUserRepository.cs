using Microsoft.EntityFrameworkCore;
using RecipePlatform.UserManagementService.Data.Entities;

namespace RecipePlatform.UserManagementService.Data.Repositories
{
    public class NewUserRepository : INewUserRepository
    {
        private readonly ApplicationDbContext _context;

        public NewUserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByIdAsync(int userId)   // Implementation of GetUserByIdAsync
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

    }
}
