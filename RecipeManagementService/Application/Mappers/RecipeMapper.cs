using RecipePlatform.RecipeManagementService.Contracts.DTO;
using RecipePlatform.RecipeManagementService.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.RecipeManagementService.Application.Mappers
{
    public static class RecipeMapper
    {
        public static RecipeDto ToDto(Recipe recipe)
        {
            ArgumentNullException.ThrowIfNull(recipe);

            return new RecipeDto
            {
                RecipeId = recipe.RecipeId,
                UserId = recipe.UserId,
                RecipeName = recipe.RecipeName,
                Description = recipe.Description,
                ImageUrls = recipe.ImageUrls?.ToList() ?? new List<string>(),
                Ingredients = recipe.Ingredients?.ToList() ?? new List<string>(),
                Instructions = recipe.Instructions ?? string.Empty,
                CookingTime = recipe.CookingTime ?? string.Empty,
                Tags = recipe.Tags?.ToList() ?? new List<string>(),
                CreatedAt = recipe.CreatedAt,
                Status = recipe.Status ?? string.Empty,
                ApprovedAt = recipe.ApprovedAt,
                LikesCount = recipe.LikesCount,
                Comments = recipe.Comments?.Select(CommentToDto).ToList() ?? new List<CommentDto>(),
                Feedback = recipe.Feedback
            };
        }

        private static CommentDto CommentToDto(Comment comment)
        {
            ArgumentNullException.ThrowIfNull(comment);

            return new CommentDto
            {
                Username = comment.UserName ?? string.Empty,
                Text = comment.Text ?? string.Empty,
                CreatedAt = comment.CreatedAt,
                Replies = comment.Replies?.Select(CommentToDto).ToList() ?? new List<CommentDto>()
            };
        }
    }
}
