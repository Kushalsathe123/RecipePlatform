using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipePlatform.UserManagementService.Contracts.DTO;
using RecipePlatform.UserManagementService.Contracts.DTOs;
using RecipePlatform.UserManagementService.Contracts.Interfaces;
using System.Net;

namespace RecipePlatform.UserManagementService.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[Controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data", errors = ModelState });
            }

            try
            {
                await _authService.RegisterUserAsync(registerDto);
                return StatusCode(201, new { message = "User registered successfully" });
            }
            catch (ArgumentException ex) when (ex.Message.Contains("already registered"))
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request" });
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto? loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            if (loginDto == null)
            {
                return BadRequest(new { message = "Invalid input data", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
            }

            try
            {
                var (userName, token) = await _authService.LoginAsync(loginDto);
                return Ok(new { message = $"Welcome back, {userName}!", token });
            }
            catch (KeyNotFoundException)
            {
                // 404 Not Found for user not existing
                return NotFound(new { message = "User not found" });
            }
            catch (UnauthorizedAccessException)
            {
                // 401 Unauthorized for invalid credentials
                return Unauthorized(new { message = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                // Log the exception here
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
#pragma warning disable S6932
            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
#pragma warning restore S6932
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { message = "Invalid or missing Authorization header" });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "Authorization token is required" });
            }

            try
            {
                var result = await _authService.LogoutAsync(token);
                if (result)
                {
                    return Ok(new { message = "You have been successfully logged out. Thank you for using our service!" });
                }
                else
                {
                    return BadRequest(new { message = "Logout failed: token could not be invalidated" });
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception here
                return StatusCode((int)HttpStatusCode.InternalServerError, new { message = "An unexpected error occurred", error = ex.Message });
            }
        }
    }
}