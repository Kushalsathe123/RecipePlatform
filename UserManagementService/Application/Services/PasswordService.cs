using Microsoft.EntityFrameworkCore;
using RecipePlatform.UserManagementService.Contracts.Interfaces;
using RecipePlatform.UserManagementService.Data;
using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using RecipePlatform.UserManagementService.Contracts.DTOs;

namespace RecipePlatform.UserManagementService.Application.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordResetUrlGenerator _urlGenerator;

        public PasswordService(ApplicationDbContext context, PasswordResetUrlGenerator urlGenerator)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _urlGenerator = urlGenerator ?? throw new ArgumentNullException(nameof(urlGenerator));
        }

        public bool VerifyPassword(string? password, string storedHash, string storedSalt)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password), "Password cannot be null.");
            }

            byte[] salt = Convert.FromBase64String(storedSalt);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);

            return Convert.ToBase64String(hash) == storedHash;
        }

        public async Task<string> ForgotPasswordAsync(string email)
        {

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return "User not found.";
            }

            // Use PasswordResetUrlGenerator to generate the reset URL
            var resetLink = _urlGenerator.GeneratePasswordResetUrl(user.UserId);

            // Create the payload for the notification service
            var payload = new
            {
                Name = user.Name,
                Email = email,
                subject = "Password Reset Request",
                templateType = "PasswordReset",
                templateData = new Dictionary<string, string>
                {
                    { "ResetLink", resetLink }
                }
            };


            // Serialize the payload into JSON format
            var jsonPayload = JsonConvert.SerializeObject(payload);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // Call the external notification service to send the email
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(
                    "https://centralizednotificationdashboardservice-a4ghfscvfdfvhsh5.westeurope-01.azurewebsites.net/api/v1/notifications/send",
                    httpContent
                );

                if (!response.IsSuccessStatusCode)
                {
                    return "Failed to send password reset link.";
                }
            }

            return "Password reset link sent to your email.";
        }
        public async Task<string> ResetPasswordAsync(ResetPasswordDto request)
        {
            return await Task.FromResult("Password successfully reset.");
        }
    }
}