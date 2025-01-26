using Xunit;
using RecipePlatform.RecipeManagementService.Application.Mappers;
using RecipePlatform.RecipeManagementService.Data.Entities;
using RecipePlatform.RecipeManagementService.Contracts.DTO;
using System;
using System.Collections.Generic;

namespace RecipePlatform.RecipeManagementService.Tests.Application.Mappers
{
    public class RecipeMapperTests
    {
        [Fact]
        public void ToDto_WithValidRecipe_ReturnsCorrectDto()
        {
            // Arrange
            var recipe = CreateValidRecipe();

            // Act
            var result = RecipeMapper.ToDto(recipe);

            // Assert
            AssertValidRecipeDto(recipe, result);
        }

        [Fact]
        public void ToDto_WithNullRecipe_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => RecipeMapper.ToDto(null!));
        }

        //[Fact]
        //public void ToDto_WithDefaultProperties_ReturnsExpectedDto()
        //{
        //    // Arrange
        //    var recipe = new Recipe();

        //    // Act
        //    var result = RecipeMapper.ToDto(recipe);

        //    // Assert
        //    AssertDefaultRecipeDto(result);
        //}

        [Fact]
        public void ToDto_WithNestedComments_MapsCorrectly()
        {
            // Arrange
            var recipe = CreateRecipeWithNestedComments();

            // Act
            var result = RecipeMapper.ToDto(recipe);

            // Assert
            AssertNestedComments(result);
        }

        private static Recipe CreateValidRecipe()
        {
            return new Recipe
            {
                RecipeId = 1,
                UserId = 10,
                RecipeName = "Test Recipe",
                Description = "Test Description",
                ImageUrls = new List<string> { "url1", "url2" },
                Ingredients = new List<string> { "ingredient1", "ingredient2" },
                Instructions = "Test Instructions",
                CookingTime = "30 minutes",
                Tags = new List<string> { "tag1", "tag2" },
                CreatedAt = DateTime.UtcNow,
                Status = "Approved",
                ApprovedAt = null,
                LikesCount = 5,
                Comments = new List<Comment>
                {
                    new Comment
                    {
                        UserName = "User1",
                        Text = "Great recipe!",
                        CreatedAt = DateTime.UtcNow,
                        Replies = new List<Comment>()
                    }
                },
                Feedback = "Test Feedback"
            };
        }

        private static Recipe CreateRecipeWithNestedComments()
        {
            return new Recipe
            {
                RecipeId = 1,
                Comments = new List<Comment>
                {
                    new Comment
                    {
                        UserName = "User1",
                        Text = "Parent Comment",
                        CreatedAt = DateTime.UtcNow,
                        Replies = new List<Comment>
                        {
                            new Comment
                            {
                                UserName = "User2",
                                Text = "Reply Comment",
                                CreatedAt = DateTime.UtcNow,
                                Replies = new List<Comment>()
                            }
                        }
                    }
                }
            };
        }

        private static void AssertValidRecipeDto(Recipe recipe, RecipeDto result)
        {
            Assert.NotNull(result);
            Assert.Equal(recipe.RecipeId, result.RecipeId);
            Assert.Equal(recipe.UserId, result.UserId);
            Assert.Equal(recipe.RecipeName, result.RecipeName);
            Assert.Equal(recipe.Description, result.Description);
            Assert.Equal(recipe.ImageUrls, result.ImageUrls);
            Assert.Equal(recipe.Ingredients, result.Ingredients);
            Assert.Equal(recipe.Instructions, result.Instructions);
            Assert.Equal(recipe.CookingTime, result.CookingTime);
            Assert.Equal(recipe.Tags, result.Tags);
            Assert.Equal(recipe.CreatedAt, result.CreatedAt);
            Assert.Equal(recipe.Status, result.Status);
            Assert.Equal(recipe.ApprovedAt, result.ApprovedAt);
            Assert.Equal(recipe.LikesCount, result.LikesCount);
            Assert.Equal(recipe.Feedback, result.Feedback);
            Assert.Single(result.Comments);
            Assert.Equal(recipe.Comments[0].UserName, result.Comments[0].Username);
            Assert.Equal(recipe.Comments[0].Text, result.Comments[0].Text);
            Assert.Equal(recipe.Comments[0].CreatedAt, result.Comments[0].CreatedAt);
        }

        private static void AssertDefaultRecipeDto(RecipeDto result)
        {
            Assert.NotNull(result);
            Assert.Equal(0, result.RecipeId);
            Assert.Equal(0, result.UserId);
            Assert.Equal(string.Empty, result.RecipeName);
            Assert.Equal(string.Empty, result.Description);
            Assert.Empty(result.ImageUrls);
            Assert.Empty(result.Ingredients);
            Assert.Equal(string.Empty, result.Instructions);
            Assert.Equal(string.Empty, result.CookingTime);
            Assert.Empty(result.Tags);
            Assert.Equal("Pending", result.Status); // Updated to expect "Pending"
            Assert.Null(result.ApprovedAt);
            Assert.Equal(0, result.LikesCount);
            Assert.Empty(result.Comments);
            Assert.Null(result.Feedback);
        }

        private static void AssertNestedComments(RecipeDto result)
        {
            Assert.NotNull(result);
            Assert.Single(result.Comments);
            Assert.Single(result.Comments[0].Replies);
            Assert.Equal("User1", result.Comments[0].Username);
            Assert.Equal("Parent Comment", result.Comments[0].Text);
            Assert.Equal("User2", result.Comments[0].Replies[0].Username);
            Assert.Equal("Reply Comment", result.Comments[0].Replies[0].Text);
        }
    }
}