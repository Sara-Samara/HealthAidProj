using HealthAidAPI.DTOs.Messages;
using System.Text.Json.Serialization;

namespace HealthAidAPI.DTOs.Notifications
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        [JsonIgnore]
        [SwaggerSchema(ReadOnly = true)]
        public int? SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string? SenderName { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
    }
}