using System.ComponentModel.DataAnnotations;

namespace RecipePlatform.UserManagementService.Contracts.DTOs
{
    public class ChangePasswordDto
    {
        [Required]
        public int? UserId { get; set; }
        [Required]
        public required string CurrentPassword { get; set; }
        [Required]
        [StringLength(128, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
  ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number and one special character.")]
        public required string NewPassword { get; set; }
        [Required]
        [Compare("NewPassword", ErrorMessage = "The confirmation password does not match.")]
        public required string ConfirmNewPassword { get; set; }
    }
}