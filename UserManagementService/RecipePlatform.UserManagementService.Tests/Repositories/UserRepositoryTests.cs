using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RecipePlatform.UserManagementService.Data;
using RecipePlatform.UserManagementService.Data.Entities;
using RecipePlatform.UserManagementService.Data.Repositories;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RecipePlatform.UserManagementService.Tests.Repositories
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UserRepository _repository;

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .Build();

            _context = new ApplicationDbContext(options, configuration);
            _repository = new UserRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var user = new User { UserId = 1, Name = "Test User", Email = "test@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserByEmailAsync("test@example.com");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test User", result.Name);
            Assert.Equal("test@example.com", result.Email);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Act
            var result = await _repository.GetUserByEmailAsync("nonexistent@example.com");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var user = new User { UserId = 1, Name = "Test User", Email = "test@example.com" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetUserByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.UserId);
            Assert.Equal("Test User", result.Name);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Act
            var result = await _repository.GetUserByIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateUserAsync_ReturnsTrue_WhenUserExists()
        {
            // Arrange
            var user = new User { UserId = 1, Name = "Test User", Email = "test@example.com" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            user.Name = "Updated User";

            // Act
            var result = await _repository.UpdateUserAsync(user);

            // Assert
            Assert.True(result);
            var updatedUser = await _context.Users.FindAsync(1);
            Assert.Equal("Updated User", updatedUser.Name);
        }

        [Fact]
        public async Task UpdateUserAsync_ThrowsDbUpdateConcurrencyException_WhenUserDoesNotExist()
        {
            // Arrange
            var user = new User { UserId = 999, Name = "Test User", Email = "test@example.com" };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => _repository.UpdateUserAsync(user));
        }

        [Fact]
        public async Task UpdateUserAsync_ThrowsArgumentNullException_WhenUserIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.UpdateUserAsync(null));
        }
    }
}