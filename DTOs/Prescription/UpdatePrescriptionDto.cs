using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Prescriptions
{
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
}