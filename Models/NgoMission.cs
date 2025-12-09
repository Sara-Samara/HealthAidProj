using HealthAidAPI.Helpers;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HealthAidAPI.Models
{
    public class NgoMission
    {
        [Key]
        public int NgoMessionId { get; set; }

        [Required(ErrorMessage = "Mission description is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        [FutureDate(ErrorMessage = "Start date must be in the future")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.Date)]
        [DateAfter("StartDate", ErrorMessage = "End date must be after start date")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Location is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Location must be between 3 and 100 characters")]
        public string Location { get; set; } = string.Empty;

        // إضافة خصائص جديدة
        [Required(ErrorMessage = "Mission type is required")]
        [StringLength(50, ErrorMessage = "Mission type cannot exceed 50 characters")]
        public string MissionType { get; set; } = "Medical"; // Medical, Educational, Emergency, etc.

        [Required(ErrorMessage = "Status is required")]
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
        [RegularExpression("^(Planned|Ongoing|Completed|Cancelled)$", ErrorMessage = "Status must be Planned, Ongoing, Completed, or Cancelled")]
        public string Status { get; set; } = "Planned";

        [Range(0, 1000, ErrorMessage = "Volunteers count must be between 0 and 1000")]
        public int RequiredVolunteers { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Budget must be a positive number")]
        public decimal Budget { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }

        // العلاقات
        [Required(ErrorMessage = "NGO ID is required")]
        [ForeignKey("NGO")]
        public int NGOId { get; set; }

        [JsonIgnore]
        public virtual NGO NGO { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // طرق مساعدة
        public bool IsActive => Status == "Ongoing" && StartDate <= DateTime.Today && EndDate >= DateTime.Today;
        public int DaysRemaining => IsActive ? (EndDate - DateTime.Today).Days : 0;
    }
}