using System.ComponentModel.DataAnnotations;

namespace RecipePlatform.UserManagementService.Contracts.DTOs
{
    public class UpdateUserProfileDto
    {
        [Required]
        public int? UserId { get; set; }
        public string? Name { get; set; } 
        public string[]? DietPreferences { get; set; } 
        public string[]? FavoriteCuisines { get; set; } 
    }
}