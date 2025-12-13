using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Analytics
{
    public class LogUserActivityDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string ActivityType { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        public string? Metadata { get; set; }
    }
}