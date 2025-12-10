using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.MedicineRequests
{
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
}