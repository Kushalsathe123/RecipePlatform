using Xunit;
using Moq;
using MongoDB.Driver;
using RecipePlatform.RecipeManagementService.Data;
using RecipePlatform.RecipeManagementService.Data.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using RecipePlatform.RecipeManagementService.Data.Repository;
using RecipePlatform.UserManagementService.Data;

namespace RecipePlatform.RecipeManagementService.Tests.Data.Repositories
{
    public class RecipeRepositoryTests
    {
        private readonly Mock<IMongoDatabase> _mockDatabase;
        private readonly Mock<IMongoCollection<Recipe>> _mockCollection;
        private readonly RecipeRepository _repository;

        public RecipeRepositoryTests()
        {
            _mockDatabase = new Mock<IMongoDatabase>();
            _mockCollection = new Mock<IMongoCollection<Recipe>>();

            _mockDatabase.Setup(db => db.GetCollection<Recipe>("Recipes", null))
                .Returns(_mockCollection.Object);

            var mockContext = new AppDbContext(_mockDatabase.Object);
            _repository = new RecipeRepository(mockContext);
        }

        //[Fact]
        //public async Task GetPendingRecipesAsync_ReturnsOnlyPendingRecipes()
        //{
        //    // Arrange
        //    var pendingRecipes = new List<Recipe>
        //    {
        //        new Recipe { RecipeId = 1, Status = "Pending" },
        //        new Recipe { RecipeId = 2, Status = "Pending" }
        //    };

        //    SetupMockFind(pendingRecipes);

        //    // Act
        //    var result = await _repository.GetPendingRecipesAsync();

        //    // Assert
        //    Assert.Equal(2, result.Count());
        //    Assert.All(result, r => Assert.Equal("Pending", r.Status));
        //}

        //[Fact]
        //public async Task GetApprovedRecipesAsync_ReturnsOnlyApprovedRecipes()
        //{
        //    // Arrange
        //    var approvedRecipes = new List<Recipe>
        //    {
        //        new Recipe { RecipeId = 1, Status = "Approved" },
        //        new Recipe { RecipeId = 2, Status = "Approved" }
        //    };

        //    SetupMockFind(approvedRecipes);

        //    // Act
        //    var result = await _repository.GetApprovedRecipesAsync();

        //    // Assert
        //    Assert.Equal(2, result.Count());
        //    Assert.All(result, r => Assert.Equal("Approved", r.Status));
        //}

        //[Fact]
        //public async Task GetPendingRecipesByUserIdAsync_ReturnsOnlyPendingRecipesForUser()
        //{
        //    // Arrange
        //    int userId = 1;
        //    var pendingRecipes = new List<Recipe>
        //    {
        //        new Recipe { RecipeId = 1, UserId = userId, Status = "Pending" },
        //        new Recipe { RecipeId = 2, UserId = userId, Status = "Pending" }
        //    };

        //    SetupMockFind(pendingRecipes);

        //    // Act
        //    var result = await _repository.GetPendingRecipesByUserIdAsync(userId);

        //    // Assert
        //    Assert.Equal(2, result.Count());
        //    Assert.All(result, r => Assert.Equal(userId, r.UserId));
        //    Assert.All(result, r => Assert.Equal("Pending", r.Status));
        //}

        //[Fact]
        //public async Task GetApprovedRecipesByUserIdAsync_ReturnsOnlyApprovedRecipesForUser()
        //{
        //    // Arrange
        //    int userId = 1;
        //    var approvedRecipes = new List<Recipe>
        //    {
        //        new Recipe { RecipeId = 1, UserId = userId, Status = "Approved" },
        //        new Recipe { RecipeId = 2, UserId = userId, Status = "Approved" }
        //    };

        //    SetupMockFind(approvedRecipes);

        //    // Act
        //    var result = await _repository.GetApprovedRecipesByUserIdAsync(userId);

        //    // Assert
        //    Assert.Equal(2, result.Count());
        //    Assert.All(result, r => Assert.Equal(userId, r.UserId));
        //    Assert.All(result, r => Assert.Equal("Approved", r.Status));
        //}

        //[Fact]
        //public async Task GetRecipeByIdAsync_ReturnsCorrectRecipe()
        //{
        //    // Arrange
        //    int recipeId = 1;
        //    var recipe = new Recipe { RecipeId = recipeId, RecipeName = "Test Recipe" };

        //    SetupMockFind(new List<Recipe> { recipe });

        //    // Act
        //    var result = await _repository.GetRecipeByIdAsync(recipeId);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Equal(recipeId, result.RecipeId);
        //}

        //[Fact]
        //public async Task UpdateRecipeAsync_UpdatesExistingRecipe()
        //{
        //    // Arrange
        //    var recipe = new Recipe { RecipeId = 1, RecipeName = "Updated Recipe" };
        //    var replaceOneResult = new Mock<ReplaceOneResult>();
        //    replaceOneResult.SetupGet(r => r.MatchedCount).Returns(1);

        //    _mockCollection.Setup(c => c.ReplaceOneAsync(
        //        It.IsAny<FilterDefinition<Recipe>>(),
        //        It.IsAny<Recipe>(),
        //        It.IsAny<ReplaceOptions>(),
        //        It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(replaceOneResult.Object);

        //    // Act
        //    var result = await _repository.UpdateRecipeAsync(recipe);

        //    // Assert
        //    Assert.Equal(recipe, result);
        //}

        //[Fact]
        //public async Task DeleteRecipeAsync_DeletesExistingRecipe()
        //{
        //    // Arrange
        //    int recipeId = 1;
        //    var deleteResult = new Mock<DeleteResult>();
        //    deleteResult.SetupGet(r => r.DeletedCount).Returns(1);

        //    _mockCollection.Setup(c => c.DeleteOneAsync(
        //        It.IsAny<FilterDefinition<Recipe>>(),
        //        It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(deleteResult.Object);

        //    // Act
        //    var result = await _repository.DeleteRecipeAsync(recipeId);

        //    // Assert
        //    Assert.True(result);
        //}

        //private void SetupMockFind(List<Recipe> recipes)
        //{
        //    var mockCursor = new Mock<IAsyncCursor<Recipe>>();
        //    mockCursor.Setup(c => c.Current).Returns(recipes);
        //    mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(true)
        //        .ReturnsAsync(false);

        //    _mockCollection.Setup(c => c.FindAsync(
        //        It.IsAny<FilterDefinition<Recipe>>(),
        //        It.IsAny<FindOptions<Recipe>>(),
        //        It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(mockCursor.Object);
        //}
    }
}