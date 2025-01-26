using RecipePlatform.UserManagementService.Contracts.CustomAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.UserManagementService.Contracts.DTO
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        [ValidEmailDomain]
        public required string? Email { get; set; }
        [Required]
        
        public required string? Password { get; set; }

    }
}
