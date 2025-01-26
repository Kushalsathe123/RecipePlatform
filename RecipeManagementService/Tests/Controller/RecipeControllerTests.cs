using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System.Security.Claims;
using RecipePlatform.RecipeManagementService.Contracts.Interfaces;
using Moq;
using RecipePlatform.RecipeManagementService.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using RecipePlatform.RecipeManagementService.Contracts.DTO;
using RecipePlatform.RecipeManagementService.Contracts.Responses;


namespace RecipePlatform.RecipeManagementService.Tests
{
    public class RecipeControllerTests
    {
        private readonly Mock<IRecipeService> _mockRecipeService;
        private readonly RecipeController _controller;

        public RecipeControllerTests()
        {
            _mockRecipeService = new Mock<IRecipeService>();
            _controller = new RecipeController(_mockRecipeService.Object);

            // Setup mock authentication
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "123"),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetPendingRecipes_ReturnsOkResult_WithRecipes()
        {
            // Arrange
            var recipes = new List<RecipeDto> { new RecipeDto { RecipeId = 1, RecipeName = "Test Recipe" } };
            _mockRecipeService.Setup(service => service.GetPendingRecipesAsync())
                .ReturnsAsync(recipes);

            // Act
            var actionResult = await _controller.GetPendingRecipes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<GetRecipesResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(recipes, response.Recipes);
        }

        [Fact]
        public async Task GetApprovedRecipes_ReturnsOkResult_WithRecipes()
        {
            // Arrange
            var recipes = new List<RecipeDto> { new RecipeDto { RecipeId = 1, RecipeName = "Approved Recipe" } };
            _mockRecipeService.Setup(service => service.GetApprovedRecipesAsync())
                .ReturnsAsync(recipes);

            // Act
            var actionResult = await _controller.GetApprovedRecipes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<GetRecipesResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(recipes, response.Recipes);
        }

        [Fact]
        public async Task GetUserPendingRecipes_ReturnsOkResult_WithUserRecipes()
        {
            // Arrange
            var recipes = new List<RecipeDto> { new RecipeDto { RecipeId = 1, RecipeName = "User Pending Recipe" } };
            _mockRecipeService.Setup(service => service.GetPendingRecipesByUserIdAsync(123))
                .ReturnsAsync(recipes);

            // Act
            var actionResult = await _controller.GetUserPendingRecipes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<GetRecipesResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(recipes, response.Recipes);
        }

        [Fact]
        public async Task GetUserApprovedRecipes_ReturnsOkResult_WithUserRecipes()
        {
            // Arrange
            var recipes = new List<RecipeDto> { new RecipeDto { RecipeId = 1, RecipeName = "User Approved Recipe" } };
            _mockRecipeService.Setup(service => service.GetApprovedRecipesByUserIdAsync(123))
                .ReturnsAsync(recipes);

            // Act
            var actionResult = await _controller.GetUserApprovedRecipes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<GetRecipesResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(recipes, response.Recipes);
        }

        [Fact]
        public async Task EditRecipe_ReturnsOkResult_WhenRecipeUpdated()
        {
            // Arrange
            var editRecipeDto = new EditRecipeDto { RecipeName = "Updated Recipe" };
            var updatedRecipe = new RecipeDto { RecipeId = 1, RecipeName = "Updated Recipe" };
            _mockRecipeService.Setup(service => service.EditRecipeAsync(1, 123, editRecipeDto))
                .ReturnsAsync(updatedRecipe);

            // Act
            var actionResult = await _controller.EditRecipe(1, editRecipeDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<EditRecipeResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(updatedRecipe, response.Recipe);
        }

        [Fact]
        public async Task EditRecipe_ReturnsNotFound_WhenRecipeNotFound()
        {
            // Arrange
            var editRecipeDto = new EditRecipeDto { RecipeName = "Non-existent Recipe" };
            _mockRecipeService.Setup(service => service.EditRecipeAsync(1, 123, editRecipeDto))
                .ReturnsAsync((RecipeDto?)null);

            // Act
            var actionResult = await _controller.EditRecipe(1, editRecipeDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            var response = Assert.IsType<EditRecipeResponse>(notFoundResult.Value);
            Assert.False(response.Success);
        }

        [Fact]
        public async Task DeleteRecipe_ReturnsOkResult_WhenRecipeDeleted()
        {
            // Arrange
            _mockRecipeService.Setup(service => service.DeleteRecipeAsync(1, 123))
                .ReturnsAsync(true);

            // Act
            var actionResult = await _controller.DeleteRecipe(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var response = Assert.IsType<DeleteRecipeResponse>(okResult.Value);
            Assert.True(response.Success);
            Assert.True(response.Deleted);
        }

        [Fact]
        public async Task DeleteRecipe_ReturnsNotFound_WhenRecipeNotFound()
        {
            // Arrange
            _mockRecipeService.Setup(service => service.DeleteRecipeAsync(1, 123))
                .ReturnsAsync(false);

            // Act
            var actionResult = await _controller.DeleteRecipe(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            var response = Assert.IsType<DeleteRecipeResponse>(notFoundResult.Value);
            Assert.False(response.Success);
            Assert.False(response.Deleted);
        }

        [Fact]
        public async Task GetPendingRecipes_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            // Arrange
            _mockRecipeService.Setup(service => service.GetPendingRecipesAsync())
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var actionResult = await _controller.GetPendingRecipes();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
            Assert.Equal(500, objectResult.StatusCode);
            var response = Assert.IsType<GetRecipesResponse>(objectResult.Value);
            Assert.False(response.Success);
        }

        [Fact]
        public async Task GetUserPendingRecipes_ReturnsBadRequest_WhenUserIdInvalid()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };

            // Act
            var actionResult = await _controller.GetUserPendingRecipes();

            // Assert
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }

        [Fact]
        public async Task GetUserApprovedRecipes_ReturnsBadRequest_WhenUserIdInvalid()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };

            // Act
            var actionResult = await _controller.GetUserApprovedRecipes();

            // Assert
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }

        [Fact]
        public async Task EditRecipe_ReturnsBadRequest_WhenUserIdInvalid()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };
            var editRecipeDto = new EditRecipeDto { RecipeName = "Test Recipe" };

            // Act
            var actionResult = await _controller.EditRecipe(1, editRecipeDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }

        [Fact]
        public async Task DeleteRecipe_ReturnsBadRequest_WhenUserIdInvalid()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };

            // Act
            var actionResult = await _controller.DeleteRecipe(1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        }
    }
}