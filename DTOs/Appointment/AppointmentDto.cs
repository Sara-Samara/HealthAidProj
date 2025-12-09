// DTOs/AppointmentDto.cs
using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs
{
    public class AppointmentDto
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Note { get; set; }
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public string? DoctorName { get; set; }
        public string? PatientName { get; set; }
        public string? DoctorSpecialization { get; set; }
        public DateTime CreatedAt { get; set; }
    }


    public class CreateAppointmentDto
    {
        [Required(ErrorMessage = "Appointment date is required")]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDate { get; set; }

        [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
        public string? Note { get; set; }

        [Required(ErrorMessage = "Doctor ID is required")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Patient ID is required")]
        public int PatientId { get; set; }
    }

    public class UpdateAppointmentDto
    {
        [DataType(DataType.DateTime)]
        public DateTime? AppointmentDate { get; set; }

        [StringLength(20)]
        [RegularExpression("^(Scheduled|Confirmed|Canceled|Completed|Pending|Rescheduled)$",
            ErrorMessage = "Status must be Scheduled, Confirmed, Canceled, Completed, Pending, or Rescheduled")]
        public string? Status { get; set; }

        [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
        public string? Note { get; set; }

        public int? DoctorId { get; set; }
        public int? PatientId { get; set; }
    }

    public class AppointmentFilterDto
    {
        public int? DoctorId { get; set; }
        public int? PatientId { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }

    public class AppointmentStatsDto
    {
        public int TotalAppointments { get; set; }
        public int ScheduledAppointments { get; set; }
        public int ConfirmedAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CanceledAppointments { get; set; }
        public int TodayAppointments { get; set; }
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
    }

    public class RescheduleAppointmentDto
    {
        [Required(ErrorMessage = "New appointment date is required")]
        [DataType(DataType.DateTime)]
        public DateTime NewAppointmentDate { get; set; }

        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string? Reason { get; set; }
    }
}