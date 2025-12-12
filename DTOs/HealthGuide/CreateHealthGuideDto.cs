using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.HealthGuides
{
    public class CreateHealthGuideDto
    {
        [Required(ErrorMessage = "Health guide title is required.")]
        [StringLength(150, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 150 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Category must be between 3 and 50 characters.")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required.")]
        [MinLength(50, ErrorMessage = "Content must be at least 50 characters long.")]
        [MaxLength(10000, ErrorMessage = "Content cannot exceed 10000 characters.")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Language is required.")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "Language must be between 2 and 10 characters.")]
        public string Language { get; set; } = "en";

        [StringLength(200, ErrorMessage = "Summary cannot exceed 200 characters")]
        public string? Summary { get; set; }

        public int? UserId { get; set; }
        public bool IsPublished { get; set; } = true;
    }
}