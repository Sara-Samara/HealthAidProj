using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Consultations
{
    public class CreateConsultationDto
    {
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime? ConsDate { get; set; }

        [StringLength(1000)]
        public string? Diagnosis { get; set; }

        [Required]
        [RegularExpression("^(Pending|Scheduled|Completed|Canceled|Rescheduled)$")]
        public string Status { get; set; } = "Pending";

        [StringLength(20)]
        [RegularExpression("^(Online|In-person|Phone)$")]
        public string? Mode { get; set; }

        [StringLength(1500)]
        public string? Note { get; set; }

        [Required]
        public int? DoctorId { get; set; }

        [Required]
        public int? PatientId { get; set; }

        public int? AppointmentId { get; set; }
    }
}