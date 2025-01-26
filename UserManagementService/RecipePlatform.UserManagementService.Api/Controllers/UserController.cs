using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RecipePlatform.UserManagementService.Application.Services;
using RecipePlatform.UserManagementService.Contracts.DTOs;
using RecipePlatform.UserManagementService.Contracts.Interfaces;
using RecipePlatform.UserManagementService.Data;
using RecipePlatform.UserManagementService.Data.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RecipePlatform.UserManagementService.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        
        private readonly IPasswordService _passwordService;
        private readonly IUserRepository _userRepository;
        
        private readonly IConfiguration _configuration;



        public UserController(ApplicationDbContext context, IPasswordService passwordService, IUserRepository userRepository, PasswordResetUrlGenerator passwordResetUrlGenerator, IConfiguration configuration)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }


        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var userResponse = new UserResponseDto
            {
                Name = user.Name,
                Email = user.Email,
                DietPreferences = user.DietPreferences,
                FavoriteCuisines = user.FavoriteCuisines
            };

            return Ok(userResponse);
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data", errors = ModelState });
            }

            try
            {
                
                // Call the PasswordService to handle the reset request and send the reset link via the notification service
                var result = await _passwordService.ForgotPasswordAsync(forgotPasswordDto.Email);

                if (result == "User not found.")
                {
                    return BadRequest(new { message = "User not found." });
                }

                if (result == null)
                {
                    return StatusCode(500, new { message = "An error occurred while processing your password reset request" });
                }

                return Ok(new { message = result });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request" });
            }
        }


        //[Authorize]
        [HttpPatch("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate and decode the token
            var tokenHandler = new JwtSecurityTokenHandler();
#pragma warning disable S6781
            var jwtSecret = _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret is not configured in the application settings.");
#pragma warning restore S6781
            var key = Encoding.ASCII.GetBytes(jwtSecret);

            try
            {
                var principal = tokenHandler.ValidateToken(resetPasswordDto.Token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                
                // Extract user ID from the token
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return BadRequest("Invalid token");
                }

                // Fetch the user
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Update the password and related fields
                var salt = GenerateSalt();
                if (string.IsNullOrEmpty(resetPasswordDto.NewPassword))
                {
                    return BadRequest("New password is required");
                }
                var passwordHash = HashPassword(resetPasswordDto.NewPassword, salt);
                user.PasswordSalt = Convert.ToBase64String(salt);
                user.PasswordHash = passwordHash;

                // Save the changes
                await _userRepository.UpdateUserAsync(user);

                return Ok("Password has been reset successfully");
            }
            catch (SecurityTokenException)
            {
                return BadRequest("Invalid token");
            }
            catch
            {
                return StatusCode(500, "An error occurred while resetting the password");
            }
        }



        public static string HashPassword(string password, byte[] salt)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            // Hash the password using PBKDF2
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);
            return Convert.ToBase64String(hash);
        }

        public static byte[] GenerateSalt()
        {
            // Generate a 16-byte salt
            byte[] salt = new byte[16];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        [Authorize]
        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data", errors = ModelState });
            }

            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            {
                return Unauthorized(new { message = "Invalid user ID" });
            }

            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }
                if (string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { message = "Password is required" });
                }
                if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
                {
                    return BadRequest(new { message = "Invalid password" });
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Account successfully deleted" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request" });
            }
        }
    }
}
