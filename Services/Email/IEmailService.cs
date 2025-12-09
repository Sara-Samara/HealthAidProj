// Services/Interfaces/IEmailService.cs
namespace HealthAidAPI.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendEmailVerificationAsync(string to, string name, string verificationLink);
        Task SendPasswordResetAsync(string to, string name, string resetLink);
    }
}