using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HealthAidAPI.Models
{
    public class MedicineRequest
    {
        public MedicineRequest()
        {
            Transactions = new List<Transaction>();
        }

        [Key]
        public int MedicineRequestId { get; set; }

        [Required, StringLength(100, MinimumLength = 3)]
        public string MedicineName { get; set; } = string.Empty;

        [Required, Range(1, 1000)]
        public int Quantity { get; set; }

        [Required, StringLength(50)]
        public string Dosage { get; set; } = string.Empty;

        [Required, StringLength(20)]
        [RegularExpression("^(Low|Medium|High|Emergency)$")]
        public string Priority { get; set; } = "Medium";

        [Required, StringLength(20)]
        [RegularExpression("^(Pending|Approved|InProgress|Fulfilled|Cancelled)$")]
        public string Status { get; set; } = "Pending";

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? PreferredPharmacy { get; set; }

        [Required, StringLength(20)]
        public string Urgency { get; set; } = "Normal";

        public DateTime? RequiredByDate { get; set; }
        public DateTime? FulfilledDate { get; set; }

        [Required, ForeignKey("Patient")]
        public int PatientId { get; set; }

        [JsonIgnore]
        public virtual Patient Patient { get; set; } = null!;

        // One-to-Many relationship
        public virtual ICollection<Transaction> Transactions { get; set; }

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public bool IsUrgent => Priority == "High" || Priority == "Emergency";
        public int DaysPending => (DateTime.UtcNow - RequestDate).Days;
    }
}