using System.Threading.Tasks;
using RecipePlatform.UserManagementService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using RecipePlatform.UserManagementService.Contracts.Interfaces;
using System;
using RecipePlatform.UserManagementService.Contracts.DTO;

namespace RecipePlatform.UserManagementService.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserDto?> GetUserByEmailAsync(string? email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return null;

            return new UserDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                PasswordSalt = user.PasswordSalt,

            };
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        private static void ValidateUser(User? user)
        {
            ArgumentNullException.ThrowIfNull(user);
        }
        public async Task<bool> UpdateUserAsync(User user)
        {
            ValidateUser(user);

            // Mark the user entity as modified
            _context.Entry(user).State = EntityState.Modified;

            // Ensure specific properties are marked as modified
            _context.Entry(user).Property(x => x.PasswordHash).IsModified = true;
            _context.Entry(user).Property(x => x.PasswordSalt).IsModified = true;

            // Save changes and check if any rows were affected
            int affectedRows = await _context.SaveChangesAsync();
            return affectedRows > 0;
        }
    }
}