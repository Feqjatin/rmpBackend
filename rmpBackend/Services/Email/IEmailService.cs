using rmpBackend.Models;

namespace rmpBackend.Services.Email
{
    public interface IEmailService
    {
        Task SendAsync(EmailRequest request);
    }
}
 
