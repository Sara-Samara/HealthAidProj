using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Notifications
{
    public class UpdateNotificationDto
    {
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters")]
        public string? Title { get; set; }

        [StringLength(500, MinimumLength = 5, ErrorMessage = "Message must be between 5 and 500 characters")]
        public string? Message { get; set; }

        [RegularExpression("^(Info|Warning|Alert|Success|Promotional)$",
            ErrorMessage = "Type must be Info, Warning, Alert, Success, or Promotional")]
        public string? Type { get; set; }

        public bool? IsRead { get; set; }
    }
}