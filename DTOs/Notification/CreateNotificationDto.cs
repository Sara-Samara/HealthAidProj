using HealthAidAPI.DTOs.Messages;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HealthAidAPI.DTOs.Notifications
{
    public class CreateNotificationDto
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
        [JsonIgnore]
        [SwaggerSchema(ReadOnly = true)]
        public int? SenderId { get; set; }

        [Required(ErrorMessage = "Receiver ID is required")]
        public int ReceiverId { get; set; }
    }
}