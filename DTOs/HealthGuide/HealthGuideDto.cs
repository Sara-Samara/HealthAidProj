// DTOs/HealthGuideDto.cs
using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs
{
    public class HealthGuideDto
    {
        public int HealthGuideId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Language { get; set; } = "en";
        public string? Summary { get; set; }
        public int? UserId { get; set; }
        public string? AuthorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsPublished { get; set; } = true;
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int ReadingTime { get; set; }
        public string TruncatedContent { get; set; } = string.Empty;
    }

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

    public class UpdateHealthGuideDto
    {
        [StringLength(150, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 150 characters.")]
        public string? Title { get; set; }

        [StringLength(50, MinimumLength = 3, ErrorMessage = "Category must be between 3 and 50 characters.")]
        public string? Category { get; set; }

        [MinLength(50, ErrorMessage = "Content must be at least 50 characters long.")]
        [MaxLength(10000, ErrorMessage = "Content cannot exceed 10000 characters.")]
        public string? Content { get; set; }

        [StringLength(10, MinimumLength = 2, ErrorMessage = "Language must be between 2 and 10 characters.")]
        public string? Language { get; set; }

        [StringLength(200, ErrorMessage = "Summary cannot exceed 200 characters")]
        public string? Summary { get; set; }

        public bool? IsPublished { get; set; }
    }

    public class HealthGuideFilterDto
    {
        public string? Search { get; set; }
        public string? Category { get; set; }
        public string? Language { get; set; }
        public bool? IsPublished { get; set; }
        public int? UserId { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }

    public class HealthGuideStatsDto
    {
        public int TotalGuides { get; set; }
        public int PublishedGuides { get; set; }
        public int TotalViews { get; set; }
        public int TotalLikes { get; set; }
        public Dictionary<string, int> CategoriesCount { get; set; } = new();
        public Dictionary<string, int> LanguagesCount { get; set; } = new();
        public List<PopularGuideDto> PopularGuides { get; set; } = new();
    }

    public class PopularGuideDto
    {
        public int HealthGuideId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
    }
}