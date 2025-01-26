using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using RecipePlatform.NotificationDashboardService.Application.Services;
using RecipePlatform.NotificationDashboardService.Contracts.DTOs;
using RecipePlatform.NotificationDashboardService.Contracts.Interfaces;
using Xunit;

namespace RecipePlatform.NotificationDashboardService.Tests.Services
{
    public class EmailServiceTests
    {
        private readonly Mock<IOptions<SmtpSettingsDto>> _mockSmtpSettings;
        private readonly Mock<IEmailTemplateService> _mockEmailTemplateService;
        private readonly EmailService _emailService;

        public EmailServiceTests()
        {
            _mockSmtpSettings = new Mock<IOptions<SmtpSettingsDto>>();
            _mockSmtpSettings.Setup(x => x.Value).Returns(new SmtpSettingsDto
            {
                Server = "smtp.example.com",
                Port = 587,
                Username = "test@example.com",
                Password = "password123"
            });

            _mockEmailTemplateService = new Mock<IEmailTemplateService>();
            _emailService = new EmailService(_mockSmtpSettings.Object, _mockEmailTemplateService.Object);
        }

        [Fact]
        public async Task PrepareAndSendEmailAsync_ValidRequest_SendsEmail()
        {
            // Arrange
            var notificationData = new NotificationRequestDto
            {
                Name = "John Doe",
                Email = "recipient@example.com",
                Subject = "Test Subject",
                TemplateType = "PasswordReset",
                TemplateData = new Dictionary<string, string> { { "ResetLink", "http://example.com/reset" } }
            };

            _mockEmailTemplateService.Setup(x => x.GenerateEmailBody(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>()))
                .Returns("Test email body");

            // Act & Assert
            await Assert.ThrowsAsync<SmtpException>(() => _emailService.PrepareAndSendEmailAsync(notificationData));

            _mockEmailTemplateService.Verify(x => x.GenerateEmailBody(
                notificationData.TemplateType, notificationData.TemplateData, notificationData.Name), Times.Once);
        }

        [Fact]
        public async Task PrepareAndSendEmailAsync_InvalidEmail_ThrowsArgumentException()
        {
            // Arrange
            var notificationData = new NotificationRequestDto
            {
                Name = "John Doe",
                Email = "",  // Invalid email
                Subject = "Test Subject",
                TemplateType = "PasswordReset",
                TemplateData = new Dictionary<string, string>()
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _emailService.PrepareAndSendEmailAsync(notificationData));
        }

        [Fact]
        public async Task PrepareAndSendEmailAsync_TemplateServiceThrowsException_PropagatesException()
        {
            // Arrange
            var notificationData = new NotificationRequestDto
            {
                Name = "John Doe",
                Email = "recipient@example.com",
                Subject = "Test Subject",
                TemplateType = "InvalidTemplate",
                TemplateData = new Dictionary<string, string>()
            };

            _mockEmailTemplateService.Setup(x => x.GenerateEmailBody(
                It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>()))
                .Throws(new NotImplementedException("Invalid template type"));

            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => _emailService.PrepareAndSendEmailAsync(notificationData));
        }
    }
}