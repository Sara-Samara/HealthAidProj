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

            // للتأكد من تحميل الإعدادات
            _logger.LogInformation("📧 Email Service Initialized");
            _logger.LogInformation($"SMTP Server: {_settings.SmtpServer}:{_settings.SmtpPort}");
            _logger.LogInformation($"Sender: {_settings.SenderEmail}");
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                // للتجربة فقط - سجل بدون إرسال فعلي
                _logger.LogInformation("📧 SIMULATED EMAIL (Development Mode)");
                _logger.LogInformation($"To: {to}");
                _logger.LogInformation($"Subject: {subject}");

                // اطبع رابط التحقق في الكونسول
                if (body.Contains("Verify Your Email"))
                {
                    var linkMatch = System.Text.RegularExpressions.Regex.Match(body, @"https?://[^\s""']+");
                    if (linkMatch.Success)
                    {
                        _logger.LogInformation($"🔗 VERIFICATION LINK: {linkMatch.Value}");
                    }
                }
                else if (body.Contains("Reset Your Password"))
                {
                    var linkMatch = System.Text.RegularExpressions.Regex.Match(body, @"https?://[^\s""']+");
                    if (linkMatch.Success)
                    {
                        _logger.LogInformation($"🔗 PASSWORD RESET LINK: {linkMatch.Value}");
                    }
                }

                _logger.LogInformation($"Body Preview: {body.Substring(0, Math.Min(200, body.Length))}...");

                await Task.Delay(100); // محاكاة وقت الإرسال

                _logger.LogInformation("✅ Email 'sent' successfully (simulated for development)");

                return; // خرج بدون إرسال فعلي

                // ⚠️ كود الإرسال الفعلي (معلق مؤقتاً):
                /*
                var smtp = new SmtpClient(_settings.SmtpServer)
                {
                    Port = _settings.SmtpPort,
                    Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                    EnableSsl = true,
                    UseDefaultCredentials = false
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
                */
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Email sending failed");
                // لا ترمي الاستثناء في مرحلة التطوير
                _logger.LogWarning("Continuing without email (development mode)");
            }
        }
        public async Task SendEmailVerificationAsync(string to, string name, string verificationLink)
        {
            var subject = "Verify Your Email - HealthAid";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2c3e50;'>Hello {name},</h2>
                    <p>Welcome to HealthAid! Please verify your email by clicking the button below:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{verificationLink}' 
                           style='display: inline-block; padding: 12px 24px; 
                                  background: #4CAF50; color: white; 
                                  text-decoration: none; border-radius: 5px;
                                  font-weight: bold;'>
                           Verify Email
                        </a>
                    </div>
                    <p style='color: #666; font-size: 14px;'>
                        Or copy and paste this link in your browser:<br/>
                        <code style='background: #f5f5f5; padding: 5px;'>{verificationLink}</code>
                    </p>
                    <p style='color: #999; font-size: 12px;'>
                        This link expires in 24 hours.<br/>
                        If you didn't request this, please ignore this email.
                    </p>
                </div>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPasswordResetAsync(string to, string name, string resetLink)
        {
            var subject = "Reset Your Password - HealthAid";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2c3e50;'>Hello {name},</h2>
                    <p>You requested to reset your password. Click the button below to create a new password:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{resetLink}' 
                           style='display: inline-block; padding: 12px 24px; 
                                  background: #f44336; color: white; 
                                  text-decoration: none; border-radius: 5px;
                                  font-weight: bold;'>
                           Reset Password
                        </a>
                    </div>
                    <p style='color: #666; font-size: 14px;'>
                        Or copy and paste this link in your browser:<br/>
                        <code style='background: #f5f5f5; padding: 5px;'>{resetLink}</code>
                    </p>
                    <p style='color: #999; font-size: 12px;'>
                        This link expires in 1 hour.<br/>
                        If you didn't request a password reset, please ignore this email.
                    </p>
                </div>";

            await SendEmailAsync(to, subject, body);
        }
    }
}