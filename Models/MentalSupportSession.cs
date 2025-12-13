using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAidAPI.Models
{
    public class MentalSupportSession
    {
        [Key]
        public int MentalSupportSessionId { get; set; }

        [Required(ErrorMessage = "Session type is required.")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Session type must be between 5 and 50 characters.")]
        public string SessionType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Session date and time are required.")]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

        [StringLength(1500, ErrorMessage = "Notes cannot exceed 1500 characters.")]
        public string? Notes { get; set; }

        [ForeignKey("Patient")]
        public int? PatientId { get; set; }
        public virtual Patient? Patient { get; set; }

        [ForeignKey("Doctor")]
        public int? DoctorId { get; set; }
        public virtual Doctor? Doctor { get; set; }


        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled, No-show
        public TimeSpan Duration { get; set; } = TimeSpan.FromHours(1);
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}