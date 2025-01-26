using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.NotificationDashboardService.Contracts.DTOs
{
    public class NotificationRequestDto
    {
        [Required]
        public required string Name { get; set; }
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Subject { get; set; }

        [Required]
        public required string TemplateType { get; set; }

        public Dictionary<string, string>? TemplateData { get; set; }
    }
}
