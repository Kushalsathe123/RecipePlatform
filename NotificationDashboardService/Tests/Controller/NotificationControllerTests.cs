using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RecipePlatform.NotificationDashboardService.Api.Controllers;
using RecipePlatform.NotificationDashboardService.Contracts.DTOs;
using RecipePlatform.NotificationDashboardService.Contracts.Interfaces;
using Xunit;

namespace RecipePlatform.NotificationDashboardService.Tests.Controllers
{
    public class NotificationsControllerTests
    {
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly NotificationsController _controller;

        public NotificationsControllerTests()
        {
            _mockEmailService = new Mock<IEmailService>();
            _controller = new NotificationsController(_mockEmailService.Object);
        }

        [Fact]
        public async Task SendNotification_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var notificationData = new NotificationRequestDto
            {
                Name = "John Doe",
                Email = "test@example.com",
                Subject = "Test Subject",
                TemplateType = "PasswordReset",
                TemplateData = new Dictionary<string, string>
                {
                    { "ResetLink", "http://example.com/reset" }
                }
            };

            _mockEmailService.Setup(x => x.PrepareAndSendEmailAsync(It.IsAny<NotificationRequestDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.SendNotification(notificationData);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<Dictionary<string, string>>(okResult.Value);
            Assert.Equal("Notification sent successfully", returnValue["message"]);
            _mockEmailService.Verify(x => x.PrepareAndSendEmailAsync(It.IsAny<NotificationRequestDto>()), Times.Once);
        }

        [Fact]
        public async Task SendNotification_EmailServiceThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var notificationData = new NotificationRequestDto
            {
                Name = "John Doe",
                Email = "test@example.com",
                Subject = "Test Subject",
                TemplateType = "PasswordReset",
                TemplateData = new Dictionary<string, string>
                {
                    { "ResetLink", "http://example.com/reset" }
                }
            };

            _mockEmailService.Setup(x => x.PrepareAndSendEmailAsync(It.IsAny<NotificationRequestDto>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.SendNotification(notificationData);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var returnValue = Assert.IsType<Dictionary<string, string>>(statusCodeResult.Value);
            Assert.Contains("An error occurred while sending the notification", returnValue["message"]);
        }

        [Fact]
        public async Task SendNotification_UnsupportedTemplateType_ReturnsBadRequest()
        {
            // Arrange
            var notificationData = new NotificationRequestDto
            {
                Name = "John Doe",
                Email = "test@example.com",
                Subject = "Test Subject",
                TemplateType = "UnsupportedTemplate",
                TemplateData = new Dictionary<string, string>()
            };

            _mockEmailService.Setup(x => x.PrepareAndSendEmailAsync(It.IsAny<NotificationRequestDto>()))
                .ThrowsAsync(new NotImplementedException($"Template type '{notificationData.TemplateType}' is not supported."));

            // Act
            var result = await _controller.SendNotification(notificationData);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnValue = Assert.IsType<Dictionary<string, string>>(badRequestResult.Value);
            Assert.Contains($"Unsupported template type: Template type '{notificationData.TemplateType}' is not supported.", returnValue["message"]);
        }

        [Fact]
        public async Task SendNotification_NullRequest_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.SendNotification(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnValue = Assert.IsType<Dictionary<string, string>>(badRequestResult.Value);
            Assert.Equal("A non-null request body is required.", returnValue["message"]);
        }

        [Theory]
        [InlineData("PasswordReset")]
        [InlineData("AccountCreation")]
        [InlineData("AccountDeletion")]
        public async Task SendNotification_ValidTemplateTypes_ReturnsOkResult(string templateType)
        {
            // Arrange
            var notificationData = new NotificationRequestDto
            {
                Name = "John Doe",
                Email = "test@example.com",
                Subject = "Test Subject",
                TemplateType = templateType,
                TemplateData = new Dictionary<string, string>
                {
                    { "ResetLink", "http://example.com/reset" }
                }
            };

            _mockEmailService.Setup(x => x.PrepareAndSendEmailAsync(It.IsAny<NotificationRequestDto>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.SendNotification(notificationData);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
    }
}