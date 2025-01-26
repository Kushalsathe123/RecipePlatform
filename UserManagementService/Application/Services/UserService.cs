using RecipePlatform.UserManagementService.Contracts.DTOs;
using RecipePlatform.UserManagementService.Contracts.Interfaces;
using RecipePlatform.UserManagementService.Data.Entities;
using RecipePlatform.UserManagementService.Data.Repositories;
using System.Security.Cryptography;

namespace RecipePlatform.UserManagementService.Application.Services
{
    public class UserService
    {
        private readonly IDeleteUserRepository _userRepository;

        public UserService(IDeleteUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<bool> DeleteUserAccountAsync(int userId, string password)
        {
            // Retrieve the user from the repository
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
                throw new UserNotFoundException(userId);

            // Verify the password
            if (!VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
                throw new UnauthorizedAccessException("Incorrect password.");

            // Proceed to delete the user if the password is verified
            return await _userRepository.DeleteUserAsync(userId);
        }


        private static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            // Convert the stored salt to byte array and hash the password
            byte[] salt = Convert.FromBase64String(storedSalt);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);

            return Convert.ToBase64String(hash) == storedHash;
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
