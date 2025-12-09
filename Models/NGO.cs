using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HealthAidAPI.Models
{
    public class NGO
    {
        public int NGOId { get; set; }

        [Required(ErrorMessage = "Organization name is required.")]
        [MaxLength(100, ErrorMessage = "Organization name cannot exceed 100 characters.")]
        [MinLength(3, ErrorMessage = "Organization name must be at least 3 characters long.")] // تم التعديل من 8 إلى 3
        public string OrganizationName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Area of work is required.")]
        [MaxLength(100, ErrorMessage = "Area of work cannot exceed 100 characters.")]
        [MinLength(3, ErrorMessage = "Area of work must be at least 3 characters long.")] // تم التعديل من 15 إلى 3
        public string AreaOfWork { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required.")]
        [MaxLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
        [RegularExpression("^(Verified|Pending|Rejected)$", ErrorMessage = "Status must be 'Verified', 'Pending', or 'Rejected'")] // إضافة تحقق من القيم المسموحة
        public string VerifiedStatus { get; set; } = "Pending"; // قيمة افتراضية

        [Required(ErrorMessage = "Name of contact person is required.")]
        [MaxLength(50, ErrorMessage = "Name of contact person cannot exceed 50 characters.")] // تم التعديل من 25 إلى 50
        [MinLength(2, ErrorMessage = "Name of contact person must be at least 2 characters long.")] // تم التعديل من 5 إلى 2
        public string ContactedPerson { get; set; } = string.Empty;

        // إضافة خصائص جديدة مفيدة
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        [MaxLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        public string? Phone { get; set; }

        [MaxLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string? Address { get; set; }

        [Url(ErrorMessage = "Invalid website URL.")]
        [MaxLength(200, ErrorMessage = "Website URL cannot exceed 200 characters.")]
        public string? Website { get; set; }

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [JsonIgnore]
        public virtual ICollection<NgoMission> NgoMessions { get; set; } = new List<NgoMission>();

        [JsonIgnore]
        public virtual ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();

        [JsonIgnore]
        public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();
    }
}