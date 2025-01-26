using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.RecipeManagementService.Contracts.DTO
{
    public class EditRecipeDto
    {
        public string? RecipeName { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<IFormFile> ImageUrls { get; set; }
        public List<string>? Ingredients { get; set; }
        public string? Instructions { get; set; }
        public string? CookingTime { get; set; }
        public List<string>? Tags { get; set; }
    }
}
