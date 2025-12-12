using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Prescriptions
{
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
}