using System;
using System.Collections.Generic;
using System.Xml.Linq;
using RecipePlatform.NotificationDashboardService.Contracts.Interfaces;

namespace RecipePlatform.NotificationDashboardService.Application.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        public string GenerateEmailBody(string templateType, Dictionary<string, string>? templateData, string Name)
        {
            return templateType switch
            {
                "PasswordReset" => GeneratePasswordResetTemplate(templateData,Name),
                "AccountCreation" => GenerateAccountCreationTemplate(Name),
                "AccountDeletion" => GenerateAccountDeletionTemplate(Name),
                _ => throw new NotImplementedException($"Template type '{templateType}' is not supported.")
            };
        }

        private static string GeneratePasswordResetTemplate(Dictionary<string, string>? templateData, string Name)
        {
            if ( templateData == null)
            {
                throw new ArgumentException("Template Data is required for password reset template");
            }
            if (!templateData.TryGetValue("ResetLink", out var resetLink))
            {
                throw new ArgumentException("Reset link is required for password reset template.");
            }
            if (Name is null)
            {
                throw new ArgumentException("Name is required for password reset template.");
            }

            return $@"
<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <h2>RecipeShare - Reset Your Password</h2>
    <p>Dear {Name},</p>
    <p>We received a request to reset the password for your RecipeShare account. If you didn't make this request, please ignore this email.</p>
    <p>To reset your password, click the button below:</p>
    <p style='text-align: center;'>
        <a href='{resetLink}' style='background-color: #4CAF50; border: none; color: white; padding: 15px 32px; text-align: center; text-decoration: none; display: inline-block; font-size: 16px; margin: 4px 2px; cursor: pointer;'>Reset Password</a>
    </p>
    <p>If the button above doesn't work, you can also copy and paste the following link into your browser:</p>
    <p>{resetLink}</p>
    <p>This link will expire in 1 hour for security reasons.</p>
    <p>If you're having trouble, please contact our support team at support@recipeshare.com.</p>
    <br>
    <p>Best regards,</p>
    <p>The RecipeShare Team</p>
    <hr>
    <p style='font-size: 12px; color: #777;'>This is an automated message, please do not reply to this email.</p>
</body>
</html>";
        }

        private static string GenerateAccountCreationTemplate( string Name)
        {
            if (Name is null)
            {
                throw new ArgumentException("Name is required for password reset template.");
            }

            return $@"
<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <h2>Welcome to RecipeShare</h2>
    <p>Dear {Name},</p>
    <p>Welcome to RecipeShare! We're excited to have you join our community of food enthusiasts.</p>
    <p>Now you can:</p>
    <ul>
        <li>Share your favorite recipes</li>
        <li>Discover new dishes from around the world</li>
        <li>Connect with other food lovers</li>
    </ul>
    <p>If you have any questions, please don't hesitate to contact our support team at support@recipeshare.com.</p>
    <p>Happy cooking!</p>
    <br>
    <p>The RecipeShare Team</p>
    <hr>
    <p style='font-size: 12px; color: #777;'>This is an automated message, please do not reply to this email.</p>
</body>
</html>";
        }

        private static string GenerateAccountDeletionTemplate(string Name)
        {
            if (Name is null)
            {
                throw new ArgumentException("Name is required for password reset template.");
            }

            return $@"
<html>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
    <h2>RecipeShare - Your Account Has Been Deleted</h2>
    <p>Dear {Name},</p>
    <p>We're sorry to see you go. Your RecipeShare account has been successfully deleted as per your request.</p>
    <p>Please note:</p>
    <ul>
        <li>Your personal information has been removed from our active database</li>
        <li>Any recipes you've shared will be anonymized</li>
    </ul>
    <p>If you have any questions or feedback, please contact us at support@recipeshare.com.</p>
    <p>We hope to see you again in the future!</p>
    <br>
    <p>Best wishes,</p>
    <p>The RecipeShare Team</p>
    <hr>
    <p style='font-size: 12px; color: #777;'>This is an automated message, please do not reply to this email.</p>
</body>
</html>";
        }
    }
}