using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RecipePlatform.UserManagementService.Data.Entities
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string PasswordSalt { get; set; } = string.Empty;
        
        public string[] DietPreferences { get; set; } = Array.Empty<string>();
        public string[] FavoriteCuisines { get; set; } = Array.Empty<string>();
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }

}
