using System;
using System.Collections.Generic;
using RecipePlatform.NotificationDashboardService.Application.Services;
using RecipePlatform.NotificationDashboardService.Contracts.DTOs;
using RecipePlatform.NotificationDashboardService.Contracts.Interfaces;
using Xunit;

namespace RecipePlatform.NotificationDashboardService.Tests.Services
{
    public class EmailTemplateServiceTests
    {
        private readonly EmailTemplateService _emailTemplateService;

        public EmailTemplateServiceTests()
        {
            _emailTemplateService = new EmailTemplateService();
        }

        [Fact]
        public void GenerateEmailBody_PasswordResetTemplate_ReturnsValidHtml()
        {
            // Arrange
            var notificationRequest = new NotificationRequestDto
            {
                Name = "John Doe",
                Email = "john@example.com",
                Subject = "Password Reset",
                TemplateType = "PasswordReset",
                TemplateData = new Dictionary<string, string>
                {
                    { "ResetLink", "http://example.com/reset" }
                }
            };

            // Act
            var result = _emailTemplateService.GenerateEmailBody(notificationRequest.TemplateType, notificationRequest.TemplateData, notificationRequest.Name);

            // Assert
            Assert.Contains("<html>", result);
            Assert.Contains("<body", result);
            Assert.Contains("RecipeShare - Reset Your Password", result);
            Assert.Contains("Dear John Doe,", result);
            Assert.Contains("http://example.com/reset", result);
            Assert.Contains("</body>", result);
            Assert.Contains("</html>", result);
        }

        [Fact]
        public void GenerateEmailBody_AccountCreationTemplate_ReturnsValidHtml()
        {
            // Arrange
            var notificationRequest = new NotificationRequestDto
            {
                Name = "Jane Smith",
                Email = "jane@example.com",
                Subject = "Welcome to RecipeShare",
                TemplateType = "AccountCreation",
                TemplateData = new Dictionary<string, string>()
            };

            // Act
            var result = _emailTemplateService.GenerateEmailBody(notificationRequest.TemplateType, notificationRequest.TemplateData, notificationRequest.Name);

            // Assert
            Assert.Contains("<html>", result);
            Assert.Contains("<body", result);
            Assert.Contains("Welcome to RecipeShare", result);
            Assert.Contains("Dear Jane Smith,", result);
            Assert.Contains("Share your favorite recipes", result);
            Assert.Contains("</body>", result);
            Assert.Contains("</html>", result);
        }

        [Fact]
        public void GenerateEmailBody_AccountDeletionTemplate_ReturnsValidHtml()
        {
            // Arrange
            var notificationRequest = new NotificationRequestDto
            {
                Name = "Alice Johnson",
                Email = "alice@example.com",
                Subject = "Account Deletion Confirmation",
                TemplateType = "AccountDeletion",
                TemplateData = new Dictionary<string, string>()
            };

            // Act
            var result = _emailTemplateService.GenerateEmailBody(notificationRequest.TemplateType, notificationRequest.TemplateData, notificationRequest.Name);

            // Assert
            Assert.Contains("<html>", result);
            Assert.Contains("<body", result);
            Assert.Contains("RecipeShare - Your Account Has Been Deleted", result);
            Assert.Contains("Dear Alice Johnson,", result);
            Assert.Contains("Your personal information has been removed", result);
            Assert.Contains("</body>", result);
            Assert.Contains("</html>", result);
        }

        [Fact]
        public void GenerateEmailBody_UnsupportedTemplateType_ThrowsNotImplementedException()
        {
            // Arrange
            var notificationRequest = new NotificationRequestDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Subject = "Test",
                TemplateType = "UnsupportedTemplate",
                TemplateData = new Dictionary<string, string>()
            };

            // Act & Assert
            var exception = Assert.Throws<NotImplementedException>(() =>
                _emailTemplateService.GenerateEmailBody(notificationRequest.TemplateType, notificationRequest.TemplateData, notificationRequest.Name));
            Assert.Contains("Template type 'UnsupportedTemplate' is not supported", exception.Message);
        }

        [Fact]
        public void GenerateEmailBody_PasswordResetTemplateWithoutResetLink_ThrowsArgumentException()
        {
            // Arrange
            var notificationRequest = new NotificationRequestDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Subject = "Password Reset",
                TemplateType = "PasswordReset",
                TemplateData = new Dictionary<string, string>()
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                _emailTemplateService.GenerateEmailBody(notificationRequest.TemplateType, notificationRequest.TemplateData, notificationRequest.Name));
            Assert.Contains("Reset link is required for password reset template", exception.Message);
        }
    }
}