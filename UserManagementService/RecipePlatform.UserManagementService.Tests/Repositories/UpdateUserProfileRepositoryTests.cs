using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RecipePlatform.UserManagementService.Data;
using RecipePlatform.UserManagementService.Data.Entities;
using RecipePlatform.UserManagementService.Data.Repositories;
using System.Threading.Tasks;
using Xunit;


namespace RecipePlatform.UserManagementService.Tests.Repositories
{
    public class UpdateUserProfileRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private static readonly string[] ExpectedDietPreferences = { "Vegan" };
        private static readonly string[] ExpectedFavoriteCuisines = { "Mexican" };

        public UpdateUserProfileRepositoryTests()
        {
            // In-memory database for testing
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options, NullLogger<ApplicationDbContext>.Instance);
            var repository = new UpdateUserProfileRepository(context);

            var user = new User
            {
                UserId = 1,
                Name = "John Doe",
                Email = "john.doe@example.com"
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetUserByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John Doe", result.Name);
            Assert.Equal("john.doe@example.com", result.Email);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options, NullLogger<ApplicationDbContext>.Instance);
            var repository = new UpdateUserProfileRepository(context);

            // Act
            var result = await repository.GetUserByIdAsync(99);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateUserAsync_UpdatesUserDetails_WhenUserExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options, NullLogger<ApplicationDbContext>.Instance);
            var repository = new UpdateUserProfileRepository(context);

            var user = new User
            {
                UserId = 2,
                Name = "John Doe",
                DietPreferences = new[] { "Vegetarian" },
                FavoriteCuisines = new[] { "Italian" }
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var updatedUser = new User
            {
                UserId = 2,
                Name = "Jane Doe",
                DietPreferences = ExpectedDietPreferences,
                FavoriteCuisines = ExpectedFavoriteCuisines
            };

            // Act
            await repository.UpdateUserAsync(updatedUser);

            var result = await context.Users.FindAsync(2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Jane Doe", result.Name);
            Assert.Equal(ExpectedDietPreferences, result.DietPreferences); // Use the static readonly field
            Assert.Equal(ExpectedFavoriteCuisines, result.FavoriteCuisines); // Use the static readonly field
        }

        [Fact]
        public async Task UpdateUserAsync_ThrowsDbUpdateConcurrencyException_WhenUserDoesNotExist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options, NullLogger<ApplicationDbContext>.Instance);
            var repository = new UpdateUserProfileRepository(context);

            var user = new User
            {
                UserId = 99,
                Name = "Non-existent User",
                DietPreferences = new[] { "Keto" },
                FavoriteCuisines = new[] { "Indian" }
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => repository.UpdateUserAsync(user));
        }
    }
}