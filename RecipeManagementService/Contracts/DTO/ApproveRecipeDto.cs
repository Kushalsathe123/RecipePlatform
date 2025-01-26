using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.RecipeManagementService.Contracts.DTO
{
    public class ApproveRecipeDto
    {
        public int UserId { get; set; }
        public int RecipeId { get; set; }
    }
}
