using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Auth
{
    public class VerifyEmailDto
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; } = string.Empty;
    }
}