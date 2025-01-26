using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using RecipePlatform.UserManagementService.Contracts.Interfaces;
using RecipePlatform.UserManagementService.Contracts.DTO;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using RecipePlatform.UserManagementService.Data.Repositories;
using RecipePlatform.UserManagementService.Contracts.DTOs;
using RecipePlatform.UserManagementService.Data.Entities;

namespace RecipePlatform.UserManagementService.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly INewUserRepository _newuserRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITokenRepository _tokenRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordService _passwordService;

        public AuthService(IUserRepository userRepository, ITokenRepository tokenRepository,ITokenService tokenService, IPasswordService passwordService, INewUserRepository newuserRepository)
        {
            _newuserRepository = newuserRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _tokenRepository = tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenRepository));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(tokenRepository));
        }

        public async Task RegisterUserAsync(RegisterUserDto userDto)
        {
            // Check if the email already exists
            var existingUser = await _newuserRepository.GetUserByEmailAsync(userDto.Email);
            if (existingUser != null)
                throw new ArgumentException("The email address is already registered.");

            // Generate salt and hash the password
            var salt = GenerateSalt();
            var passwordHash = HashPassword(userDto.Password, salt);

            // Create the user entity
            var user = new User
            {
                Name = userDto.Name,
                Email = userDto.Email,
                DietPreferences = userDto.DietPreferences ?? Array.Empty<string>(),
                FavoriteCuisines = userDto.FavoriteCuisines ?? Array.Empty<string>(),
                DateCreated = DateTime.UtcNow,
                PasswordHash = passwordHash,
                PasswordSalt = Convert.ToBase64String(salt)
            };

            // Add the user to the repository
            await _newuserRepository.AddUserAsync(user);

        }

        private static string HashPassword(string password, byte[] salt)
        {
            // Hash the password using PBKDF2
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

        public async Task<(string Name, TokenDto Token)> LoginAsync(LoginDto? loginDto)
        {
            UserDto? user = await _userRepository.GetUserByEmailAsync(loginDto?.Email).ConfigureAwait(false);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            if (!_passwordService.VerifyPassword(loginDto?.Password, user.PasswordHash, user.PasswordSalt))
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            var tokenDto = _tokenService.GenerateToken(user);

            await _tokenRepository.StoreUserTokenAsync(tokenDto).ConfigureAwait(false);

            return (user.Name,tokenDto);
        }

        public async Task<bool> LogoutAsync(string token)
        {
            return await _tokenRepository.InvalidateTokenAsync(token).ConfigureAwait(false);
        }
    }
}
