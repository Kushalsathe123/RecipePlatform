using RecipePlatform.UserManagementService.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.UserManagementService.Data.Repositories
{
    public interface IDeleteUserRepository
    {
        Task<User?> GetUserByIdAsync(int userId);
        Task<bool> DeleteUserAsync(int userId);
    }
}
