using RecipePlatform.RecipeManagementService.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.RecipeManagementService.Data.IRepository
{
    public interface ILikeRepository
    {
        Task AddLikeAsync(int recipeId, int userId);
        Task<bool> IsRecipeLikedByUserAsync(int recipeId, int userId);
        Task<List<Like>> GetLikesByRecipeIdAsync(int recipeId);

        Task<bool> RecipeExistsAsync(int recipeId);
        Task RemoveLikeAsync(int recipeId, int userId);
    }
}
