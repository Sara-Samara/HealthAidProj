// DTOs/MedicineRequestDto.cs
using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs
{
    public class MedicineRequestDto
    {
        public int MedicineRequestId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Dosage { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? PreferredPharmacy { get; set; }
        public string Urgency { get; set; } = string.Empty;
        public DateTime? RequiredByDate { get; set; }
        public DateTime? FulfilledDate { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsUrgent { get; set; }
        public int DaysPending { get; set; }
        public int TransactionCount { get; set; }
    }

    public class CreateMedicineRequestDto
    {
        [Required(ErrorMessage = "Medicine name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Medicine name must be between 3 and 100 characters")]
        public string MedicineName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Dosage is required")]
        [StringLength(50, ErrorMessage = "Dosage cannot exceed 50 characters")]
        public string Dosage { get; set; } = string.Empty;

        [Required(ErrorMessage = "Priority is required")]
        [RegularExpression("^(Low|Medium|High|Emergency)$", ErrorMessage = "Priority must be Low, Medium, High, or Emergency")]
        public string Priority { get; set; } = "Medium";

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        [StringLength(100, ErrorMessage = "Preferred pharmacy cannot exceed 100 characters")]
        public string? PreferredPharmacy { get; set; }

        [Required(ErrorMessage = "Urgency is required")]
        [StringLength(20, ErrorMessage = "Urgency cannot exceed 20 characters")]
        public string Urgency { get; set; } = "Normal";

        public DateTime? RequiredByDate { get; set; }

        [Required(ErrorMessage = "Patient ID is required")]
        public int PatientId { get; set; }
    }

    public class UpdateMedicineRequestDto
    {
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Medicine name must be between 3 and 100 characters")]
        public string? MedicineName { get; set; }

        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
        public int? Quantity { get; set; }

        [StringLength(50, ErrorMessage = "Dosage cannot exceed 50 characters")]
        public string? Dosage { get; set; }

        [RegularExpression("^(Low|Medium|High|Emergency)$", ErrorMessage = "Priority must be Low, Medium, High, or Emergency")]
        public string? Priority { get; set; }

        [RegularExpression("^(Pending|Approved|InProgress|Fulfilled|Cancelled)$", ErrorMessage = "Invalid status")]
        public string? Status { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

        [StringLength(100, ErrorMessage = "Preferred pharmacy cannot exceed 100 characters")]
        public string? PreferredPharmacy { get; set; }

        [StringLength(20, ErrorMessage = "Urgency cannot exceed 20 characters")]
        public string? Urgency { get; set; }

        public DateTime? RequiredByDate { get; set; }
        public DateTime? FulfilledDate { get; set; }
    }

    public class MedicineRequestFilterDto
    {
        public string? Search { get; set; }
        public string? MedicineName { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? Urgency { get; set; }
        public int? PatientId { get; set; }
        public bool? IsUrgent { get; set; }
        public DateTime? RequestDateFrom { get; set; }
        public DateTime? RequestDateTo { get; set; }
        public DateTime? RequiredByDateFrom { get; set; }
        public DateTime? RequiredByDateTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }

    public class UpdateMedicineRequestStatusDto
    {
        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(Pending|Approved|InProgress|Fulfilled|Cancelled)$", ErrorMessage = "Invalid status")]
        public string Status { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }

    public class MedicineRequestStatsDto
    {
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int InProgressRequests { get; set; }
        public int FulfilledRequests { get; set; }
        public int CancelledRequests { get; set; }
        public int UrgentRequests { get; set; }
        public int EmergencyRequests { get; set; }
        public Dictionary<string, int> StatusCount { get; set; } = new();
        public Dictionary<string, int> PriorityCount { get; set; } = new();
        public int AverageDaysPending { get; set; }
        public int OverdueRequests { get; set; }
    }
}