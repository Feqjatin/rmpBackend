using System.Net;
using System.Net.Mail;
using rmpBackend.Models;

namespace rmpBackend.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly IEmailTemplateProvider _templateProvider;

        public EmailService(
            IConfiguration config,
            IEmailTemplateProvider templateProvider)
        {
            _config = config;
            _templateProvider = templateProvider;
        }

        public async Task SendAsync(EmailRequest request)
        {
            var (subject, body) =
                _templateProvider.GetTemplate(request.EventType, request.Data);

            var mail = new MailMessage
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            foreach (var email in request.ToEmails)
            {
                mail.To.Add(email);
            }

            mail.From = new MailAddress("prajapatijatin233@gmail.com", "HR Team");

            using var smtp = new SmtpClient(
                _config["Email:Host"],
                int.Parse(_config["Email:Port"]))
            {
                Credentials = new NetworkCredential(
                    _config["Email:Username"],
                    _config["Email:Password"]),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
        }
    }

}
