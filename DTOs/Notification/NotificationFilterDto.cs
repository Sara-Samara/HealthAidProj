using HealthAidAPI.DTOs.Messages;
using System.Text.Json.Serialization;

namespace HealthAidAPI.DTOs.Notifications
{
    public class NotificationFilterDto
    {
        public string? Search { get; set; }
        public string? Type { get; set; }
        public int? ReceiverId { get; set; }
        [JsonIgnore]
        [SwaggerSchema(ReadOnly = true)]
        public int? SenderId { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        // Sorting
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }
}