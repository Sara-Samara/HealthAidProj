// Services/Interfaces/ITokenService.cs
using System.Security.Claims;
using HealthAidAPI.DTOs;

namespace HealthAidAPI.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(UserDto user);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        bool IsTokenRevoked(string token);
        void RevokeToken(string token);
        string GenerateRefreshToken();
    }
}