using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Ratings
{
    public class UpdateRatingDto
    {
        [Required(ErrorMessage = "Rating value is required")]
        [Range(1, 5, ErrorMessage = "Rating value must be between 1 and 5")]
        public int Value { get; set; }

        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string? Comment { get; set; }
    }
}