using System.ComponentModel.DataAnnotations;

namespace RecipePlatform.UserManagementService.Contracts.DTOs
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
}
