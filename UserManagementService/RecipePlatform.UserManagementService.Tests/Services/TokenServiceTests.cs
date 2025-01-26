using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Moq;
using RecipePlatform.UserManagementService.Application.Services;
using RecipePlatform.UserManagementService.Contracts.DTO;
using Xunit;

namespace RecipePlatform.UserManagementService.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly TokenService _tokenService;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public TokenServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(x => x["Jwt:Secret"]).Returns("your_test_secret_key_here_make_it_long_enough");
            _mockConfiguration.Setup(x => x["Jwt:ExpirationInMinutes"]).Returns("60");

            _tokenService = new TokenService(_mockConfiguration.Object);
        }

        [Fact]
        public void GenerateToken_ValidUser_ReturnsValidTokenDto()
        {
            // Arrange
            var user = new UserDto
            {
                UserId = 1,
                Email = "test@example.com"
            };

            // Act
            var result = _tokenService.GenerateToken(user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.UserId, result.UserId);
            Assert.Equal("jwt-access-token", result.TokenType);
            Assert.True(result.ExpirationDate > DateTime.UtcNow);
            Assert.True(result.ExpirationDate <= DateTime.UtcNow.AddMinutes(61)); // Allow for slight time differences
            Assert.NotNull(result.AccessToken);
            Assert.True(IsValidJwtToken(result.AccessToken));
        }

        [Fact]
        public void GenerateToken_ValidUser_ContainsCorrectClaims()
        {
            // Arrange
            var user = new UserDto
            {
                UserId = 1,
                Email = "test@example.com"
            };

            // Act
            var result = _tokenService.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(result.AccessToken) as JwtSecurityToken;

            Assert.NotNull(jsonToken);
            Assert.Contains(jsonToken.Claims, claim => claim.Type == "nameid" && claim.Value == user.UserId.ToString());
            Assert.Contains(jsonToken.Claims, claim => claim.Type == "email" && claim.Value == user.Email);
        }

        [Fact]
        public void GenerateToken_MissingJwtSecret_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockConfiguration.Setup(x => x["Jwt:Secret"]).Returns(() => null as string);
            var user = new UserDto { UserId = 1, Email = "test@example.com" };

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _tokenService.GenerateToken(user));
        }

        [Fact]
        public void GenerateToken_InvalidExpirationMinutes_UsesDefaultValue()
        {
            // Arrange
            _mockConfiguration.Setup(x => x["Jwt:ExpirationInMinutes"]).Returns("invalid");
            var user = new UserDto { UserId = 1, Email = "test@example.com" };

            // Act
            var result = _tokenService.GenerateToken(user);

            // Assert
            Assert.True(result.ExpirationDate > DateTime.UtcNow.AddMinutes(59));
            Assert.True(result.ExpirationDate <= DateTime.UtcNow.AddMinutes(61));
        }

        private static bool IsValidJwtToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            try
            {
                var tokenS = handler.ReadToken(token) as JwtSecurityToken;
                return (tokenS != null);
            }
            catch
            {
                return false;
            }
        }
    }
}