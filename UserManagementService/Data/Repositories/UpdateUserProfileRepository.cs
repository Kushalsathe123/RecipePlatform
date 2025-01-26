using Microsoft.EntityFrameworkCore;
using RecipePlatform.UserManagementService.Data;
using RecipePlatform.UserManagementService.Data.Entities;

namespace RecipePlatform.UserManagementService.Data.Repositories
{
    public class UpdateUserProfileRepository : IUpdateUserProfileRepository
    {
        private readonly ApplicationDbContext _context;

        public UpdateUserProfileRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task UpdateUserAsync(User user)
        {
            var existingUser = await _context.Users.FindAsync(user.UserId);
            if (existingUser != null)
            {
                existingUser.Name = user.Name;
                existingUser.DietPreferences = user.DietPreferences;
                existingUser.FavoriteCuisines = user.FavoriteCuisines;
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new DbUpdateConcurrencyException();
            }
        }
    }
}