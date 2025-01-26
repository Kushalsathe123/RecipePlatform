using System.ComponentModel.DataAnnotations;

namespace RecipePlatform.UserManagementService.Contracts.DTOs
{
    public class DeleteAccountRequestDto
    {
        [Required]
        [EmailAddress]
        public string? email { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
