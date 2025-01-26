using RecipePlatform.RecipeManagementService.Contracts.DTO;
using RecipePlatform.RecipeManagementService.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.RecipeManagementService.Data.IRepository
{
    public interface IRecipeRepository
    {
        Task<string> AddRecipeAsync(Recipe recipe);
        Task<int> GetNextRecipeIdAsync();
        Task<Recipe> GetRecipeByIdAsync(int recipeId);
        Task<Recipe> GetLatestRecipeAsync();
        Task<(bool IsSuccess, string ErrorMessage)> RejectRecipeAsync(int recipeId, int userId, string feedback);
        Task<(bool IsSuccess, string ErrorMessage)> ApproveRecipeAsync(int recipeId, int userId);
        Task<IEnumerable<Recipe>> GetPendingRecipesAsync();
        Task<IEnumerable<Recipe>> GetRejectedRecipesAsync();
        Task<IEnumerable<Recipe>> GetRejectedByUserIdAsync(int userId);
        Task<IEnumerable<Recipe>> GetApprovedRecipesAsync();
        Task<IEnumerable<Recipe>> GetPendingRecipesByUserIdAsync(int userId);
        Task<IEnumerable<Recipe>> GetApprovedRecipesByUserIdAsync(int userId);
        Task<Recipe> UpdateRecipeAsync(Recipe recipe);
        Task<bool> DeleteRecipeAsync(int recipeId);
    }
}
