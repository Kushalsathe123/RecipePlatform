using System.ComponentModel.DataAnnotations;

namespace RecipePlatform.UserManagementService.Contracts.DTOs
{
    public class UserResponseDto
    {
        
        public string Name { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;
        
        public string[] DietPreferences { get; set; } = Array.Empty<string>();
        
        public string[] FavoriteCuisines { get; set; } = Array.Empty<string>();
    }
}