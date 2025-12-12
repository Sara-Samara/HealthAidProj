namespace HealthAidAPI.DTOs.Appointments
{
    public class AppointmentDto
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; }
        public string? Note { get; set; }
        public int DoctorId { get; set; }
        public int PatientId { get; set; }

        // Flattened properties for easier UI display
        public string? DoctorName { get; set; }
        public string? PatientName { get; set; }
        public string? DoctorSpecialization { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}