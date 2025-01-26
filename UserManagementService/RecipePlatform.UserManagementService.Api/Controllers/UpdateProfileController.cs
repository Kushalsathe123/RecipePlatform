using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipePlatform.UserManagementService.Contracts.DTOs;
using RecipePlatform.UserManagementService.Contracts.Interfaces;

namespace RecipePlatform.UserManagementService.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UpdateProfileController : ControllerBase
    {
        private readonly IUpdateProfileService _updateProfileService;

        public UpdateProfileController(IUpdateProfileService updateProfileService)
        {
            _updateProfileService = updateProfileService ?? throw new ArgumentNullException(nameof(updateProfileService));
        }

        [Authorize]
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfileDetails([FromBody] UpdateUserProfileDto? updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data", errors = ModelState });
            }

            if (updateDto == null)
            {
                return BadRequest(new { message = "Request body cannot be empty" });
            }

            if (!updateDto.UserId.HasValue)
            {
                return BadRequest(new { message = "UserId is required." });
            }

            try
            {
                await _updateProfileService.UpdateUserProfileAsync(updateDto.UserId.Value, updateDto);
                return Ok(new { message = "Profile details updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request" });
            }
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto? passwordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid input data", errors = ModelState });
            }

            if (passwordDto == null)
            {
                return BadRequest(new { message = "Request body cannot be empty" });
            }

            if (!passwordDto.UserId.HasValue)
            {
                return BadRequest(new { message = "UserId is required." });
            }

            try
            {
                await _updateProfileService.ChangePasswordAsync(passwordDto.UserId.Value, passwordDto);
                return Ok(new { message = "Password changed successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request" });
            }
        }
    }
}
