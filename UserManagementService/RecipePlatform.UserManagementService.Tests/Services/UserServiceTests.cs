using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using RecipePlatform.UserManagementService.Application.Services;
using RecipePlatform.UserManagementService.Contracts.Interfaces;
using RecipePlatform.UserManagementService.Data;
using RecipePlatform.UserManagementService.Data.Entities;
using RecipePlatform.UserManagementService.Data.Repositories;
using System;
using System.Threading.Tasks;
using Xunit;


namespace RecipePlatform.UserManagementService.Tests.Services
{
    public class UserServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IDeleteUserRepository _userRepository;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            // Set up in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var mockConfiguration = new Mock<IConfiguration>();
            _context = new ApplicationDbContext(options, mockConfiguration.Object);

            _userRepository = new DeleteUserRepository(_context);
            _userService = new UserService(_userRepository);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task DeleteUserAccountAsync_SuccessfulDeletion_ReturnsTrue()
        {
            // Arrange
            var userId = 1;
            var password = "testPassword";
            var salt = Convert.ToBase64String(new byte[16]); // Generate a salt
            var hash = HashPassword(password, Convert.FromBase64String(salt));

            var user = new User
            {
                UserId = userId,
                Name = "Test User",
                Email = "test@example.com",
                PasswordHash = hash,
                PasswordSalt = salt
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userService.DeleteUserAccountAsync(userId, password);

            // Assert
            Assert.True(result);
            Assert.Null(await _context.Users.FindAsync(userId));
        }

        //[Fact]
        //public async Task DeleteUserAccountAsync_UserNotFound_ThrowsException()
        //{
        //    // Arrange
        //    var userId = 1;
        //    var password = "testPassword";

        //    // Act & Assert
        //    await Assert.ThrowsAsync<UserNotFoundException>(() => _userService.DeleteUserAccountAsync(userId, password));
        //}

        [Fact]
        public async Task DeleteUserAccountAsync_IncorrectPassword_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userId = 1;
            var correctPassword = "correctPassword";
            var incorrectPassword = "incorrectPassword";
            var salt = Convert.ToBase64String(new byte[16]); // Generate a salt
            var hash = HashPassword(correctPassword, Convert.FromBase64String(salt));

            var user = new User
            {
                UserId = userId,
                Name = "Test User",
                Email = "test@example.com",
                PasswordHash = hash,
                PasswordSalt = salt
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _userService.DeleteUserAccountAsync(userId, incorrectPassword));
        }

        // Additional tests for private methods
        [Fact]
        public void HashPassword_ReturnsValidHash()
        {
            // Arrange
            string password = "testPassword";
            byte[] salt = new byte[16];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Act
            var hash = HashPassword(password, salt);

            // Assert
            Assert.NotNull(hash);
            Assert.NotEmpty(hash);
            Assert.Equal(28, hash.Length); // Length of Base64 encoded 20-byte hash
        }

        [Fact]
        public void GenerateSalt_ReturnsValidSalt()
        {
            // Act
            var salt = GenerateSalt();

            // Assert
            Assert.NotNull(salt);
            Assert.Equal(16, salt.Length); // The salt should be 16 bytes long
        }

        // Private methods are tested indirectly, but still included for completeness.
        private static string HashPassword(string password, byte[] salt)
        {
            using var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(password, salt, 100000, System.Security.Cryptography.HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);
            return Convert.ToBase64String(hash);
        }

        private static byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }
        [Fact]
        public async Task DeleteUserAccountAsync_SuccessfulDeletion_CallsRepositoryDeleteMethod()
        {
            // Arrange
            var userId = 1;
            var password = "testPassword";
            var salt = Convert.ToBase64String(new byte[16]); // Generate a salt
            var hash = HashPassword(password, Convert.FromBase64String(salt));

            var user = new User
            {
                UserId = userId,
                Name = "Test User",
                Email = "test@example.com",
                PasswordHash = hash,
                PasswordSalt = salt
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var mockRepository = new Mock<IDeleteUserRepository>();
            mockRepository.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(user);
            mockRepository.Setup(r => r.DeleteUserAsync(userId)).ReturnsAsync(true);

            var userService = new UserService(mockRepository.Object);

            // Act
            var result = await userService.DeleteUserAccountAsync(userId, password);

            // Assert
            Assert.True(result);
            mockRepository.Verify(r => r.DeleteUserAsync(userId), Times.Once);
        }
    }
}
