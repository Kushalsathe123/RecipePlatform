using System.Collections.Generic;

namespace RecipePlatform.NotificationDashboardService.Contracts.Interfaces
{
    public interface IEmailTemplateService
    {
        string GenerateEmailBody(string templateType, Dictionary<string, string>? templateData, string Name);
    }
}