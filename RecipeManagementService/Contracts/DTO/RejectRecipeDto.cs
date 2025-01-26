using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.RecipeManagementService.Contracts.DTO
{
    public class RejectRecipeDto
    {
        public int UserId { get; set; }
        public int RecipeId { get; set; }

        public string Feedback { get; set; }
    }
}
