using HealthAidAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class PublicAlert
{
    public int PublicAlertId { get; set; }

    [Required(ErrorMessage = "Alert title is required")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Alert description is required")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Region is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Region must be between 3 and 100 characters")]
    public string Region { get; set; } = string.Empty;

    [Required(ErrorMessage = "Alert type is required")]
    [StringLength(50, ErrorMessage = "Alert type cannot exceed 50 characters")]
    [RegularExpression("^(Health|Safety|Weather|Security|Other)$", ErrorMessage = "Invalid alert type")]
    public string AlertType { get; set; } = "Health";

    [Required(ErrorMessage = "Severity level is required")]
    [StringLength(20, ErrorMessage = "Severity cannot exceed 20 characters")]
    [RegularExpression("^(Low|Medium|High|Critical)$", ErrorMessage = "Severity must be Low, Medium, High, or Critical")]
    public string Severity { get; set; } = "Medium";

    [Required(ErrorMessage = "Status is required")]
    [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
    [RegularExpression("^(Active|Resolved|Expired)$", ErrorMessage = "Status must be Active, Resolved, or Expired")]
    public string Status { get; set; } = "Active";

    // ??? ??? ??????? ??????? ???????
    public bool IsActive { get; set; } = true;

    [Url(ErrorMessage = "Invalid URL format")]
    [StringLength(500, ErrorMessage = "More info URL cannot exceed 500 characters")]
    public string? MoreInfoUrl { get; set; }

    [StringLength(50, ErrorMessage = "Contact info cannot exceed 50 characters")]
    public string? ContactInfo { get; set; }

    [Range(-90.0, 90.0, ErrorMessage = "Invalid latitude value")]
    public double? Latitude { get; set; }

    [Range(-180.0, 180.0, ErrorMessage = "Invalid longitude value")]
    public double? Longitude { get; set; }

    public DateTime? ExpiryDate { get; set; }
    public int ViewCount { get; set; } = 0;

    [Required(ErrorMessage = "User ID is required")]
    [ForeignKey("User")]
    public int UserId { get; set; }

    [JsonIgnore]
    public virtual User User { get; set; } = null!;

    [DataType(DataType.DateTime)]
    public DateTime DatePosted { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdated { get; set; }

    public bool IsCurrentlyActive => Status == "Active" && (ExpiryDate == null || ExpiryDate > DateTime.UtcNow);

    public string TimeAgo => GetTimeAgo(DatePosted);

    private static string GetTimeAgo(DateTime date)
    {
        var timeSpan = DateTime.UtcNow - date;
        if (timeSpan.TotalMinutes < 1) return "Just now";
        if (timeSpan.TotalHours < 1) return $"{(int)timeSpan.TotalMinutes}m ago";
        if (timeSpan.TotalDays < 1) return $"{(int)timeSpan.TotalHours}h ago";
        if (timeSpan.TotalDays < 30) return $"{(int)timeSpan.TotalDays}d ago";
        return date.ToString("MMM dd, yyyy");
    }
}