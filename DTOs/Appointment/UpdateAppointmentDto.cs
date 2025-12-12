using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Appointments
{
    public class UpdateAppointmentDto
    {
        [DataType(DataType.DateTime)]
        public DateTime? AppointmentDate { get; set; }

        [StringLength(20)]
        [RegularExpression("^(Scheduled|Confirmed|Canceled|Completed|Pending|Rescheduled)$",
            ErrorMessage = "Status must be: Scheduled, Confirmed, Canceled, Completed, Pending, or Rescheduled")]
        public string? Status { get; set; }

        [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
        public string? Note { get; set; }

        public int? DoctorId { get; set; }
        public int? PatientId { get; set; }
    }
}