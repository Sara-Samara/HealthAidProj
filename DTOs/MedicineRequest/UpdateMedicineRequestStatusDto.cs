using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.MedicineRequests
{
    public class UpdateMedicineRequestStatusDto
    {
        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(Pending|Approved|InProgress|Fulfilled|Cancelled)$", ErrorMessage = "Invalid status")]
        public string Status { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }
}