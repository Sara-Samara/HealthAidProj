// Models/Appointment.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAidAPI.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required(ErrorMessage = "Appointment date is required.")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        [Display(Name = "Appointment Date and Time")]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Appointment status is required.")]
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
        [RegularExpression("^(Scheduled|Confirmed|Canceled|Completed|Pending|Rescheduled)$",
            ErrorMessage = "Status must be Scheduled, Confirmed, Canceled, Completed, Pending, or Rescheduled.")]
        [Display(Name = "Appointment Status")]
        public string Status { get; set; } = "Pending";

        [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters.")]
        [Display(Name = "Appointment Note")]
        public string? Note { get; set; }

        // تغيير من int? إلى int
        [Required(ErrorMessage = "Doctor ID is required.")]
        [ForeignKey("Doctor")]
        [Display(Name = "Doctor ID")]
        public int DoctorId { get; set; }

        public virtual Doctor? Doctor { get; set; }

        // تغيير من int? إلى int
        [Required(ErrorMessage = "Patient ID is required.")]
        [ForeignKey("Patient")]
        [Display(Name = "Patient ID")]
        public int PatientId { get; set; }

        public virtual Patient? Patient { get; set; }

        public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    }
}