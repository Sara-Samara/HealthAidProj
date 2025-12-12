using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Analytics
{
    public class LogUserActivityDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string ActivityType { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}