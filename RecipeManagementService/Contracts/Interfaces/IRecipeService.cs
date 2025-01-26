using RecipePlatform.RecipeManagementService.Contracts.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.RecipeManagementService.Contracts.Interfaces
{
    public interface IRecipeService
    {
        Task<string> CreateRecipeAsync(CreateRecipeDto createRecipeDto,int userId);
        Task<RecipeDetailDto> GetRecipeByIdAsync(int recipeId);
        Task<(bool IsSuccess, string ErrorMessage)> ApproveRecipeAsync(int recipeId, int userId);
        Task<(bool IsSuccess, string ErrorMessage)> RejectRecipeAsync(int recipeId, int userId, string feedback);

        Task LikeRecipeAsync(int recipeId, int userId);

        Task UnlikeRecipeAsync(int recipeId, int userId);

        Task<IEnumerable<RecipeDto>> GetRejectedRecipesAsync();
        Task<IEnumerable<RecipeDto>> GetRejectedRecipesByUserIdAsync(int userId);

        Task<IEnumerable<RecipeDto>> GetPendingRecipesAsync();
        Task<IEnumerable<RecipeDto>> GetApprovedRecipesAsync();
        Task<IEnumerable<RecipeDto>> GetPendingRecipesByUserIdAsync(int userId);
        Task<IEnumerable<RecipeDto>> GetApprovedRecipesByUserIdAsync(int userId);
        Task<RecipeDto?> EditRecipeAsync(int recipeId, int userId, EditRecipeDto editRecipeDto);
        Task<bool> DeleteRecipeAsync(int recipeId, int userId);

    }
}
