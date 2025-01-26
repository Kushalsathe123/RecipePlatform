using RecipePlatform.UserManagementService.Contracts.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.UserManagementService.Contracts.Interfaces
{
    public interface IPasswordService
    {
        Task<string> ForgotPasswordAsync(string email);
        Task<string> ResetPasswordAsync(ResetPasswordDto request);
        bool VerifyPassword(string? password, string storedHash, string storedSalt);
    }
}
