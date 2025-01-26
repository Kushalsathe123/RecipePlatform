using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.RecipeManagementService.Contracts.DTO
{
    public class RecipeDetailDto
    {
        public int RecipeId { get; set; }
        public string RecipeName { get; set; }
        public string Description { get; set; }
        public List<string> Ingredients { get; set; } // List of ingredients
        public string Instructions { get; set; }
        public string CookingTime { get; set; } // In minutes
        public List<string> Tags { get; set; }
        public string Status { get; set; }
        public List<LikeDto> Likes { get; set; } // List of likes, each containing user ID and timestamp
        public List<CommentDto> Comments { get; set; } // List of comments, each containing user ID, text, and timestamp
        public DateTime CreatedAt { get; set; } // Date when the recipe was created
    }
}
