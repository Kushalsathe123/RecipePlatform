using System.Net.Mail;
using System.Net;
using RecipePlatform.NotificationDashboardService.Contracts.Interfaces;
using RecipePlatform.NotificationDashboardService.Contracts.DTOs;
using Microsoft.Extensions.Options;

namespace RecipePlatform.NotificationDashboardService.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettingsDto _smtpSettings;
        private readonly IEmailTemplateService _emailTemplateService;

        public EmailService(IOptions<SmtpSettingsDto> smtpSettings, IEmailTemplateService emailTemplateService)
        {
            _smtpSettings = smtpSettings.Value;
            _emailTemplateService = emailTemplateService;
        }

        public async Task PrepareAndSendEmailAsync(NotificationRequestDto notificationData)
        {
            string body = _emailTemplateService.GenerateEmailBody(notificationData.TemplateType, notificationData.TemplateData, notificationData.Name);
            ValidateEmailParameters(notificationData.Email, notificationData.Subject, body);
            await SendEmailInternalAsync(notificationData.Email!, notificationData.Subject!, body!);
        }

        private static void ValidateEmailParameters(string? to, string? subject, string? body)
        {
            if (string.IsNullOrEmpty(to))
                throw new ArgumentException("'to' cannot be null or empty", nameof(to));

            ArgumentNullException.ThrowIfNull(subject);
            ArgumentNullException.ThrowIfNull(body);
        }

        private async Task SendEmailInternalAsync(string to, string subject, string body)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.Username, "Recipe Platform"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);

            using var smtpClient = new SmtpClient(_smtpSettings.Server, _smtpSettings.Port)
            {
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}

