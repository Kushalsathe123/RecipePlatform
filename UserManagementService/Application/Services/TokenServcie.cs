using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RecipePlatform.UserManagementService.Contracts.DTO;
using RecipePlatform.UserManagementService.Contracts.Interfaces;
using RecipePlatform.UserManagementService.Data.Entities;

namespace RecipePlatform.UserManagementService.Application.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public TokenDto GenerateToken(UserDto user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]
                ?? throw new InvalidOperationException("JWT secret is not configured"));

#pragma warning disable S6781
            var signingKey = new SymmetricSecurityKey(key);
#pragma warning restore S6781

            var expirationTime = DateTime.UtcNow.AddMinutes(
                int.TryParse(_configuration["Jwt:ExpirationInMinutes"], out int minutes) ? minutes : 60);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
            }),
                Expires = expirationTime,
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new TokenDto
            {
                UserId = user.UserId,
                ExpirationDate = expirationTime,
                AccessToken = tokenHandler.WriteToken(token),
                TokenType = "jwt-access-token"
            };
        }
    }
}