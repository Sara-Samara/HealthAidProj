using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.MentalSupportSessions
{
    public class SessionAvailabilityDto
    {
        [Required(ErrorMessage = "Doctor ID is required.")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        public List<TimeSlotDto> AvailableSlots { get; set; } = new();
    }
}