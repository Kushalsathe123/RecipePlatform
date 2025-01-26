using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RecipePlatform.UserManagementService.Contracts.DTO;
using RecipePlatform.UserManagementService.Contracts.Interfaces;
using RecipePlatform.UserManagementService.Data.Entities;
using RecipePlatform.UserManagementService.Data.Repositories;

namespace RecipePlatform.UserManagementService.Application.Services
{
    public class PasswordResetUrlGenerator
    {
        private readonly IConfiguration _configuration;
        private readonly ITokenRepository _tokenRepository;
        private readonly string _baseUrl;

        public PasswordResetUrlGenerator(IConfiguration configuration, ITokenRepository tokenRepository)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _tokenRepository = tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));
            _baseUrl = @"https://recipeaugust10-angularui-cmahaaddc0e7hqe9.westeurope-01.azurewebsites.net/create-new-password";
        }

        public string GeneratePasswordResetUrl(int userId, int expirationMinutes = 60)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]
                ?? throw new InvalidOperationException("JWT secret is not configured"));

            var expirationTime = DateTime.UtcNow.AddMinutes(
                    int.TryParse(_configuration["Jwt:ExpirationInMinutes"], out int minutes) ? minutes : 60);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }),
                Expires = expirationTime,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenToStore = new TokenDto
            {
                UserId = userId,
                ExpirationDate = expirationTime,
                AccessToken = tokenHandler.WriteToken(token),
                TokenType = "reset-password" // Optional: specify token type here
            };

            _tokenRepository.StoreUserTokenAsync(tokenToStore).ConfigureAwait(false);

            var tokenString = tokenHandler.WriteToken(token);

            var urlEncodedToken = Uri.EscapeDataString(tokenString);
            return $"{_baseUrl}?token={urlEncodedToken}";
        }
    }
}