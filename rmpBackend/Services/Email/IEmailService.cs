using rmpBackend.Models.DTOs;

namespace rmpBackend.Services.Email
{
    public interface IEmailService
    {
        Task SendAsync(EmailRequest request);
    }
}
 
