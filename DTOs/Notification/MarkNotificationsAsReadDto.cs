namespace HealthAidAPI.DTOs.Notifications
{
    public class MarkNotificationsAsReadDto
    {
        public List<int>? NotificationIds { get; set; }
        public int? ReceiverId { get; set; }
    }
}