using RecipePlatform.UserManagementService.Contracts.DTO;
using RecipePlatform.UserManagementService.Contracts.Interfaces;
using RecipePlatform.UserManagementService.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RecipePlatform.UserManagementService.Data.Repositories
{
    public class TokenRepository : ITokenRepository
    {
        private readonly ApplicationDbContext _context;

        public TokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task StoreUserTokenAsync(TokenDto tokenDto)
        {
            var userToken = new UserToken
            {
                UserId = tokenDto.UserId,
                AccessToken = tokenDto.AccessToken,
                ExpirationDateTime = tokenDto.ExpirationDate,
            };

            _context.UserTokens.Add(userToken);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsTokenValidAsync(int userId, string token)
        {
            var userToken = await _context.UserTokens
                .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.AccessToken == token);

            return userToken != null && !userToken.IsInvalid && userToken.ExpirationDateTime > DateTime.UtcNow;
        }

        public async Task<bool> InvalidateTokenAsync(string token)
        {
            var userToken = await _context.UserTokens
        .FirstOrDefaultAsync(ut => ut.AccessToken == token && !ut.IsInvalid);

            if (userToken == null)
                return false;

            userToken.IsInvalid = true;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
