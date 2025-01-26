using RecipePlatform.UserManagementService.Contracts.CustomAttributes;
using System.ComponentModel.DataAnnotations;

namespace RecipePlatform.UserManagementService.Contracts.DTOs
{
    public class RegisterUserDto
    {
        [Required]
        public required string Name { get; set; }
        [Required]
        [EmailAddress]
        [ValidEmailDomain]
        public required string Email { get; set; }
        [Required]
        [StringLength(128, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
  ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number and one special character.")]
        public required string Password { get; set; }
        [Required, Compare("Password")]
        public required string ConfirmPassword { get; set; }

        public string[]? DietPreferences { get; set; } // Allow null not mandatory

        public string[]? FavoriteCuisines { get; set; } // Allow null not mandatory
    }
}
