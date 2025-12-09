// Models/HealthGuide.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthAidAPI.Models
{
    public class HealthGuide
    {
        [Key]
        public int HealthGuideId { get; set; }

        [Required(ErrorMessage = "Health guide title is required.")]
        [StringLength(150, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 150 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Category must be between 3 and 50 characters.")]
        [RegularExpression("^(Nutrition|Exercise|Mental Health|Disease Prevention|First Aid|General Wellness|Chronic Conditions|Pediatric Care|Senior Care|Women Health)$",
            ErrorMessage = "Category must be one of the predefined types.")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required.")]
        [MinLength(50, ErrorMessage = "Content must be at least 50 characters long.")]
        [MaxLength(10000, ErrorMessage = "Content cannot exceed 10000 characters.")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Language is required.")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "Language must be between 2 and 10 characters.")]
        [RegularExpression("^(en|ar|fr|es|de)$", ErrorMessage = "Language must be a supported language code.")]
        public string Language { get; set; } = "en";

        [StringLength(200, ErrorMessage = "Summary cannot exceed 200 characters")]
        public string? Summary { get; set; }

        [ForeignKey("User")]
        public int? UserId { get; set; }

        public virtual User? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public bool IsPublished { get; set; } = true;
        public int ViewCount { get; set; } = 0;
        public int LikeCount { get; set; } = 0;

        // Computed properties
        [NotMapped]
        public int ReadingTime => CalculateReadingTime();

        [NotMapped]
        public string TruncatedContent => Content.Length > 200 ? Content.Substring(0, 200) + "..." : Content;

        private int CalculateReadingTime()
        {
            // Average reading speed: 200 words per minute
            var wordCount = Content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            return (int)Math.Ceiling(wordCount / 200.0);
        }
    }
}