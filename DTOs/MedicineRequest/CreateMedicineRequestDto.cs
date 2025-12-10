using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.MedicineRequests
{
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
}