using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using RecipePlatform.UserManagementService.Application.Services;
using RecipePlatform.UserManagementService.Contracts.DTO;
using RecipePlatform.UserManagementService.Contracts.Interfaces;
using RecipePlatform.UserManagementService.Data;
using RecipePlatform.UserManagementService.Data.Entities;
using RecipePlatform.UserManagementService.Data.Repositories;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RecipePlatform.UserManagementService.Tests.Services
{
    public class PasswordResetUrlGeneratorTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ITokenRepository _tokenRepository;
        private readonly PasswordResetUrlGenerator _urlGenerator;

        public PasswordResetUrlGeneratorTests()
        {
            // Set up in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options, Mock.Of<IConfiguration>());

            // Set up configuration
            var inMemorySettings = new Dictionary<string, string> {
                {"Jwt:Secret", "your_test_secret_key_which_is_long_enough_for_testing"},
                {"Jwt:ExpirationInMinutes", "60"},
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Set up token repository
            _tokenRepository = new TokenRepository(_context);

            // Create the PasswordResetUrlGenerator
            _urlGenerator = new PasswordResetUrlGenerator(_configuration, _tokenRepository);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GeneratePasswordResetUrl_CreatesValidUrl()
        {
            // Arrange
            int userId = 1;

            // Act
            string resetUrl = _urlGenerator.GeneratePasswordResetUrl(userId);

            // Assert
            Assert.StartsWith("https://recipeaugust10-angularui-cmahaaddc0e7hqe9.westeurope-01.azurewebsites.net/create-new-password?token=", resetUrl);

            // Verify token is stored
            var storedToken = await _context.UserTokens.FirstOrDefaultAsync(t => t.UserId == userId);
            Assert.NotNull(storedToken);
            Assert.Equal("jwt-access-token", storedToken.TokenType);
        }

        [Fact]
        public void GeneratePasswordResetUrl_CreatesValidToken()
        {
            // Arrange
            int userId = 1;

            // Act
            string resetUrl = _urlGenerator.GeneratePasswordResetUrl(userId);

            // Extract token from URL
            var token = resetUrl.Split("token=")[1];
            token = Uri.UnescapeDataString(token);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            Assert.NotNull(jsonToken);
            Assert.Contains(jsonToken.Claims, claim => claim.Type == "nameid" && claim.Value == userId.ToString());
        }

        [Fact]
        public void GeneratePasswordResetUrl_UsesConfiguredExpirationTime()
        {
            // Arrange
            int userId = 1;
            int expectedExpirationMinutes = 60; // As set in the configuration

            // Act
            string resetUrl = _urlGenerator.GeneratePasswordResetUrl(userId);

            // Extract token from URL
            var token = resetUrl.Split("token=")[1];
            token = Uri.UnescapeDataString(token);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            Assert.NotNull(jsonToken);
            Assert.Equal(DateTime.UtcNow.AddMinutes(expectedExpirationMinutes).ToString("yyyyMMddHHmm"),
                         jsonToken.ValidTo.ToString("yyyyMMddHHmm"));
        }

        [Fact]
        public void GeneratePasswordResetUrl_ThrowsException_WhenJwtSecretNotConfigured()
        {
            // Arrange
            var invalidConfiguration = new ConfigurationBuilder().Build(); // Empty configuration
            var invalidGenerator = new PasswordResetUrlGenerator(invalidConfiguration, _tokenRepository);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => invalidGenerator.GeneratePasswordResetUrl(1));
        }
    }
}