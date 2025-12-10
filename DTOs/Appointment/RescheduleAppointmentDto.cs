using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Appointments
{
    public class RescheduleAppointmentDto
    {
        [Required(ErrorMessage = "New appointment date is required")]
        [DataType(DataType.DateTime)]
        public DateTime NewAppointmentDate { get; set; }

        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string? Reason { get; set; }
    }
}