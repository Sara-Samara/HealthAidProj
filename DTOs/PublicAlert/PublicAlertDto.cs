namespace HealthAidAPI.DTOs.PublicAlerts
{
    public class PublicAlertDto
    {
        public int AlertId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty;
        public DateTime DatePosted { get; set; }
        public int UserId { get; set; }
        public string PostedBy { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public string Severity { get; set; } = "Medium";
        public bool IsActive { get; set; } = true;
        // ملاحظة: إذا كنت تريد إرجاع MoreInfoUrl و ExpiryDate هنا، يفضل إضافتهم
    }
}