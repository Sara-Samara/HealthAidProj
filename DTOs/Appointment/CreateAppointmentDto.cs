using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Appointments
{
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
}