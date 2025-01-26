using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using RecipePlatform.RecipeManagementService.Application.Services;
using RecipePlatform.RecipeManagementService.Contracts.DTO;
using RecipePlatform.RecipeManagementService.Data.Entities;
using RecipePlatform.RecipeManagementService.Data.IRepository;
//using RecipePlatform.RecipeManagementService.Data.Repositories;
using Xunit;

namespace RecipePlatform.RecipeManagementService.Tests.Application.Services
{
    public class RecipeServiceTests
    {
        private readonly Mock<IRecipeRepository> _mockRepository;
        private readonly RecipeService _service;

        public RecipeServiceTests()
        {
            _mockRepository = new Mock<IRecipeRepository>();
            _service = new RecipeService(_mockRepository.Object);
        }

        //[Fact]
        //public async Task GetPendingRecipesAsync_ReturnsListOfPendingRecipes()
        //{
        //    // Arrange
        //    var pendingRecipes = new List<Recipe>
        //    {
        //        new Recipe { RecipeId = 1, RecipeName = "Pending Recipe 1", Status = "Pending" },
        //        new Recipe { RecipeId = 2, RecipeName = "Pending Recipe 2", Status = "Pending" }
        //    };
        //    _mockRepository.Setup(repo => repo.GetPendingRecipesAsync()).ReturnsAsync(pendingRecipes);

        //    // Act
        //    var result = await _service.GetPendingRecipesAsync();

        //    // Assert
        //    Assert.Equal(2, result.Count());
        //    Assert.All(result, recipe => Assert.Equal("Pending", recipe.Status));
        //}

        //[Fact]
        //public async Task GetApprovedRecipesAsync_ReturnsListOfApprovedRecipes()
        //{
        //    // Arrange
        //    var approvedRecipes = new List<Recipe>
        //    {
        //        new Recipe { RecipeId = 1, RecipeName = "Approved Recipe 1", Status = "Approved" },
        //        new Recipe { RecipeId = 2, RecipeName = "Approved Recipe 2", Status = "Approved" }
        //    };
        //    _mockRepository.Setup(repo => repo.GetApprovedRecipesAsync()).ReturnsAsync(approvedRecipes);

        //    // Act
        //    var result = await _service.GetApprovedRecipesAsync();

        //    // Assert
        //    Assert.Equal(2, result.Count());
        //    Assert.All(result, recipe => Assert.Equal("Approved", recipe.Status));
        //}

        //[Fact]
        //public async Task GetPendingRecipesByUserIdAsync_ReturnsUsersPendingRecipes()
        //{
        //    // Arrange
        //    int userId = 1;
        //    var userPendingRecipes = new List<Recipe>
        //    {
        //        new Recipe { RecipeId = 1, RecipeName = "User Pending Recipe", Status = "Pending", UserId = userId }
        //    };
        //    _mockRepository.Setup(repo => repo.GetPendingRecipesByUserIdAsync(userId)).ReturnsAsync(userPendingRecipes);

        //    // Act
        //    var result = await _service.GetPendingRecipesByUserIdAsync(userId);

        //    // Assert
        //    Assert.Single(result);
        //    Assert.All(result, recipe => Assert.Equal(userId, recipe.UserId));
        //}

        //[Fact]
        //public async Task GetApprovedRecipesByUserIdAsync_ReturnsUsersApprovedRecipes()
        //{
        //    // Arrange
        //    int userId = 1;
        //    var userApprovedRecipes = new List<Recipe>
        //    {
        //        new Recipe { RecipeId = 1, RecipeName = "User Approved Recipe", Status = "Approved", UserId = userId }
        //    };
        //    _mockRepository.Setup(repo => repo.GetApprovedRecipesByUserIdAsync(userId)).ReturnsAsync(userApprovedRecipes);

        //    // Act
        //    var result = await _service.GetApprovedRecipesByUserIdAsync(userId);

        //    // Assert
        //    Assert.Single(result);
        //    Assert.All(result, recipe => Assert.Equal(userId, recipe.UserId));
        //}

        
        //[Fact]
        //public async Task EditRecipeAsync_ReturnsNullForNonExistentRecipe()
        //{
        //    // Arrange
        //    int recipeId = 1;
        //    int userId = 1;
        //    var editRecipeDto = new EditRecipeDto { RecipeName = "Updated Recipe" };
        //    _mockRepository.Setup(repo => repo.GetRecipeByIdAsync(recipeId)).ReturnsAsync((Recipe?)null);

        //    // Act
        //    var result = await _service.EditRecipeAsync(recipeId, userId, editRecipeDto);

        //    // Assert
        //    Assert.Null(result);
        //}

        //[Fact]
        //public async Task EditRecipeAsync_ReturnsNullForDifferentUserId()
        //{
        //    // Arrange
        //    int recipeId = 1;
        //    int userId = 1;
        //    int differentUserId = 2;
        //    var existingRecipe = new Recipe { RecipeId = recipeId, UserId = userId };
        //    var editRecipeDto = new EditRecipeDto { RecipeName = "Updated Recipe" };
        //    _mockRepository.Setup(repo => repo.GetRecipeByIdAsync(recipeId)).ReturnsAsync(existingRecipe);

        //    // Act
        //    var result = await _service.EditRecipeAsync(recipeId, differentUserId, editRecipeDto);

        //    // Assert
        //    Assert.Null(result);
        //}

        //[Fact]
        //public async Task EditRecipeAsync_HandlesKeyNotFoundException()
        //{
        //    // Arrange
        //    int recipeId = 1;
        //    int userId = 1;
        //    var existingRecipe = new Recipe { RecipeId = recipeId, UserId = userId };
        //    var editRecipeDto = new EditRecipeDto { RecipeName = "Updated Recipe" };
        //    _mockRepository.Setup(repo => repo.GetRecipeByIdAsync(recipeId)).ReturnsAsync(existingRecipe);
        //    _mockRepository.Setup(repo => repo.UpdateRecipeAsync(It.IsAny<Recipe>())).ThrowsAsync(new KeyNotFoundException());

        //    // Act
        //    var result = await _service.EditRecipeAsync(recipeId, userId, editRecipeDto);

        //    // Assert
        //    Assert.Null(result);
        //}

        //[Fact]
        //public async Task DeleteRecipeAsync_DeletesRecipeSuccessfully()
        //{
        //    // Arrange
        //    int recipeId = 1;
        //    int userId = 1;
        //    var existingRecipe = new Recipe { RecipeId = recipeId, UserId = userId };
        //    _mockRepository.Setup(repo => repo.GetRecipeByIdAsync(recipeId)).ReturnsAsync(existingRecipe);
        //    _mockRepository.Setup(repo => repo.DeleteRecipeAsync(recipeId)).ReturnsAsync(true);

        //    // Act
        //    var result = await _service.DeleteRecipeAsync(recipeId, userId);

        //    // Assert
        //    Assert.True(result);
        //}

        //[Fact]
        //public async Task DeleteRecipeAsync_ReturnsFalseForNonExistentRecipe()
        //{
        //    // Arrange
        //    int recipeId = 1;
        //    int userId = 1;
        //    _mockRepository.Setup(repo => repo.GetRecipeByIdAsync(recipeId)).ReturnsAsync((Recipe?)null);

        //    // Act
        //    var result = await _service.DeleteRecipeAsync(recipeId, userId);

        //    // Assert
        //    Assert.False(result);
        //}

        //[Fact]
        //public async Task DeleteRecipeAsync_ReturnsFalseForDifferentUserId()
        //{
        //    // Arrange
        //    int recipeId = 1;
        //    int userId = 1;
        //    int differentUserId = 2;
        //    var existingRecipe = new Recipe { RecipeId = recipeId, UserId = userId };
        //    _mockRepository.Setup(repo => repo.GetRecipeByIdAsync(recipeId)).ReturnsAsync(existingRecipe);

        //    // Act
        //    var result = await _service.DeleteRecipeAsync(recipeId, differentUserId);

        //    // Assert
        //    Assert.False(result);
        //}

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetPendingRecipesByUserIdAsync_ThrowsArgumentExceptionForInvalidUserId(int invalidUserId)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetPendingRecipesByUserIdAsync(invalidUserId));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetApprovedRecipesByUserIdAsync_ThrowsArgumentExceptionForInvalidUserId(int invalidUserId)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetApprovedRecipesByUserIdAsync(invalidUserId));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task EditRecipeAsync_ThrowsArgumentExceptionForInvalidRecipeId(int invalidRecipeId)
        {
            // Arrange
            var editRecipeDto = new EditRecipeDto { RecipeName = "Test Recipe" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.EditRecipeAsync(invalidRecipeId, 1, editRecipeDto));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task EditRecipeAsync_ThrowsArgumentExceptionForInvalidUserId(int invalidUserId)
        {
            // Arrange
            var editRecipeDto = new EditRecipeDto { RecipeName = "Test Recipe" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.EditRecipeAsync(1, invalidUserId, editRecipeDto));
        }

        [Fact]
        public async Task EditRecipeAsync_ThrowsArgumentNullExceptionForNullEditRecipeDto()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.EditRecipeAsync(1, 1, null!));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeleteRecipeAsync_ThrowsArgumentExceptionForInvalidRecipeId(int invalidRecipeId)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteRecipeAsync(invalidRecipeId, 1));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeleteRecipeAsync_ThrowsArgumentExceptionForInvalidUserId(int invalidUserId)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteRecipeAsync(1, invalidUserId));
        }
    }
}