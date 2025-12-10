using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.MentalSupportSessions
{
    public class CreateMentalSupportSessionDto
    {
        [Required(ErrorMessage = "Session type is required.")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Session type must be between 5 and 50 characters.")]
        public string SessionType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Session date and time are required.")]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

        [StringLength(1500, ErrorMessage = "Notes cannot exceed 1500 characters.")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Patient ID is required.")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Doctor ID is required.")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Duration is required.")]
        [Range(15, 240, ErrorMessage = "Duration must be between 15 and 240 minutes.")]
        public int DurationMinutes { get; set; } = 60;
    }
}