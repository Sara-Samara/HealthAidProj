using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Ratings
{
    public class CreateRatingDto
    {
        [Required(ErrorMessage = "Target type is required")]
        [StringLength(50, ErrorMessage = "Target type cannot exceed 50 characters")]
        [RegularExpression("^(NGO|Doctor|Equipment|Service)$", ErrorMessage = "Target type must be NGO, Doctor, Equipment, or Service")]
        public string TargetType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Target ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Target ID must be a positive number")]
        public int TargetId { get; set; }

        [Required(ErrorMessage = "Rating value is required")]
        [Range(1, 5, ErrorMessage = "Rating value must be between 1 and 5")]
        public int Value { get; set; }

        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string? Comment { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be a positive number")]
        public int UserId { get; set; }
    }
}