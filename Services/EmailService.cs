using Microsoft.Extensions.Options;
using P4_Backend_Car_App.Interfaces;
using System.Net;
using System.Net.Mail;

namespace P4_Backend_Car_App.Services
{
    public class EmailService : IMailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger;

            if (string.IsNullOrWhiteSpace(_settings.SmtpHost))
                throw new ArgumentNullException(nameof(_settings.SmtpHost), "SMTP host is required");

            if (string.IsNullOrWhiteSpace(_settings.SmtpUsername))
                throw new ArgumentNullException(nameof(_settings.SmtpUsername), "SMTP username is required");

            if (string.IsNullOrWhiteSpace(_settings.SmtpPassword))
                throw new ArgumentNullException(nameof(_settings.SmtpPassword), "SMTP password is required");
        }

        public async Task SendEmailAsync(string emailAddress, string subject, string body, bool isHtml = false)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
                throw new ArgumentNullException(nameof(emailAddress));

            try
            {
                _logger.LogInformation("Sending email to {Email}", emailAddress);

                using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword),
                    EnableSsl = true,

                    Timeout = 10000
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_settings.SmtpUsername, "Car App Service"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml,
                };

                mailMessage.To.Add(emailAddress);

                await client.SendMailAsync(mailMessage);

                _logger.LogInformation("Email successfully sent to {Email}", emailAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", emailAddress);
                throw new MailServiceException($"Failed to send email to {emailAddress}", ex);
            }
        }

        public class MailServiceException : Exception
        {
            public MailServiceException(string message, Exception innerException)
                : base(message, innerException)
            {
            }
        }
    }
}
