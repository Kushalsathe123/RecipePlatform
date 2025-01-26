using RecipePlatform.UserManagementService.Contracts.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.UserManagementService.Contracts.Interfaces
{
    public interface ITokenService
    {
        TokenDto GenerateToken(UserDto user);
    }
}
