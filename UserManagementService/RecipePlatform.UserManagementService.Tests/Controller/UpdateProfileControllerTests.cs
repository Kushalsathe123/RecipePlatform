using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json.Linq;
using RecipePlatform.UserManagementService.Api.Controllers;
using RecipePlatform.UserManagementService.Contracts.DTOs;
using RecipePlatform.UserManagementService.Contracts.Interfaces;
using Xunit;

namespace RecipePlatform.UserManagementService.Tests.Controller
{
    public class UpdateProfileControllerTests
    {
        private readonly Mock<IUpdateProfileService> _mockUpdateProfileService;
        private readonly UpdateProfileController _controller;

        public UpdateProfileControllerTests()
        {
            _mockUpdateProfileService = new Mock<IUpdateProfileService>();
            _controller = new UpdateProfileController(_mockUpdateProfileService.Object);
        }

        [Theory]
        [InlineData(null)]
        public async Task UpdateProfileDetails_ReturnsBadRequest_WhenDtoIsNull(UpdateUserProfileDto? dto)
        {
            // Act
            var result = await _controller.UpdateProfileDetails(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var jsonResult = JObject.FromObject(badRequestResult.Value ?? new { });
            Assert.Equal("Request body cannot be empty", jsonResult["message"]?.ToString());
        }

        [Fact]
        public async Task UpdateProfileDetails_ReturnsOk_WhenProfileUpdatedSuccessfully()
        {
            // Arrange
            var updateDto = new UpdateUserProfileDto
            {
                UserId = 1,
                Name = "Updated Name",
                DietPreferences = new[] { "Vegan" },
                FavoriteCuisines = new[] { "Italian" }
            };

            _mockUpdateProfileService
                .Setup(service => service.UpdateUserProfileAsync(It.IsAny<int>(), It.IsAny<UpdateUserProfileDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateProfileDetails(updateDto);

            // Assert
            var okRequestResult = Assert.IsType<OkObjectResult>(result);
            var jsonResult = JObject.FromObject(okRequestResult.Value ?? new { });
            Assert.Equal("Profile details updated successfully", jsonResult["message"]?.ToString());
        }

        [Fact]
        public async Task UpdateProfileDetails_ReturnsBadRequest_WhenServiceThrowsArgumentException()
        {
            // Arrange
            var updateDto = new UpdateUserProfileDto { UserId = 1 };
            _mockUpdateProfileService
                .Setup(service => service.UpdateUserProfileAsync(It.IsAny<int>(), It.IsAny<UpdateUserProfileDto>()))
                .Throws(new ArgumentException("User name is required."));

            // Act
            var result = await _controller.UpdateProfileDetails(updateDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var jsonResult = JObject.FromObject(badRequestResult.Value ?? new { });
            Assert.Equal("User name is required.", jsonResult["message"]?.ToString());
        }

        [Fact]
        public async Task ChangePassword_ReturnsOk_WhenPasswordChangedSuccessfully()
        {
            // Arrange
            var passwordDto = new ChangePasswordDto
            {
                UserId = 1,
                CurrentPassword = "OldPassword123",
                NewPassword = "NewPassword123",
                ConfirmNewPassword = "NewPassword123"
            };

            _mockUpdateProfileService
                .Setup(service => service.ChangePasswordAsync(It.IsAny<int>(), It.IsAny<ChangePasswordDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ChangePassword(passwordDto);

            // Assert
            var okRequestResult = Assert.IsType<OkObjectResult>(result);
            var jsonResult = JObject.FromObject(okRequestResult.Value ?? new { });
            Assert.Equal("Password changed successfully", jsonResult["message"]?.ToString());
        }

        [Fact]
        public async Task ChangePassword_ReturnsUnauthorized_WhenCurrentPasswordIsIncorrect()
        {
            // Arrange
            var passwordDto = new ChangePasswordDto
            {
                UserId = 1,
                CurrentPassword = "WrongPassword",
                NewPassword = "NewPassword123",
                ConfirmNewPassword = "NewPassword123"
            };

            _mockUpdateProfileService
                .Setup(service => service.ChangePasswordAsync(It.IsAny<int>(), It.IsAny<ChangePasswordDto>()))
                .Throws(new UnauthorizedAccessException("The current password is incorrect."));

            // Act
            var result = await _controller.ChangePassword(passwordDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var jsonResult = JObject.FromObject(unauthorizedResult.Value ?? new { });
            Assert.Equal("The current password is incorrect.", jsonResult["message"]?.ToString());
        }

        [Fact]
        public async Task ChangePassword_ReturnsOk_WhenPasswordIsChangedSuccessfully()
        {
            // Arrange
            var passwordDto = new ChangePasswordDto
            {
                UserId = 1,
                CurrentPassword = "OldPassword123",
                NewPassword = "NewPassword123",
                ConfirmNewPassword = "NewPassword123"
            };

            _mockUpdateProfileService
                .Setup(service => service.ChangePasswordAsync(passwordDto.UserId.Value, passwordDto))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.ChangePassword(passwordDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var jsonResult = JObject.FromObject(okResult.Value ?? new { });
            Assert.Equal("Password changed successfully", jsonResult["message"]?.ToString());
        }


        [Fact]
        public async Task ChangePassword_ReturnsBadRequest_WhenDtoIsNull()
        {
            // Act
            var result = await _controller.ChangePassword(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var jsonResult = JObject.FromObject(badRequestResult.Value ?? new { });
            Assert.Equal("Request body cannot be empty", jsonResult["message"]?.ToString());
        }

        [Fact]
        public async Task ChangePassword_ReturnsUnauthorized_WhenUnauthorizedAccessExceptionIsThrown()
        {
            // Arrange
            var passwordDto = new ChangePasswordDto
            {
                UserId = 1,
                CurrentPassword = "WrongPassword123",
                NewPassword = "NewPassword123",
                ConfirmNewPassword = "NewPassword123"
            };

            _mockUpdateProfileService
                .Setup(service => service.ChangePasswordAsync(passwordDto.UserId.Value, passwordDto))
                .ThrowsAsync(new UnauthorizedAccessException("Invalid current password"));

            // Act
            var result = await _controller.ChangePassword(passwordDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var jsonResult = JObject.FromObject(unauthorizedResult.Value ?? new { });
            Assert.Equal("Invalid current password", jsonResult["message"]?.ToString());
        }

        [Fact]
        public async Task ChangePassword_ReturnsBadRequest_WhenArgumentExceptionIsThrown()
        {
            // Arrange
            var passwordDto = new ChangePasswordDto
            {
                UserId = 1,
                CurrentPassword = "OldPassword123",
                NewPassword = "short",
                ConfirmNewPassword = "short"
            };

            _mockUpdateProfileService
                .Setup(service => service.ChangePasswordAsync(passwordDto.UserId.Value, passwordDto))
                .ThrowsAsync(new ArgumentException("New password must meet complexity requirements"));

            // Act
            var result = await _controller.ChangePassword(passwordDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var jsonResult = JObject.FromObject(badRequestResult.Value ?? new { });
            Assert.Equal("New password must meet complexity requirements", jsonResult["message"]?.ToString());
        }

        [Fact]
        public async Task ChangePassword_ReturnsStatusCode500_WhenUnexpectedExceptionIsThrown()
        {
            // Arrange
            var passwordDto = new ChangePasswordDto
            {
                UserId = 1,
                CurrentPassword = "OldPassword123",
                NewPassword = "NewPassword123",
                ConfirmNewPassword = "NewPassword123"
            };

            _mockUpdateProfileService
                .Setup(service => service.ChangePasswordAsync(passwordDto.UserId.Value, passwordDto))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _controller.ChangePassword(passwordDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var jsonResult = JObject.FromObject(statusCodeResult.Value ?? new { });
            Assert.Equal("An error occurred while processing your request", jsonResult["message"]?.ToString());
        }
    }
}