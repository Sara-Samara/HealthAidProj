using HealthAidAPI.Helpers;
using HealthAidAPI.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace HealthAidAPI.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var smtp = new SmtpClient(_settings.SmtpServer)
                {
                    Port = _settings.Port,
                    Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                    EnableSsl = true
                };

                var mail = new MailMessage()
                {
                    From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mail.To.Add(to);

                await smtp.SendMailAsync(mail);

                _logger.LogInformation("📧 Email sent successfully to {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Email sending failed");
                throw;
            }
        }

        public async Task SendEmailVerificationAsync(string to, string name, string verificationLink)
        {
            var subject = "Verify Your Email - HealthAid";
            var body = $@"
                <h2>Hello {name},</h2>
                <p>Please verify your email by clicking the button below:</p>
                <a href='{verificationLink}' 
                   style='padding:10px 20px; background:#4CAF50; color:white; text-decoration:none;'>
                   Verify Email
                </a>
                <br/><br/>
                <p>This link expires in 24 hours.</p>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPasswordResetAsync(string to, string name, string resetLink)
        {
            var subject = "Reset Your Password - HealthAid";
            var body = $@"
                <h2>Hello {name},</h2>
                <p>You requested to reset your password. Use the link below:</p>
                <a href='{resetLink}' 
                   style='padding:10px 20px; background:#f44336; color:white; text-decoration:none;'>
                   Reset Password
                </a>
                <br/><br/>
                <p>This link expires in 1 hour.</p>";

            await SendEmailAsync(to, subject, body);
        }
    }
}
