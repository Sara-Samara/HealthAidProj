using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Prescriptions
{
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