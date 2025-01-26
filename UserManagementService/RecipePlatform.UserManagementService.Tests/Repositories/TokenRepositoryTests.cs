using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RecipePlatform.UserManagementService.Contracts.DTO;
using RecipePlatform.UserManagementService.Data;
using RecipePlatform.UserManagementService.Data.Entities;
using RecipePlatform.UserManagementService.Data.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RecipePlatform.UserManagementService.Tests.Repositories
{
    public class TokenRepositoryTests
    {
        private readonly TokenRepository _tokenRepository;
        private readonly ApplicationDbContext _context;

        public TokenRepositoryTests()
        {
            // Mock IConfiguration for the test
            var inMemorySettings = new Dictionary<string, string>
            {
                { "ConnectionStrings:DefaultConnection", "server=localhost;database=testdb;user=root;password=password;" }
            };

            // Build configuration, using null-forgiving operator to suppress CS8620 warning
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build(); // Configuration should never be null here
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

            // Setup DbContext with InMemoryDatabase (for testing)
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDatabase")  // Use InMemoryDatabase
                .Options;

            _context = new ApplicationDbContext(options, configuration);
            _tokenRepository = new TokenRepository(_context);

            // Ensuring the database is created for each test
            _context.Database.EnsureCreated();
        }

        // Cleanup after each test
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void Cleanup()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
            _context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task StoreUserTokenAsync_ShouldAddTokenToDatabase()
        {
            // Arrange
            var tokenDto = new TokenDto
            {
                UserId = 1,
                AccessToken = "someAccessToken",
                ExpirationDate = DateTime.UtcNow.AddHours(1),
                TokenType = "jwt-access-token"
            };

            // Act
            await _tokenRepository.StoreUserTokenAsync(tokenDto);

            // Assert
            var userToken = await _context.UserTokens.FirstOrDefaultAsync(ut => ut.UserId == tokenDto.UserId && ut.AccessToken == tokenDto.AccessToken);
            Assert.NotNull(userToken);
            Assert.Equal(tokenDto.UserId, userToken.UserId);
            Assert.Equal(tokenDto.AccessToken, userToken.AccessToken);
            Assert.Equal(tokenDto.ExpirationDate, userToken.ExpirationDateTime);
        }

        [Fact]
        public async Task IsTokenValidAsync_ShouldReturnTrue_WhenTokenIsValid()
        {
            // Arrange
            var tokenDto = new TokenDto
            {
                UserId = 1,
                AccessToken = "validAccessToken",
                ExpirationDate = DateTime.UtcNow.AddHours(1),
                TokenType = "jwt-access-token"
            };
            await _tokenRepository.StoreUserTokenAsync(tokenDto);

            // Act
            var isValid = await _tokenRepository.IsTokenValidAsync(tokenDto.UserId, tokenDto.AccessToken);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public async Task IsTokenValidAsync_ShouldReturnFalse_WhenTokenIsExpired()
        {
            // Arrange
            var tokenDto = new TokenDto
            {
                UserId = 1,
                AccessToken = "expiredAccessToken",
                ExpirationDate = DateTime.UtcNow.AddMinutes(-1),  // Expired token
                TokenType = "jwt-access-token"
            };
            await _tokenRepository.StoreUserTokenAsync(tokenDto);

            // Act
            var isValid = await _tokenRepository.IsTokenValidAsync(tokenDto.UserId, tokenDto.AccessToken);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public async Task IsTokenValidAsync_ShouldReturnFalse_WhenTokenIsInvalidated()
        {
            // Arrange
            var tokenDto = new TokenDto
            {
                UserId = 1,
                AccessToken = "someTokenToInvalidate",
                ExpirationDate = DateTime.UtcNow.AddHours(1),
                TokenType = "jwt-access-token"
            };
            await _tokenRepository.StoreUserTokenAsync(tokenDto);

            // Invalidate the token
            await _tokenRepository.InvalidateTokenAsync(tokenDto.AccessToken);

            // Act
            var isValid = await _tokenRepository.IsTokenValidAsync(tokenDto.UserId, tokenDto.AccessToken);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public async Task InvalidateTokenAsync_ShouldReturnTrue_WhenTokenExistsAndIsNotInvalid()
        {
            // Arrange
            var tokenDto = new TokenDto
            {
                UserId = 1,
                AccessToken = "tokenToInvalidate",
                ExpirationDate = DateTime.UtcNow.AddHours(1),
                TokenType = "jwt-access-token"
            };
            await _tokenRepository.StoreUserTokenAsync(tokenDto);

            // Act
            var result = await _tokenRepository.InvalidateTokenAsync(tokenDto.AccessToken);

            // Assert
            Assert.True(result);

            var userToken = await _context.UserTokens.FirstOrDefaultAsync(ut => ut.AccessToken == tokenDto.AccessToken);
            Assert.True(userToken?.IsInvalid);
        }

        [Fact]
        public async Task InvalidateTokenAsync_ShouldReturnFalse_WhenTokenDoesNotExist()
        {
            // Act
            var result = await _tokenRepository.InvalidateTokenAsync("nonExistentToken");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task InvalidateTokenAsync_ShouldReturnFalse_WhenTokenIsAlreadyInvalid()
        {
            // Arrange
            var tokenDto = new TokenDto
            {
                UserId = 1,
                AccessToken = "alreadyInvalidToken",
                ExpirationDate = DateTime.UtcNow.AddHours(1),
                TokenType = "jwt-access-token"
            };
            await _tokenRepository.StoreUserTokenAsync(tokenDto);

            // Invalidate the token initially
            await _tokenRepository.InvalidateTokenAsync(tokenDto.AccessToken);

            // Act
            var result = await _tokenRepository.InvalidateTokenAsync(tokenDto.AccessToken);

            // Assert
            Assert.False(result);
        }
    }
}
