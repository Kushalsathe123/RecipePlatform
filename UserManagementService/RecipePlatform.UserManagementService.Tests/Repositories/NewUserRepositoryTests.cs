using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RecipePlatform.UserManagementService.Data;
using RecipePlatform.UserManagementService.Data.Entities;
using RecipePlatform.UserManagementService.Data.Repositories;
using Xunit;

namespace RecipePlatform.UserManagementService.Tests.Repositories
{
    public class NewUserRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

        public NewUserRepositoryTests()
        {
            // Configure in-memory database for testing
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDb_NewUserRepository")
                .Options;
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsUser_WhenEmailExists()
        {
            // Arrange
            var email = "test@example.com";
            var user = new User
            {
                UserId = 1,
                Email = email,
                Name = "Test User"
            };

            using (var context = new ApplicationDbContext(_dbContextOptions, NullLogger<ApplicationDbContext>.Instance))
            {
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(_dbContextOptions, NullLogger<ApplicationDbContext>.Instance))
            {
                var repository = new NewUserRepository(context);

                // Act
                var result = await repository.GetUserByEmailAsync(email);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(email, result.Email);
            }
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsNull_WhenEmailDoesNotExist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions, NullLogger<ApplicationDbContext>.Instance);
            var repository = new NewUserRepository(context);

            // Act
            var result = await repository.GetUserByEmailAsync("nonexistent@example.com");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser_WhenUserIdExists()
        {
            // Arrange
            var userId = 2;
            var user = new User
            {
                UserId = userId,
                Email = "test@example.com",
                Name = "Test User"
            };

            using (var context = new ApplicationDbContext(_dbContextOptions, NullLogger<ApplicationDbContext>.Instance))
            {
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }

            using (var context = new ApplicationDbContext(_dbContextOptions, NullLogger<ApplicationDbContext>.Instance))
            {
                var repository = new NewUserRepository(context);

                // Act
                var result = await repository.GetUserByIdAsync(userId);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(userId, result.UserId);
            }
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsNull_WhenUserIdDoesNotExist()
        {
            // Arrange
            using var context = new ApplicationDbContext(_dbContextOptions, NullLogger<ApplicationDbContext>.Instance);
            var repository = new NewUserRepository(context);

            // Act
            var result = await repository.GetUserByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddUserAsync_AddsUserToDatabase()
        {
            // Arrange
            var user = new User
            {
                UserId = 3,
                Email = "test@example.com",
                Name = "Test User"
            };

            using (var context = new ApplicationDbContext(_dbContextOptions, NullLogger<ApplicationDbContext>.Instance))
            {
                var repository = new NewUserRepository(context);

                // Act
                await repository.AddUserAsync(user);
            }

            using (var context = new ApplicationDbContext(_dbContextOptions, NullLogger<ApplicationDbContext>.Instance))
            {
                // Assert
                var addedUser = await context.Users.FindAsync(1);
                Assert.NotNull(addedUser);
                Assert.Equal("test@example.com", addedUser.Email);
            }
        }
    }
}
