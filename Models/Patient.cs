using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAidAPI.Models
{
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Patient name is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Patient name must be between 3 and 50 characters.")]
        [Display(Name = "Patient Full Name")] 
        public string PatientName { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Medical history cannot exceed 1000 characters.")]
        [Display(Name = "Medical History Details")]
        public string? MedicalHistory { get; set; }

        [DataType(DataType.Date)] 
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(10, ErrorMessage = "Gender cannot exceed 10 characters.")]
        [RegularExpression("^(Male|Female)$", ErrorMessage = "Gender must be Male or Female.")]
        public string? Gender { get; set; }

        [StringLength(5, ErrorMessage = "Blood type cannot exceed 5 characters.")]
        [RegularExpression("^(A\\+|A-|B\\+|B-|AB\\+|AB-|O\\+|O-)$", ErrorMessage = "Invalid blood type format.")]
        public string? BloodType { get; set; }

        [Required]

        [ForeignKey("User")] 
        public int UserId { get; set; }

        public virtual User? User { get; set; }

        [ForeignKey("NGO")]
        public int? NGOId { get; set; }

        public virtual NGO? NGO { get; set; }

        public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public virtual ICollection<MedicineRequest> MedicineRequests { get; set; } = new List<MedicineRequest>();
        public virtual ICollection<Sponsorship> Sponsorships { get; set; } = new List<Sponsorship>();
        public virtual ICollection<MentalSupportSession> MentalSupportSessions { get; set; } = new List<MentalSupportSession>();
    }
}