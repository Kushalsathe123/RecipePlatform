using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.RecipeManagementService.Contracts.DTO
{
    public class CreateRecipeDto
    {
        //public int UserId { get; set; }
        public string RecipeName { get; set; }
        public string Description { get; set; }
        public List<IFormFile> ? ImageUrls { get; set; }
        public List<string> Ingredients { get; set; }
        public string Instructions { get; set; }
        public string CookingTime { get; set; }
        public List<string> Tags { get; set; }

        // Optional properties if needed for additional context
        //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        //public string Status { get; set; } = "pending";  // Default to "pending" on creation
    }
}
