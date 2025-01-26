using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using RecipePlatform.UserManagementService.Contracts.CustomAttributes;

namespace RecipePlatform.UserManagementService.Contracts.DTO
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = "";

        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string PasswordSalt { get; set; } = "";
    }

}
