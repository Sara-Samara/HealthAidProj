using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAidAPI.Models
{
    public class Consultation
    {
        [Key]
        public int ConsultationId { get; set; }

        [Required(ErrorMessage = "Consultation date is required.")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? ConsDate { get; set; }

        [StringLength(1000, ErrorMessage = "Diagnosis cannot exceed 1000 characters.")]
        [Display(Name = "Consultation Diagnosis")]
        public string? Diagnosis { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
        [RegularExpression("^(Pending|Scheduled|Completed|Cancelled|Rescheduled)$",
            ErrorMessage = "Status must be Pending, Scheduled, Completed, Cancelled, or Rescheduled.")]
        public string Status { get; set; } = "Pending";

        [StringLength(20, ErrorMessage = "Mode cannot exceed 20 characters.")]
        [RegularExpression("^(Online|In-person|Phone)$",
            ErrorMessage = "Mode must be Online, In-person, or Phone.")]
        public string? Mode { get; set; }

        [StringLength(1500, ErrorMessage = "Note cannot exceed 1500 characters.")]
        public string? Note { get; set; }

        [ForeignKey("Doctor")]
        public int? DoctorId { get; set; }
        public virtual Doctor? Doctor { get; set; }

        [ForeignKey("Patient")]
        public int? PatientId { get; set; }
        public virtual Patient? Patient { get; set; }

        [ForeignKey("Appointment")]
        public int? AppointmentId { get; set; }
        public virtual Appointment? Appointment { get; set; }

        public DateTime Date
        {
            get => ConsDate ?? DateTime.MinValue;
            set => ConsDate = value;
        }

        public string? Notes
        {
            get => Note;
            set => Note = value;
        }


        public TimeSpan Duration { get; set; } = TimeSpan.FromHours(1);
        public string ConsultationType { get; set; } = "General";
        public decimal? Fee { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}