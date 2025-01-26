using MongoDB.Bson;
using MongoDB.Driver;
using RecipePlatform.RecipeManagementService.Data.Entities;
using RecipePlatform.RecipeManagementService.Data.IRepository;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace RecipePlatform.RecipeManagementService.Data.Repository
{
    public class RecipeRepository : IRecipeRepository
    {
        private readonly IMongoCollection<Recipe> _recipes;
        private AppDbContext mockContext;

        public RecipeRepository(IMongoDatabase mongoDatabase)
        {
            // Set the collection name to 'Recipes' from the MongoDB database
            _recipes = mongoDatabase.GetCollection<Recipe>("Recipes");
        }

        public RecipeRepository(AppDbContext mockContext)
        {
            this.mockContext = mockContext;
        }

        public async Task<string> AddRecipeAsync(Recipe recipe)
        {
            // Generate the RecipeId (manual increment logic)
            recipe.RecipeId = await GetNextRecipeIdAsync();  // Sequential ID generation

            // Insert the new recipe into the Recipes collection
            await _recipes.InsertOneAsync(recipe);

            // Return the RecipeId as a string
            return recipe.RecipeId.ToString();
        }

        private async Task<int> GetNextRecipeIdAsync()
        {
            // Find the recipe with the highest RecipeId
            var lastRecipe = await _recipes.Find(FilterDefinition<Recipe>.Empty)
                .Sort(Builders<Recipe>.Sort.Descending(r => r.RecipeId))
                .FirstOrDefaultAsync();

            // If no recipe exists, return 1. Otherwise, increment the latest RecipeId.
            return lastRecipe == null ? 1 : lastRecipe.RecipeId + 1;
        }

        public async Task<Recipe> GetRecipeByIdAsync(int recipeId)
        {
            // Query to find a recipe by its RecipeId
            return await _recipes.Find(r => r.RecipeId == recipeId).FirstOrDefaultAsync();
        }

        public async Task<Recipe> GetLatestRecipeAsync()
        {
            // Get the recipe with the highest RecipeId
            return await _recipes.Find(FilterDefinition<Recipe>.Empty)
                .SortByDescending(r => r.RecipeId)
                .FirstOrDefaultAsync();
        }

        Task<int> IRecipeRepository.GetNextRecipeIdAsync()
        {
            throw new NotImplementedException();
        }
        public async Task<(bool IsSuccess, string ErrorMessage)> ApproveRecipeAsync(int recipeId, int userId)
        {
            var recipe = await _recipes.Find(r => r.RecipeId == recipeId).FirstOrDefaultAsync();

            if (recipe == null) return (false, "Recipe not found.");
            if (recipe.UserId != userId) return (false, "User does not own the recipe.");
            if (recipe.Status == "approved") return (false, "Recipe is already approved.");

            // Change the status of the recipe to "approved"
            recipe.Status = "approved";
            recipe.ApprovedAt = DateTime.UtcNow;

            var updateResult = await _recipes.ReplaceOneAsync(
                r => r.RecipeId == recipeId,
                recipe
            );

            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0
                ? (true, "")
                : (false, "Failed to approve the recipe.");
        }


        public async Task<(bool IsSuccess, string ErrorMessage)> RejectRecipeAsync(int recipeId, int userId, string feedback)
        {
            // Find the recipe by its RecipeId
            var recipe = await _recipes.Find(r => r.RecipeId == recipeId).FirstOrDefaultAsync();

            if (recipe == null)
            {
                return (false, "Recipe not found.");
            }

            // Check if the recipe belongs to the given UserId
            if (recipe.UserId != userId)
            {
                return (false, "User does not own the recipe.");
            }

            // Check if the recipe is in "pending" status
            if (recipe.Status != "pending")
            {
                return (false, "Recipe is not in pending status and cannot be rejected.");
            }

            // Change the status of the recipe to "rejected" and add feedback
            recipe.Status = "rejected";
            recipe.Feedback = feedback;

            var updateResult = await _recipes.ReplaceOneAsync(
                r => r.RecipeId == recipeId,
                recipe
            );

            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0
                ? (true, "")
                : (false, "Failed to reject the recipe.");
        }

        public async Task<IEnumerable<Recipe>> GetPendingRecipesAsync()
        {
            return await _recipes
                .Find(r => r.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                .ToListAsync();
        }

        public async Task<IEnumerable<Recipe>> GetApprovedRecipesAsync()
        {
            return await _recipes
                .Find(r => r.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                .ToListAsync();
        }

        public async Task<IEnumerable<Recipe>> GetPendingRecipesByUserIdAsync(int userId)
        {
            return await _recipes
                .Find(r => r.UserId == userId && r.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                .ToListAsync();
        }

        public async Task<IEnumerable<Recipe>> GetApprovedRecipesByUserIdAsync(int userId)
        {
            return await _recipes
                .Find(r => r.UserId == userId && r.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                .ToListAsync();
        }

        public async Task<Recipe?> UpdateRecipeAsync(Recipe recipe)
        {
            try
            {
                var filter = Builders<Recipe>.Filter.Eq(r => r.RecipeId, recipe.RecipeId);
                var update = Builders<Recipe>.Update
                    .Set(r => r.RecipeName, recipe.RecipeName)
                    .Set(r => r.Description, recipe.Description)
                    .Set(r => r.Ingredients, recipe.Ingredients)
                    .Set(r => r.Tags, recipe.Tags)
                    .Set(r => r.CookingTime, recipe.CookingTime)
                    .Set(r => r.Instructions, recipe.Instructions)
                    .Set(r => r.ImageUrls, recipe.ImageUrls)  // Ensure this field is updated
                    .Set(r => r.LastUpdatedAt, recipe.LastUpdatedAt);

                var result = await _recipes.UpdateOneAsync(filter, update);

                if (result.ModifiedCount > 0)
                {
                    return recipe;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating recipe: {ex.Message}");
                return null;
            }
        }


        public async Task<bool> DeleteRecipeAsync(int recipeId)
        {
            var filter = Builders<Recipe>.Filter.Eq(r => r.RecipeId, recipeId);
            var result = await _recipes.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        public async Task<IEnumerable<Recipe>> GetRejectedRecipesAsync()
        {
            return await _recipes
                .Find(r => r.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
                .ToListAsync();
        }

        public async Task<IEnumerable<Recipe>> GetRejectedByUserIdAsync(int userId)
        {
            return await _recipes
                .Find(r => r.UserId == userId && r.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
                .ToListAsync();
        }
    }
}
