using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs
{
    public class ConsultationDto
    {
        public int ConsultationId { get; set; }
        public DateTime? ConsDate { get; set; }
        public string? Diagnosis { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Mode { get; set; }
        public string? Note { get; set; }
        public int? DoctorId { get; set; }
        public int? PatientId { get; set; }
        public int? AppointmentId { get; set; }
        public DoctorDto? Doctor { get; set; }
        public PatientDto? Patient { get; set; }
        //public AppointmentDto? Appointment { get; set; }
        public int PrescriptionCount { get; set; }
        public int TransactionCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

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

    public class UpdateConsultationDto
    {
        [DataType(DataType.DateTime)]
        public DateTime? ConsDate { get; set; }

        [StringLength(1000)]
        public string? Diagnosis { get; set; }

        [RegularExpression("^(Pending|Scheduled|Completed|Canceled|Rescheduled)$")]
        public string? Status { get; set; }

        [StringLength(20)]
        [RegularExpression("^(Online|In-person|Phone)$")]
        public string? Mode { get; set; }

        [StringLength(1500)]
        public string? Note { get; set; }

        public int? DoctorId { get; set; }
        public int? PatientId { get; set; }
        public int? AppointmentId { get; set; }
    }

    public class ConsultationFilterDto
    {
        public int? DoctorId { get; set; }
        public int? PatientId { get; set; }
        public string? Status { get; set; }
        public string? Mode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }

    public class ConsultationStatsDto
    {
        public int TotalConsultations { get; set; }
        public int CompletedConsultations { get; set; }
        public int PendingConsultations { get; set; }
        public int CanceledConsultations { get; set; }
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
        public Dictionary<string, int> ModeDistribution { get; set; } = new();
        public int ConsultationsThisMonth { get; set; }
        public double AverageConsultationsPerDay { get; set; }
    }

    public class UpdateConsultationStatusDto
    {
        [Required]
        [RegularExpression("^(Pending|Scheduled|Completed|Canceled|Rescheduled)$")]
        public string Status { get; set; } = string.Empty;
    }
}