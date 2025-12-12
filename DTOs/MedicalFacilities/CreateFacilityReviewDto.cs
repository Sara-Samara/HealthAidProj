using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.MedicalFacilities
{
    public class CreateFacilityReviewDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(500)]
        public string Comment { get; set; } = string.Empty;
    }
}