using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RecipePlatform.UserManagementService.Api.Controllers;
using RecipePlatform.UserManagementService.Contracts.DTO;
using RecipePlatform.UserManagementService.Contracts.Interfaces;
using Xunit;
using System.Net;
using Newtonsoft.Json.Linq;

namespace RecipePlatform.UserManagementService.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _controller = new AuthController(_mockAuthService.Object);
        }

        [Theory]
        [InlineData("test@example.com", "password", "Test User")]
        public async Task Login_ValidCredentials_ReturnsOkResult(string email, string password, string expectedName)
        {
            // Arrange
            var loginDto = new LoginDto { Email = email, Password = password };
            var tokenDto = new TokenDto
            {
                UserId = 1,
                AccessToken = "valid_token",
                ExpirationDate = DateTime.UtcNow.AddHours(1),
                TokenType = "Bearer"
            };
            _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync((expectedName, tokenDto));

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var jsonResult = JObject.FromObject(okResult.Value ?? new { });
            Assert.Equal($"Welcome back, {expectedName}!", jsonResult["message"]?.ToString());
            Assert.Equal(JObject.FromObject(tokenDto).ToString(), jsonResult["token"]?.ToString());
        }

        [Theory]
        [InlineData("test@example.com", "wrongpassword", "Invalid credentials")]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized(string email, string password, string expectedMessage)
        {
            // Arrange
            var loginDto = new LoginDto { Email = email, Password = password };
            _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
                .ThrowsAsync(new UnauthorizedAccessException());

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var jsonResult = JObject.FromObject(unauthorizedResult.Value ?? new { });
            Assert.Equal(expectedMessage, jsonResult["message"]?.ToString());
        }

        [Theory]
        [InlineData("valid_token", "You have been successfully logged out. Thank you for using our service!")]
        public async Task Logout_ValidToken_ReturnsOkResult(string token, string expectedMessage)
        {
            // Arrange
            _mockAuthService.Setup(x => x.LogoutAsync(token)).ReturnsAsync(true);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.HttpContext.Request.Headers.Authorization = $"Bearer {token}";

            // Act
            var result = await _controller.Logout();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var jsonResult = JObject.FromObject(okResult.Value ?? new { });
            Assert.Equal(expectedMessage, jsonResult["message"]?.ToString());
        }

        [Theory]
        [InlineData("InvalidHeader", "Invalid or missing Authorization header")]
        public async Task Logout_InvalidAuthorizationHeader_ReturnsUnauthorized(string header, string expectedMessage)
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.HttpContext.Request.Headers.Authorization = header;

            // Act
            var result = await _controller.Logout();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var jsonResult = JObject.FromObject(unauthorizedResult.Value ?? new { });
            Assert.Equal(expectedMessage, jsonResult["message"]?.ToString());
        }

        [Theory]
        [InlineData("valid_token", "Logout failed: token could not be invalidated")]
        public async Task Logout_LogoutFailed_ReturnsBadRequest(string token, string expectedMessage)
        {
            // Arrange
            _mockAuthService.Setup(x => x.LogoutAsync(token)).ReturnsAsync(false);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.HttpContext.Request.Headers.Authorization = $"Bearer {token}";

            // Act
            var result = await _controller.Logout();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var jsonResult = JObject.FromObject(badRequestResult.Value ?? new { });
            Assert.Equal(expectedMessage, jsonResult["message"]?.ToString());
        }

        [Fact]
        public async Task Login_NullLoginDto_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Login(null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid input data", badRequestResult.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task Login_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Email", "Email is required");

            // Act
            var result = await _controller.Login(new LoginDto { Email = null!, Password = null! });

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid input data", badRequestResult.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task Login_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "nonexistent@example.com", Password = "password" };
            _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("User not found", notFoundResult.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task Login_UnexpectedError_ReturnsInternalServerError()
        {
            // Arrange
            var loginDto = new LoginDto { Email = "test@example.com", Password = "password" };
            _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<LoginDto>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
            Assert.Contains("An unexpected error occurred", statusCodeResult.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task Logout_EmptyToken_ReturnsUnauthorized()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.HttpContext.Request.Headers.Authorization = "Bearer ";

            // Act
            var result = await _controller.Logout();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Contains("Authorization token is required", unauthorizedResult.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task Logout_UnexpectedError_ReturnsInternalServerError()
        {
            // Arrange
            _mockAuthService.Setup(x => x.LogoutAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Unexpected error"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            _controller.HttpContext.Request.Headers.Authorization = "Bearer valid_token";

            // Act
            var result = await _controller.Logout();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.InternalServerError, statusCodeResult.StatusCode);
            Assert.Contains("An unexpected error occurred", statusCodeResult.Value?.ToString() ?? string.Empty);
        }
    }
}