using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAidAPI.Models.Extras
{
    public class BloodRequest
    {
        public int Id { get; set; }

        [Required]
        public string BloodType { get; set; } = string.Empty; // A+, O-, etc.

        [Required]
        public string HospitalName { get; set; } = string.Empty;

        public string ContactNumber { get; set; } = string.Empty;

        [Required]
        public string UrgencyLevel { get; set; } = "High"; // Low, Medium, High, Critical

        public bool IsFulfilled { get; set; } = false;

        public int RequesterId { get; set; } // User ID
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}