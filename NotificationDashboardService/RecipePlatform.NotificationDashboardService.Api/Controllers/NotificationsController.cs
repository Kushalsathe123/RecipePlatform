using Microsoft.AspNetCore.Mvc;
using RecipePlatform.NotificationDashboardService.Contracts.DTOs;
using RecipePlatform.NotificationDashboardService.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecipePlatform.NotificationDashboardService.Api.Controllers
{
    [ApiController]
    [Route("api/v1/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly IEmailService _emailService;
        

        public NotificationsController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationRequestDto? notificationData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (notificationData is null)
            {
                return BadRequest(new Dictionary<string, string> { { "message", $"A non-null request body is required." } });
            }

            try
            {
                await _emailService.PrepareAndSendEmailAsync(notificationData);
                return Ok(new Dictionary<string, string> { { "message", "Notification sent successfully" } });
            }
            catch (NotImplementedException ex)
            {
                // Log the exception
                return BadRequest(new Dictionary<string, string> { { "message", $"Unsupported template type: {ex.Message}" } });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new Dictionary<string, string> { { "message", $"An error occurred while sending the notification: {ex.Message}" } });
            }
        }
    }
}
