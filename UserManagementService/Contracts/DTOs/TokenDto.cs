using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.UserManagementService.Contracts.DTO
{
    public class TokenDto
    {
        public int UserId { get; set; }
        [Required]
        public required string AccessToken { get; set; }
        [Required]
        public DateTime ExpirationDate { get; set; }
        [Required]
        public required string TokenType { get; set; } 
    }

}
