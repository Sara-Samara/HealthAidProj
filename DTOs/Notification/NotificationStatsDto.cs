namespace HealthAidAPI.DTOs.Notifications
{
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