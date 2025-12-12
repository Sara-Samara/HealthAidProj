using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.MentalSupportSessions
{
    public class UpdateMentalSupportSessionDto
    {
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Session type must be between 5 and 50 characters.")]
        public string? SessionType { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? Date { get; set; }

        [StringLength(1500, ErrorMessage = "Notes cannot exceed 1500 characters.")]
        public string? Notes { get; set; }

        [Range(15, 240, ErrorMessage = "Duration must be between 15 and 240 minutes.")]
        public int? DurationMinutes { get; set; }

        public string? Status { get; set; }
        public bool? IsCompleted { get; set; }
    }
}