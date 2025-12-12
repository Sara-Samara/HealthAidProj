using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Extras
{
    public class BloodRequestDto
    {
        public int Id { get; set; }
        public string BloodType { get; set; } = string.Empty;
        public string HospitalName { get; set; } = string.Empty;
        public string UrgencyLevel { get; set; } = string.Empty;
        public string ContactNumber { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateBloodRequestDto
    {
        [Required]
        [RegularExpression("^(A|B|AB|O)[+-]$", ErrorMessage = "Invalid blood type (e.g., A+, O-)")]
        public string BloodType { get; set; } = string.Empty;

        [Required]
        public string HospitalName { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string ContactNumber { get; set; } = string.Empty;

        public string UrgencyLevel { get; set; } = "High";
    }
}