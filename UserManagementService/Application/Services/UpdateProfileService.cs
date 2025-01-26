using RecipePlatform.UserManagementService.Contracts.DTOs;
using RecipePlatform.UserManagementService.Contracts.Interfaces;
using RecipePlatform.UserManagementService.Data.Repositories;
using System.Runtime.Serialization;
using System.Security.Cryptography;

namespace RecipePlatform.UserManagementService.Application.Services
{
    public class UpdateProfileService : IUpdateProfileService
    {
        private readonly IUpdateUserProfileRepository _userProfileRepository;
        private readonly IPasswordService _passwordService;

        public UpdateProfileService(IUpdateUserProfileRepository userProfileRepository, IPasswordService passwordService)
        {
            _userProfileRepository = userProfileRepository;
            _passwordService = passwordService;
        }


        private static void ValidateUpdateUserProfileParameters(int userId, UpdateUserProfileDto updateDto)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID.", nameof(userId));

            if (updateDto == null)
                throw new ArgumentException("User profile data cannot be null", nameof(updateDto));

            if (string.IsNullOrWhiteSpace(updateDto.Name))
                throw new ArgumentException("User name is required.", nameof(updateDto));
        }

        public async Task UpdateUserProfileAsync(int userId, UpdateUserProfileDto updateDto)
        {
            ValidateUpdateUserProfileParameters(userId, updateDto);

            var existingUser = await _userProfileRepository.GetUserByIdAsync(userId);
            if (existingUser == null)
                throw new UserNotFoundException(userId);

            existingUser.Name = updateDto.Name ?? existingUser.Name;
            existingUser.DietPreferences = updateDto.DietPreferences ?? existingUser.DietPreferences;
            existingUser.FavoriteCuisines = updateDto.FavoriteCuisines ?? existingUser.FavoriteCuisines;

            await _userProfileRepository.UpdateUserAsync(existingUser);
        }

        public static void ValidateChangePasswordParameters(ChangePasswordDto? passwordDto)
        {
            if (passwordDto == null)
                throw new ArgumentException("Password change data cannot be null.", nameof(passwordDto));

            if (string.IsNullOrEmpty(passwordDto.CurrentPassword))
                throw new UnauthorizedAccessException("Current password is required to change your password.");

            if (passwordDto.NewPassword != passwordDto.ConfirmNewPassword)
                throw new ArgumentException("New password and confirm password do not match.");
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordDto? passwordDto)
        {
            // Validate parameters
            ValidateChangePasswordParameters(passwordDto);

            // Fetch existing user
            var existingUser = await _userProfileRepository.GetUserByIdAsync(userId);
            if (existingUser == null)
                throw new UserNotFoundException(userId);

            // Verify current password
            var isCurrentPasswordValid = _passwordService.VerifyPassword(
                passwordDto!.CurrentPassword,
                existingUser.PasswordHash,
                existingUser.PasswordSalt
            );

            if (!isCurrentPasswordValid)
                throw new UnauthorizedAccessException("The current password is incorrect.");

            // Update password hash and salt
            var salt = GenerateSalt();
            var newHash = HashPassword(passwordDto.NewPassword, salt);
            existingUser.PasswordHash = newHash;
            existingUser.PasswordSalt = Convert.ToBase64String(salt);

            // Persist changes
            await _userProfileRepository.UpdateUserAsync(existingUser);
        }

        private static string HashPassword(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);
            return Convert.ToBase64String(hash);
        }

        private static byte[] GenerateSalt()
        {
            // Generate a 16-byte salt using RandomNumberGenerator
            byte[] salt = new byte[16];
            RandomNumberGenerator.Fill(salt);
            return salt;

        }
        [Serializable]
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
        public class UserNotFoundException : Exception
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
        {
            public int UserId { get; }

            // Default constructor
            public UserNotFoundException() : base("User not found.") { }

            // Constructor with a custom message
            public UserNotFoundException(string message) : base(message) { }

            // Constructor with a custom message and inner exception
            public UserNotFoundException(string message, Exception innerException)
                : base(message, innerException) { }

            // Constructor with user ID
            public UserNotFoundException(int userId)
                : base($"User with ID {userId} was not found.")
            {
                UserId = userId;
            }

        }
    }
}