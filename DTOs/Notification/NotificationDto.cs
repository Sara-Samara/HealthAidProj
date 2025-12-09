// DTOs/NotificationDto.cs
using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string? SenderName { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
    }

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

        public int? SenderId { get; set; }

        [Required(ErrorMessage = "Receiver ID is required")]
        public int ReceiverId { get; set; }
    }

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

    public class NotificationFilterDto
    {
        public string? Search { get; set; }
        public string? Type { get; set; }
        public int? ReceiverId { get; set; }
        public int? SenderId { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }

    public class MarkNotificationsAsReadDto
    {
        public List<int>? NotificationIds { get; set; }
        public int? ReceiverId { get; set; }
    }

    public class NotificationStatsDto
    {
        public int TotalNotifications { get; set; }
        public int UnreadNotifications { get; set; }
        public int InfoCount { get; set; }
        public int WarningCount { get; set; }
        public int AlertCount { get; set; }
        public int SuccessCount { get; set; }
        public int PromotionalCount { get; set; }
        public int TodayNotifications { get; set; }
    }
}