using rmpBackend.Models.DTOs;

namespace rmpBackend.Services.Email
{
    public interface IEmailTemplateProvider
    {
        (string Subject, string Body) GetTemplate(
            EmailEventType eventType,
            Dictionary<string, string> data);
    }
}


