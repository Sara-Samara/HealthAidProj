using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Consultations
{
    public class UpdateConsultationStatusDto
    {
        [Required]
        [RegularExpression("^(Pending|Scheduled|Completed|Canceled|Rescheduled)$")]
        public string Status { get; set; } = string.Empty;
    }
}