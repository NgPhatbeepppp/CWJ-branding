using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Cw.Branding.Web.Models.Settings;
using Cw.Branding.Web.Services.Interfaces;

namespace Cw.Branding.Web.Services.Implementations
{
  
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(string subject, string body)
        {
            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.SenderEmail, _settings.AppPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true 
            };

            mailMessage.To.Add(_settings.TargetEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}