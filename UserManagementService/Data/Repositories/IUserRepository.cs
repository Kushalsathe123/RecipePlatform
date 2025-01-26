using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecipePlatform.UserManagementService.Contracts.DTO;
using RecipePlatform.UserManagementService.Data.Entities;

namespace RecipePlatform.UserManagementService.Data.Repositories

{
    public interface IUserRepository
    {
        Task<UserDto?> GetUserByEmailAsync(string? email);
        Task<User?> GetUserByIdAsync(int userId);
        Task<bool> UpdateUserAsync(User user);
    }
}
