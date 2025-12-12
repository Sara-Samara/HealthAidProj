using HealthAidAPI.DTOs.Messages;
using System.Text.Json.Serialization;

public class MessageFilterDto
{
    public string? Search { get; set; }
    [JsonIgnore]
    [SwaggerSchema(ReadOnly = true)]
    public int? SenderId { get; set; }
    public int? ReceiverId { get; set; }
    public bool? IsRead { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public string? SortBy { get; set; }
    public bool SortDesc { get; set; } = false;

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // ⭐⭐ مهم جداً
    public string? CurrentUserName { get; set; }
}
