using RecipePlatform.UserManagementService.Contracts.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.UserManagementService.Data.Repositories
{
    public interface ITokenRepository
    {
        Task StoreUserTokenAsync(TokenDto tokenDto);
        Task<bool> IsTokenValidAsync(int userId, string token);
        Task<bool> InvalidateTokenAsync(string token);
    }
}
