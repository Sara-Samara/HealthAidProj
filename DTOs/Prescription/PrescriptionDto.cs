// DTOs/PrescriptionDto.cs
using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs
{
    public class PrescriptionDto
    {
        public int PrescriptionId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string Dosages { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public int ConsultationId { get; set; }
        public string? ConsultationNotes { get; set; }
        public DateTime? ConsultationDate { get; set; }
        public string? DoctorName { get; set; }
        public string? PatientName { get; set; }
        public int? DoctorId { get; set; }
        public int? PatientId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Status { get; set; } = "Active";
        public string? Instructions { get; set; }
        public int? RefillsRemaining { get; set; }
        public bool IsCompleted { get; set; } = false;
    }

    public class CreatePrescriptionDto
    {
        [Required(ErrorMessage = "Medicine name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Medicine name must be between 2 and 100 characters.")]
        public string MedicineName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Dosage instructions are required.")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Dosage instructions must be between 5 and 200 characters.")]
        public string Dosages { get; set; } = string.Empty;

        [Required(ErrorMessage = "Duration is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Duration must be between 2 and 50 characters.")]
        public string Duration { get; set; } = string.Empty;

        [Required(ErrorMessage = "Consultation ID is required.")]
        public int ConsultationId { get; set; }

        [StringLength(500, ErrorMessage = "Instructions cannot exceed 500 characters.")]
        public string? Instructions { get; set; }

        [Range(0, 10, ErrorMessage = "Refills must be between 0 and 10.")]
        public int? RefillsRemaining { get; set; } = 0;

        public string Status { get; set; } = "Active";
    }

    public class UpdatePrescriptionDto
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Medicine name must be between 2 and 100 characters.")]
        public string? MedicineName { get; set; }

        [StringLength(200, MinimumLength = 5, ErrorMessage = "Dosage instructions must be between 5 and 200 characters.")]
        public string? Dosages { get; set; }

        [StringLength(50, MinimumLength = 2, ErrorMessage = "Duration must be between 2 and 50 characters.")]
        public string? Duration { get; set; }

        [StringLength(500, ErrorMessage = "Instructions cannot exceed 500 characters.")]
        public string? Instructions { get; set; }

        [Range(0, 10, ErrorMessage = "Refills must be between 0 and 10.")]
        public int? RefillsRemaining { get; set; }

        public string? Status { get; set; }
        public bool? IsCompleted { get; set; }
    }

    public class PrescriptionFilterDto
    {
        public string? MedicineName { get; set; }
        public int? ConsultationId { get; set; }
        public int? PatientId { get; set; }
        public int? DoctorId { get; set; }
        public string? Status { get; set; }
        public bool? IsCompleted { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }

    public class PrescriptionStatsDto
    {
        public int TotalPrescriptions { get; set; }
        public int ActivePrescriptions { get; set; }
        public int CompletedPrescriptions { get; set; }
        public int ExpiredPrescriptions { get; set; }
        public Dictionary<string, int> MedicineFrequency { get; set; } = new();
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public List<RecentPrescriptionDto> RecentPrescriptions { get; set; } = new();
    }

    public class RecentPrescriptionDto
    {
        public int PrescriptionId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class PatientPrescriptionSummaryDto
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int TotalPrescriptions { get; set; }
        public int ActivePrescriptions { get; set; }
        public List<PrescriptionDto> RecentPrescriptions { get; set; } = new();
    }

    public class RefillRequestDto
    {
        [Required(ErrorMessage = "Prescription ID is required.")]
        public int PrescriptionId { get; set; }

        [Required(ErrorMessage = "Reason for refill is required.")]
        [StringLength(200, MinimumLength = 10, ErrorMessage = "Reason must be between 10 and 200 characters.")]
        public string Reason { get; set; } = string.Empty;

        [Range(1, 10, ErrorMessage = "Refill quantity must be between 1 and 10.")]
        public int Quantity { get; set; } = 1;
    }
}