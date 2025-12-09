// DTOs/RatingDto.cs
using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Rating
{
    public class RatingDto
    {
        public int RatingId { get; set; }
        public string TargetType { get; set; } = string.Empty;
        public int TargetId { get; set; }
        public int Value { get; set; }
        public string? Comment { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string TargetName { get; set; } = string.Empty;
    }

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

    public class UpdateRatingDto
    {
        [Required(ErrorMessage = "Rating value is required")]
        [Range(1, 5, ErrorMessage = "Rating value must be between 1 and 5")]
        public int Value { get; set; }

        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string? Comment { get; set; }
    }

    public class RatingFilterDto
    {
        public string? TargetType { get; set; }
        public int? TargetId { get; set; }
        public int? UserId { get; set; }
        public int? MinRating { get; set; }
        public int? MaxRating { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = true;
    }

    public class RatingStatsDto
    {
        public int TotalRatings { get; set; }
        public double AverageRating { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
        public int FiveStarRatings { get; set; }
        public int OneStarRatings { get; set; }
        public int RecentRatings { get; set; }
    }

    public class AverageRatingDto
    {
        public string TargetType { get; set; } = string.Empty;
        public int TargetId { get; set; }
        public double Average { get; set; }
        public int TotalRatings { get; set; }
        public Dictionary<int, int> Distribution { get; set; } = new();
        public string TargetName { get; set; } = string.Empty;
    }
}