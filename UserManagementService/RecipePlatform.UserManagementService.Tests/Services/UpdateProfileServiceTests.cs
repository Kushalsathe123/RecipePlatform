using Moq;
using RecipePlatform.UserManagementService.Application.Services;
using RecipePlatform.UserManagementService.Contracts.DTOs;
using RecipePlatform.UserManagementService.Contracts.Interfaces;
using RecipePlatform.UserManagementService.Data.Entities;
using RecipePlatform.UserManagementService.Data.Repositories;
using Xunit;

namespace RecipePlatform.UserManagementService.Tests.Services
{
    public class UpdateProfileServiceTests
    {
        private readonly Mock<IUpdateUserProfileRepository> _mockRepository;
        private readonly Mock<IPasswordService> _mockPasswordService;
        private readonly UpdateProfileService _service;
        private static readonly string[] ExpectedDietPreferences = { "Vegan" };
        private static readonly string[] ExpectedFavoriteCuisines = { "Mexican" };

        public UpdateProfileServiceTests()
        {
            _mockRepository = new Mock<IUpdateUserProfileRepository>();
            _mockPasswordService = new Mock<IPasswordService>();
            _service = new UpdateProfileService(_mockRepository.Object, _mockPasswordService.Object);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_ThrowsArgumentException_WhenUserDtoIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateUserProfileAsync(1, new UpdateUserProfileDto()));
        }

        
        [Fact]
        public async Task UpdateUserProfileAsync_ThrowsArgumentException_WhenUserNameIsNullOrWhiteSpace()
        {
            // Arrange
            var userDto = new UpdateUserProfileDto
            {
                Name = "   "
            };

            _mockRepository.Setup(repo => repo.GetUserByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new User { UserId = 1 });

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateUserProfileAsync(1, userDto));
        }

        [Fact]
        public async Task UpdateUserProfileAsync_UpdatesUserDetails_WhenInputIsValid()
        {
            // Arrange
            var user = new User
            {
                UserId = 1,
                Name = "Old Name",
                DietPreferences = new[] { "Vegetarian" },
                FavoriteCuisines = new[] { "Italian" }
            };

            var userDto = new UpdateUserProfileDto
            {
                Name = "New Name",
                DietPreferences = ExpectedDietPreferences,
                FavoriteCuisines = ExpectedFavoriteCuisines
            };

            _mockRepository.Setup(repo => repo.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(user);

            // Act
            await _service.UpdateUserProfileAsync(1, userDto);

            // Assert
            Assert.Equal("New Name", user.Name);
            Assert.Equal(ExpectedDietPreferences, user.DietPreferences);
            Assert.Equal(ExpectedFavoriteCuisines, user.FavoriteCuisines);
            _mockRepository.Verify(repo => repo.UpdateUserAsync(It.Is<User>(u =>
                u.Name == "New Name" &&
                u.DietPreferences.SequenceEqual(ExpectedDietPreferences) &&
                u.FavoriteCuisines.SequenceEqual(ExpectedFavoriteCuisines)
            )), Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_ThrowsArgumentException_WhenPasswordDtoIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ChangePasswordAsync(1, null));
        }

        [Fact]
        public async Task ChangePasswordAsync_ThrowsUnauthorizedAccessException_WhenCurrentPasswordIsEmpty()
        {
            // Arrange
            var passwordDto = new ChangePasswordDto
            {
                CurrentPassword = "",
                NewPassword = "NewPassword123",
                ConfirmNewPassword = "NewPassword123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.ChangePasswordAsync(1, passwordDto));
        }

        [Fact]
        public async Task ChangePasswordAsync_ThrowsArgumentException_WhenPasswordsDoNotMatch()
        {
            // Arrange
            var passwordDto = new ChangePasswordDto
            {
                CurrentPassword = "OldPassword123",
                NewPassword = "NewPassword123",
                ConfirmNewPassword = "MismatchPassword123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ChangePasswordAsync(1, passwordDto));
        }

        [Fact]
        public async Task ChangePasswordAsync_ThrowsUserNotFoundException_WhenUserDoesNotExist()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync((User?)null);

            var passwordDto = new ChangePasswordDto
            {
                CurrentPassword = "OldPassword123",
                NewPassword = "NewPassword123",
                ConfirmNewPassword = "NewPassword123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UpdateProfileService.UserNotFoundException>(() => _service.ChangePasswordAsync(1, passwordDto));
        }

        [Fact]
        public async Task ChangePasswordAsync_ThrowsUnauthorizedAccessException_WhenCurrentPasswordIsIncorrect()
        {
            // Arrange
            var user = new User
            {
                UserId = 1,
                PasswordHash = "StoredHash",
                PasswordSalt = "StoredSalt"
            };

            _mockRepository.Setup(repo => repo.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(user);
            _mockPasswordService.Setup(service => service.VerifyPassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            var passwordDto = new ChangePasswordDto
            {
                CurrentPassword = "IncorrectPassword",
                NewPassword = "NewPassword123",
                ConfirmNewPassword = "NewPassword123"
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.ChangePasswordAsync(1, passwordDto));
        }

        [Fact]
        public async Task ChangePasswordAsync_UpdatesPassword_WhenValid()
        {
            // Arrange
            var user = new User
            {
                UserId = 1,
                PasswordHash = "OldHash",
                PasswordSalt = "OldSalt"
            };

            _mockRepository.Setup(repo => repo.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync(user);
            _mockPasswordService.Setup(service => service.VerifyPassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            var passwordDto = new ChangePasswordDto
            {
                CurrentPassword = "OldPassword123",
                NewPassword = "NewPassword123",
                ConfirmNewPassword = "NewPassword123"
            };

            // Act
            await _service.ChangePasswordAsync(1, passwordDto);

            // Assert
            Assert.NotEqual("OldHash", user.PasswordHash);
            Assert.NotEqual("OldSalt", user.PasswordSalt);
            _mockRepository.Verify(repo => repo.UpdateUserAsync(It.IsAny<User>()), Times.Once);
        }
    }
}
