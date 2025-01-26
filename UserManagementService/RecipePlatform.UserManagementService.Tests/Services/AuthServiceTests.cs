using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using RecipePlatform.UserManagementService.Application.Services;
using RecipePlatform.UserManagementService.Contracts.DTO;
using RecipePlatform.UserManagementService.Contracts.DTOs;
using RecipePlatform.UserManagementService.Contracts.Interfaces;
using RecipePlatform.UserManagementService.Data.Entities;
using RecipePlatform.UserManagementService.Data.Repositories;
using Xunit;

namespace RecipePlatform.UserManagementService.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ITokenRepository> _mockTokenRepository;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IPasswordService> _mockPasswordService;
        private readonly Mock<ITokenService> _mockTokenService;
        private readonly Mock<INewUserRepository> _mockNewUserRepository;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockTokenRepository = new Mock<ITokenRepository>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockPasswordService = new Mock<IPasswordService>();
            _mockTokenService = new Mock<ITokenService>();
            _mockNewUserRepository = new Mock<INewUserRepository>();

            _mockConfiguration.Setup(x => x["Jwt:Secret"]).Returns("your_test_secret_key_here_make_it_long_enough");
            _mockConfiguration.Setup(x => x["Jwt:ExpirationInMinutes"]).Returns("60");

            _authService = new AuthService(
                _mockUserRepository.Object,
                _mockTokenRepository.Object,
                _mockTokenService.Object,
                _mockPasswordService.Object,
                _mockNewUserRepository.Object
            );
        }

        [Theory]
        [InlineData("test@example.com", "password", 1, "jwt-access-token")]
        public async Task LoginAsync_ValidCredentials_ReturnsNameAndTokenDto(string email, string password, int expectedUserId, string expectedTokenType)
        {
            // Arrange
            var loginDto = new LoginDto { Email = email, Password = password };
            var expectedTokenDto = new TokenDto
            {
                UserId = expectedUserId,
                AccessToken = "test_token",
                ExpirationDate = DateTime.UtcNow.AddHours(1),
                TokenType = expectedTokenType
            };
            var expectedName = "Test User";

            _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync(new UserDto { UserId = expectedUserId, Name = expectedName, Email = email });
            _mockPasswordService.Setup(x => x.VerifyPassword(password, It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);
            _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<UserDto>()))
                .Returns(expectedTokenDto);

            // Act
            var result = await _authService.LoginAsync(loginDto);

            // Assert
            Assert.Equal(expectedName, result.Name);
            Assert.NotNull(result.Token);
            Assert.Equal(expectedUserId, result.Token.UserId);
            Assert.Equal(expectedTokenDto.AccessToken, result.Token.AccessToken);
            Assert.Equal(expectedTokenType, result.Token.TokenType);
            Assert.Equal(expectedTokenDto.ExpirationDate, result.Token.ExpirationDate);

            _mockTokenRepository.Verify(x => x.StoreUserTokenAsync(It.Is<TokenDto>(t => t.AccessToken == expectedTokenDto.AccessToken)), Times.Once);
        }

        [Theory]
        [InlineData("test@example.com", "wrongpassword")]
        public async Task LoginAsync_InvalidCredentials_ThrowsUnauthorizedAccessException(string email, string password)
        {
            // Arrange
            var loginDto = new LoginDto { Email = email, Password = password };
            var userDto = new UserDto
            {
                UserId = 1,
                Email = email,
                PasswordHash = "hashed_password",
                PasswordSalt = "salt"
            };

            _mockUserRepository.Setup(x => x.GetUserByEmailAsync(loginDto.Email))
                .ReturnsAsync(userDto);

            _mockPasswordService.Setup(x => x.VerifyPassword(password, userDto.PasswordHash, userDto.PasswordSalt))
                        .Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.LoginAsync(loginDto));

            _mockTokenRepository.Verify(x => x.StoreUserTokenAsync(It.IsAny<TokenDto>()), Times.Never);
        }

        [Theory]
        [InlineData("nonexistent@example.com", "password123")]
        public async Task LoginAsync_UserNotFound_ThrowsKeyNotFoundException(string email, string password)
        {
            // Arrange
            var loginDto = new LoginDto { Email = email, Password = password };

            _mockUserRepository.Setup(x => x.GetUserByEmailAsync(It.IsAny<string>()))
                    .ReturnsAsync((UserDto?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _authService.LoginAsync(loginDto));

            _mockTokenRepository.Verify(x => x.StoreUserTokenAsync(It.IsAny<TokenDto>()), Times.Never);
        }

        [Theory]
        [InlineData("valid_token", true)]
        [InlineData("invalid_token", false)]
        public async Task LogoutAsync_ReturnsExpectedResult(string token, bool expectedResult)
        {
            // Arrange
            _mockTokenRepository.Setup(x => x.InvalidateTokenAsync(token))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _authService.LogoutAsync(token);

            // Assert
            Assert.Equal(expectedResult, result);
            _mockTokenRepository.Verify(x => x.InvalidateTokenAsync(token), Times.Once);
        }

        [Theory]
        [InlineData("existing@example.com", true, typeof(ArgumentException))]
        [InlineData("newuser@example.com", false, null)]
        public async Task RegisterUserAsync_EmailCheck_AddsOrThrowsException(
    string email, bool userExists, Type? expectedException)
        {
            // Arrange
            var registerUserDto = new RegisterUserDto
            {
                Name = "Test User",
                Email = email,
                Password = "password123",
                ConfirmPassword = "password123"
            };

            _mockNewUserRepository.Setup(x => x.GetUserByEmailAsync(email))
                .ReturnsAsync(userExists ? new User { Email = email } : null);

            // Act & Assert
            if (expectedException != null)
            {
                await Assert.ThrowsAsync(expectedException, () => _authService.RegisterUserAsync(registerUserDto));
                _mockNewUserRepository.Verify(x => x.AddUserAsync(It.IsAny<User>()), Times.Never);
            }
            else
            {
                await _authService.RegisterUserAsync(registerUserDto);
                _mockNewUserRepository.Verify(x => x.AddUserAsync(It.Is<User>(u => u.Email == email)), Times.Once);
            }
        }

        [Theory]
        [InlineData("newuser@example.com", "ValidPassword@123", null)]
        public async Task RegisterUserAsync_PasswordValidation_NotThrowsForValidPassword(
    string email, string password, Type? expectedException)
        {
            // Arrange
            var registerUserDto = new RegisterUserDto
            {
                Name = "Test User",
                Email = email,
                Password = password,
                ConfirmPassword = password
            };

            _mockNewUserRepository.Setup(x => x.GetUserByEmailAsync(email)).ReturnsAsync((User?)null);

            // Act & Assert
            if (expectedException != null)
            {
                await Assert.ThrowsAsync(expectedException, () => _authService.RegisterUserAsync(registerUserDto));
                _mockNewUserRepository.Verify(x => x.AddUserAsync(It.IsAny<User>()), Times.Never);
            }
            else
            {
                await _authService.RegisterUserAsync(registerUserDto);
                _mockNewUserRepository.Verify(x => x.AddUserAsync(It.Is<User>(u => u.Email == email)), Times.Once);
            }
        }


        [Theory]
        [InlineData("newuser@example.com", "password123")]
        public async Task RegisterUserAsync_GeneratesSaltAndHash(string email, string password)
        {
            // Arrange
            var registerUserDto = new RegisterUserDto 
            {
                Name = "Test User", 
                Email = email, 
                Password = password, 
                ConfirmPassword = password 
            };
            _mockNewUserRepository.Setup(x => x.GetUserByEmailAsync(email)).ReturnsAsync((User?)null);

            // Act
            await _authService.RegisterUserAsync(registerUserDto);

            // Assert
            _mockNewUserRepository.Verify(x => x.AddUserAsync(It.Is<User>(u =>
                !string.IsNullOrEmpty(u.PasswordSalt) &&
                !string.IsNullOrEmpty(u.PasswordHash))), Times.Once);
        }

    }
}