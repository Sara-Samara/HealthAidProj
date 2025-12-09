// DTOs/MentalSupportSessionDto.cs
using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs
{
    public class MentalSupportSessionDto
    {
        public int MentalSupportSessionId { get; set; }
        public string SessionType { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Notes { get; set; }
        public int? PatientId { get; set; }
        public int? DoctorId { get; set; }
        public string? PatientName { get; set; }
        public string? DoctorName { get; set; }
        public string? PatientEmail { get; set; }
        public string? DoctorEmail { get; set; }
        public string Status { get; set; } = "Scheduled";
        public TimeSpan Duration { get; set; } = TimeSpan.FromHours(1);
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

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

    public class UpdateMentalSupportSessionDto
    {
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Session type must be between 5 and 50 characters.")]
        public string? SessionType { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? Date { get; set; }

        [StringLength(1500, ErrorMessage = "Notes cannot exceed 1500 characters.")]
        public string? Notes { get; set; }

        [Range(15, 240, ErrorMessage = "Duration must be between 15 and 240 minutes.")]
        public int? DurationMinutes { get; set; }

        public string? Status { get; set; }
        public bool? IsCompleted { get; set; }
    }

    public class MentalSupportSessionFilterDto
    {
        public string? SessionType { get; set; }
        public int? PatientId { get; set; }
        public int? DoctorId { get; set; }
        public string? Status { get; set; }
        public bool? IsCompleted { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }

    public class MentalSupportSessionStatsDto
    {
        public int TotalSessions { get; set; }
        public int CompletedSessions { get; set; }
        public int UpcomingSessions { get; set; }
        public Dictionary<string, int> SessionTypesCount { get; set; } = new();
        public Dictionary<string, int> StatusCount { get; set; } = new();
        public int TotalPatients { get; set; }
        public int TotalDoctors { get; set; }
        public List<RecentSessionDto> RecentSessions { get; set; } = new();
    }

    public class RecentSessionDto
    {
        public int MentalSupportSessionId { get; set; }
        public string SessionType { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class SessionAvailabilityDto
    {
        [Required(ErrorMessage = "Doctor ID is required.")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public List<TimeSlotDto> AvailableSlots { get; set; } = new();
    }

    public class TimeSlotDto
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }
}