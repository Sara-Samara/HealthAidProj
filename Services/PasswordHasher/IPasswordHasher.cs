// Services/Interfaces/IPasswordHasher.cs
namespace HealthAidAPI.Services.Interfaces
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }
}