namespace HealthAidAPI.DTOs.Analytics
{
    public class UserActivityDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty; 
        public string ActivityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}