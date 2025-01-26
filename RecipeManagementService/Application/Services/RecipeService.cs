using MongoDB.Driver;
using RecipePlatform.RecipeManagementService.Application.Mappers;
using RecipePlatform.RecipeManagementService.Contracts.DTO;
using RecipePlatform.RecipeManagementService.Contracts.Interfaces;
using RecipePlatform.RecipeManagementService.Data.Entities;
using RecipePlatform.RecipeManagementService.Data.IRepository;
using RecipePlatform.RecipeManagementService.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.RecipeManagementService.Application.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly IRecipeRepository _recipeRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILikeRepository _likeRepository;
        private readonly IMongoCollection<Recipe> _recipes;
        private IRecipeRepository @object;

        public RecipeService(IRecipeRepository recipeRepository, ICloudinaryService cloudinaryService, ILikeRepository likeRepository, IMongoDatabase database)
        {
            _recipeRepository = recipeRepository;
            _cloudinaryService = cloudinaryService;
            _likeRepository = likeRepository;
            _recipes = database.GetCollection<Recipe>("Recipes");
        }

        public RecipeService(IRecipeRepository @object)
        {
            this.@object = @object;
        }

        public async Task<string> CreateRecipeAsync(CreateRecipeDto createRecipeDto,int userId)
        {
            // Create a new Recipe object from the DTO
            var recipe = new Recipe
            {
                RecipeName = createRecipeDto.RecipeName,
                Description = createRecipeDto.Description,
                Ingredients = createRecipeDto.Ingredients,
                Tags = createRecipeDto.Tags,
                CookingTime = createRecipeDto.CookingTime,
                Instructions = createRecipeDto.Instructions,
                UserId = userId,
                LastUpdatedAt = DateTime.UtcNow,
            };

            // Handle image uploads
            if (createRecipeDto.ImageUrls != null && createRecipeDto.ImageUrls.Count > 0)
            {
                var imageUrls = await _cloudinaryService.UploadImagesAsync(createRecipeDto.ImageUrls);
                recipe.ImageUrls = imageUrls;
            }


            return await _recipeRepository.AddRecipeAsync(recipe);
        }


        public async Task<RecipeDetailDto> GetRecipeByIdAsync(int recipeId)
        {
            var recipe = await _recipeRepository.GetRecipeByIdAsync(recipeId);

            if (recipe == null)
            {
                return null;
            }

            return new RecipeDetailDto
            {
                RecipeId = recipe.RecipeId,
                RecipeName = recipe.RecipeName,
                Description = recipe.Description,
                Ingredients = recipe.Ingredients,
                Instructions = recipe.Instructions,
                CookingTime = recipe.CookingTime,
                Tags = recipe.Tags,
                Status = recipe.Status
            };
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> ApproveRecipeAsync(int recipeId, int userId)
        {
            return await _recipeRepository.ApproveRecipeAsync(recipeId, userId);
        }
        public async Task<(bool IsSuccess, string ErrorMessage)> RejectRecipeAsync(int recipeId, int userId, string feedback)
        {
            return await _recipeRepository.RejectRecipeAsync(recipeId, userId, feedback);
        }

        public async Task LikeRecipeAsync(int recipeId, int userId)
        {
            // Check if the recipe exists
            if (!await _likeRepository.RecipeExistsAsync(recipeId))
            {
                throw new InvalidOperationException("Recipe does not exist.");
            }

            // Check if the user has already liked the recipe
            if (await _likeRepository.IsRecipeLikedByUserAsync(recipeId, userId))
            {
                throw new InvalidOperationException("You have already liked this recipe.");
            }

            // Add the like
            await _likeRepository.AddLikeAsync(recipeId, userId);

            // Increment the like count of the recipe
            var recipeUpdate = Builders<Recipe>.Update.Inc(r => r.LikesCount, 1);
            await _recipes.UpdateOneAsync(r => r.RecipeId == recipeId, recipeUpdate);
        }

        public async Task UnlikeRecipeAsync(int recipeId, int userId)
        {
            var isLiked = await _likeRepository.IsRecipeLikedByUserAsync(recipeId, userId);
            if (isLiked)
            {
                await _likeRepository.RemoveLikeAsync(recipeId, userId);
            }
            else
            {
                throw new InvalidOperationException("You did not like this recipe yet.");
            }
        }

        public async Task<IEnumerable<RecipeDto>> GetPendingRecipesAsync()
        {
            var recipes = await _recipeRepository.GetPendingRecipesAsync();
            return recipes.Select(RecipeMapper.ToDto);
        }

        public async Task<IEnumerable<RecipeDto>> GetApprovedRecipesAsync()
        {
            var recipes = await _recipeRepository.GetApprovedRecipesAsync();
            return recipes.Select(RecipeMapper.ToDto);
        }

        public async Task<IEnumerable<RecipeDto>> GetPendingRecipesByUserIdAsync(int userId)
        {
            ValidateUserId(userId);
            return await GetPendingRecipesByUserIdInternalAsync(userId);
        }

        public async Task<IEnumerable<RecipeDto>> GetApprovedRecipesByUserIdAsync(int userId)
        {
            ValidateUserId(userId);
            return await GetApprovedRecipesByUserIdInternalAsync(userId);
        }

        public async Task<RecipeDto?> EditRecipeAsync(int recipeId, int userId, EditRecipeDto editRecipeDto)
        {
            ValidateEditRecipeParameters(recipeId, userId, editRecipeDto);
            return await EditRecipeInternalAsync(recipeId, userId, editRecipeDto);
        }

        public async Task<bool> DeleteRecipeAsync(int recipeId, int userId)
        {
            ValidateDeleteRecipeParameters(recipeId, userId);
            return await DeleteRecipeInternalAsync(recipeId, userId);
        }

        private async Task<IEnumerable<RecipeDto>> GetPendingRecipesByUserIdInternalAsync(int userId)
        {
            var recipes = await _recipeRepository.GetPendingRecipesByUserIdAsync(userId);
            return recipes.Select(RecipeMapper.ToDto);
        }

        private async Task<IEnumerable<RecipeDto>> GetApprovedRecipesByUserIdInternalAsync(int userId)
        {
            var recipes = await _recipeRepository.GetApprovedRecipesByUserIdAsync(userId);
            return recipes.Select(RecipeMapper.ToDto);
        }

        public async Task<RecipeDto?> EditRecipeInternalAsync(int recipeId, int userId, EditRecipeDto editRecipeDto)
        {
            ValidateEditRecipeParameters(recipeId, userId, editRecipeDto);

            // Get the existing recipe from the database
            var existingRecipe = await _recipeRepository.GetRecipeByIdAsync(recipeId);
            if (existingRecipe == null || existingRecipe.UserId != userId)
            {
                return null;
            }

            // Handle image files upload
            if (editRecipeDto.ImageUrls != null && editRecipeDto.ImageUrls.Count > 0)
            {
                // Clear the old images and upload new ones to Cloudinary
                var imageUrls = await _cloudinaryService.UploadImagesAsync(editRecipeDto.ImageUrls);

                // Add the new image URLs to the ImageUrls list
                existingRecipe.ImageUrls = imageUrls;
            }

            // Update other fields of the recipe
            existingRecipe.RecipeName = editRecipeDto.RecipeName ?? existingRecipe.RecipeName;
            existingRecipe.Description = editRecipeDto.Description ?? existingRecipe.Description;
            existingRecipe.Ingredients = editRecipeDto.Ingredients ?? existingRecipe.Ingredients;
            existingRecipe.Tags = editRecipeDto.Tags ?? existingRecipe.Tags;
            existingRecipe.CookingTime = editRecipeDto.CookingTime ?? existingRecipe.CookingTime;
            existingRecipe.Instructions = editRecipeDto.Instructions ?? existingRecipe.Instructions;
            existingRecipe.LastUpdatedAt = DateTime.UtcNow;

            try
            {
                // Update the recipe in the MongoDB collection
                var updateResult = await _recipeRepository.UpdateRecipeAsync(existingRecipe);

                // Check if the recipe was successfully updated
                if (updateResult == null)
                {
                    return null;
                }

                // Return the updated recipe DTO
                return RecipeMapper.ToDto(existingRecipe);
            }
            catch (Exception ex)
            {
                // Log the error if any
                Console.WriteLine($"Error updating recipe: {ex.Message}");
                return null;
            }
        }



        private static void UpdateRecipeFields(Recipe existingRecipe, EditRecipeDto editRecipeDto)
        {
            if (editRecipeDto.RecipeName != null)
                existingRecipe.RecipeName = editRecipeDto.RecipeName;

            if (editRecipeDto.Description != null)
                existingRecipe.Description = editRecipeDto.Description;

            if (editRecipeDto.Ingredients != null && editRecipeDto.Ingredients.Count > 0)
                existingRecipe.Ingredients = editRecipeDto.Ingredients;


            if (editRecipeDto.Instructions != null)
                existingRecipe.Instructions = editRecipeDto.Instructions;

            if (editRecipeDto.CookingTime != null)
                existingRecipe.CookingTime = editRecipeDto.CookingTime;

            if (editRecipeDto.Tags != null && editRecipeDto.Tags.Count > 0)
                existingRecipe.Tags = editRecipeDto.Tags;

            existingRecipe.LastUpdatedAt = DateTime.UtcNow;
        }



        private async Task<bool> DeleteRecipeInternalAsync(int recipeId, int userId)
        {
            var recipe = await _recipeRepository.GetRecipeByIdAsync(recipeId);
            if (recipe == null || recipe.UserId != userId)
            {
                return false;
            }

            return await _recipeRepository.DeleteRecipeAsync(recipeId);
        }

        private static void ValidateUserId(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("Invalid user ID", nameof(userId));
            }
        }

        private static void ValidateEditRecipeParameters(int recipeId, int userId, EditRecipeDto editRecipeDto)
        {
            if (recipeId <= 0)
            {
                throw new ArgumentException("Invalid recipe ID", nameof(recipeId));
            }

            ValidateUserId(userId);

            ArgumentNullException.ThrowIfNull(editRecipeDto);
        }

        private static void ValidateDeleteRecipeParameters(int recipeId, int userId)
        {
            if (recipeId <= 0)
            {
                throw new ArgumentException("Invalid recipe ID", nameof(recipeId));
            }

            ValidateUserId(userId);
        }

        public async Task<IEnumerable<RecipeDto>> GetRejectedRecipesAsync()
        {
            var recipes = await _recipeRepository.GetRejectedRecipesAsync();
            return recipes.Select(RecipeMapper.ToDto);
        }

        public async Task<IEnumerable<RecipeDto>> GetRejectedRecipesByUserIdAsync(int userId)
        {
            var recipes = await _recipeRepository.GetRejectedByUserIdAsync(userId);
            return recipes.Select(RecipeMapper.ToDto);
        }
    }
}
