using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.NotificationDashboardService.Contracts.DTOs
{
    public class SmtpSettingsDto
    {
        public required string Server { get; set; }
        public int Port { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
