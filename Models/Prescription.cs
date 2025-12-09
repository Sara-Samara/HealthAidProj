// Models/Prescription.cs (محدث)
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAidAPI.Models
{
    public class Prescription
    {
        [Key]
        public int PrescriptionId { get; set; }

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
        [ForeignKey("Consultation")]
        public int ConsultationId { get; set; }

        [Required(ErrorMessage = "Consultation details are required.")]
        public virtual Consultation Consultation { get; set; } = null!;

        public string? Instructions { get; set; }
        public int RefillsRemaining { get; set; } = 0;
        public string Status { get; set; } = "Active"; // Active, Completed, Expired, Cancelled
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}