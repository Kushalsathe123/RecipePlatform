using RecipePlatform.UserManagementService.Contracts.DTO;
using RecipePlatform.UserManagementService.Contracts.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.UserManagementService.Contracts.Interfaces
{
    public interface IAuthService
    {
        Task RegisterUserAsync(RegisterUserDto userDto);
        Task<(string Name, TokenDto Token)> LoginAsync(LoginDto loginDto);
        Task<bool> LogoutAsync(string token);
    }
}
