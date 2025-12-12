using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Notifications
{
    public class CreateNotificationByNameDto
    {
        [Required(ErrorMessage = "Notification title is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Notification message is required")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Message must be between 5 and 500 characters")]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "Notification type is required")]
        [RegularExpression("^(Info|Warning|Alert|Success|Promotional)$",
            ErrorMessage = "Type must be Info, Warning, Alert, Success, or Promotional")]
        public string Type { get; set; } = "Info";

        public string? SenderName { get; set; }

        [Required(ErrorMessage = "Receiver name is required")]
        public string ReceiverName { get; set; } = string.Empty;
    }
}