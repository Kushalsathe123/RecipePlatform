using MongoDB.Bson;
using MongoDB.Driver;
using RecipePlatform.RecipeManagementService.Data.Entities;
using RecipePlatform.RecipeManagementService.Data.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.RecipeManagementService.Data.Repository
{
    public class LikeRepository : ILikeRepository
    {
        private readonly IMongoCollection<Like> _likes;
        private readonly IMongoCollection<Recipe> _recipeCollection;

        public LikeRepository(IMongoDatabase database)
        {
            _likes = database.GetCollection<Like>("Likes");
            _recipeCollection = database.GetCollection<Recipe>("Recipes");
        }

        public async Task AddLikeAsync(int recipeId, int userId)
        {
            var like = new Like
            {
                RecipeId = recipeId,
                UserId = userId,
                LikedAt = DateTime.UtcNow
            };

            await _likes.InsertOneAsync(like);
        }

        // Checks if the recipe is already liked by the user
        public async Task<bool> IsRecipeLikedByUserAsync(int recipeId, int userId)
        {
            var like = await _likes
                .Find(l => l.RecipeId == recipeId && l.UserId == userId)
                .FirstOrDefaultAsync();

            return like != null;
        }

        // Retrieves all likes for a given recipe
        public async Task<List<Like>> GetLikesByRecipeIdAsync(int recipeId)
        {
            return await _likes
                .Find(l => l.RecipeId == recipeId)
                .ToListAsync();
        }

        public async Task<bool> RecipeExistsAsync(int recipeId)

        {  // Ensure the correct collection name and field mapping
            var recipe = await _recipeCollection
                .Find(r => r.RecipeId == recipeId)
                .FirstOrDefaultAsync();

            if (recipe == null)
            {
                return false;
            }

            //Console.WriteLine($"Recipe with RecipeId {recipeId} found.");
            return true;
        }
        public async Task RemoveLikeAsync(int recipeId, int userId)
        {
            try
            {
                // Find the like record for the given recipeId and userId
                var like = await _likes
                    .Find(l => l.RecipeId == recipeId && l.UserId == userId)
                    .FirstOrDefaultAsync();

                if (like != null)
                {
                    // Remove the like from the likes collection
                    await _likes.DeleteOneAsync(l => l.RecipeId == recipeId && l.UserId == userId);

                    // Now reduce the like count for the associated recipe
                    var recipe = await _recipeCollection.Find(r => r.RecipeId == recipeId).FirstOrDefaultAsync();
                    if (recipe != null)
                    {
                        // Ensure like count doesn't go below 0
                        if (recipe.LikesCount > 0)
                        {
                            recipe.LikesCount -= 1;

                            // Update the recipe with the new like count
                            await _recipeCollection.ReplaceOneAsync(r => r.RecipeId == recipeId, recipe);
                        }
                    }
                }
                else
                {
                    // Handle the case where the like doesn't exist
                    throw new InvalidOperationException("Like not found for the given recipe and user.");
                }
            }
            catch (Exception ex)
            {
                // Log the error (if needed) and rethrow it or handle it accordingly
                throw new Exception("An error occurred while removing the like.", ex);
            }
        }



    }
}
