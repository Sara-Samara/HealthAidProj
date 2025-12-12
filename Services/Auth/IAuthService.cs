// Services/Interfaces/IAuthService.cs
using HealthAidAPI.DTOs.Auth;

namespace HealthAidAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginDto loginDto);
        Task<AuthResponse> RegisterAsync(RegisterDto registerDto);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
        Task<bool> VerifyEmailAsync(VerifyEmailDto verifyEmailDto);
        Task<bool> LogoutAsync(int userId);
        Task<bool> RevokeTokenAsync(string token);
    }
}