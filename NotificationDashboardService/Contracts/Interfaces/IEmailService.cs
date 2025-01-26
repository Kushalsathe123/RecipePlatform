using RecipePlatform.NotificationDashboardService.Contracts.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.NotificationDashboardService.Contracts.Interfaces
{
    public interface IEmailService
    {
        Task PrepareAndSendEmailAsync(NotificationRequestDto notificationData);
    }
}
