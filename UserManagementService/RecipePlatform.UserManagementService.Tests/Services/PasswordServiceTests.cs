using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using RecipePlatform.UserManagementService.Application.Services;
using RecipePlatform.UserManagementService.Contracts.DTO;
using RecipePlatform.UserManagementService.Contracts.DTOs;
using RecipePlatform.UserManagementService.Contracts.Interfaces;
using RecipePlatform.UserManagementService.Data;
using RecipePlatform.UserManagementService.Data.Entities;
using RecipePlatform.UserManagementService.Data.Repositories;
using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Xunit;

namespace RecipePlatform.UserManagementService.Tests.Services
{
    public class PasswordServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordResetUrlGenerator _urlGenerator;
        private readonly PasswordService _passwordService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<ITokenRepository> _mockTokenRepository;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;

        public PasswordServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c["Jwt:Secret"]).Returns("your_mocked_jwt_secret_at_least_128_bits_long");
            _mockConfiguration.Setup(c => c["Jwt:ExpirationInMinutes"]).Returns("60");

            _context = new ApplicationDbContext(options, _mockConfiguration.Object);

            _mockTokenRepository = new Mock<ITokenRepository>();
            _mockTokenRepository.Setup(r => r.StoreUserTokenAsync(It.IsAny<TokenDto>())).Returns(Task.CompletedTask);

            _urlGenerator = new PasswordResetUrlGenerator(_mockConfiguration.Object, _mockTokenRepository.Object);

            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });
            var client = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            _passwordService = new PasswordService(_context, _urlGenerator);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void VerifyPassword_ReturnsTrue_WhenPasswordIsCorrect()
        {
            // Arrange
            var password = "correctPassword";
            var salt = Convert.ToBase64String(new byte[16]); // Generate a salt
            var hash = ComputeHash(password, salt);

            // Act
            var result = _passwordService.VerifyPassword(password, hash, salt);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_ReturnsFalse_WhenPasswordIsIncorrect()
        {
            // Arrange
            var correctPassword = "correctPassword";
            var incorrectPassword = "incorrectPassword";
            var salt = Convert.ToBase64String(new byte[16]); // Generate a salt
            var hash = ComputeHash(correctPassword, salt);

            // Act
            var result = _passwordService.VerifyPassword(incorrectPassword, hash, salt);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ForgotPasswordAsync_ReturnsUserNotFoundMessage_WhenUserDoesNotExist()
        {
            // Act
            var result = await _passwordService.ForgotPasswordAsync("nonexistent@example.com");

            // Assert
            Assert.Equal("User not found.", result);
        }

        [Fact]
        public async Task ResetPasswordAsync_ReturnsSuccessMessage()
        {
            // Arrange
            var resetPasswordDto = new ResetPasswordDto { Token = "validToken", NewPassword = "newPassword123" };

            // Act
            var result = await _passwordService.ResetPasswordAsync(resetPasswordDto);

            // Assert
            Assert.Equal("Password successfully reset.", result);
        }

        private string ComputeHash(string password, string salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 100000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);
            return Convert.ToBase64String(hash);
        }
    }
}