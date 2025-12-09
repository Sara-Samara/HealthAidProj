// DTOs/MessageFilterDto.cs
namespace HealthAidAPI.DTOs
{
    public class MessageFilterDto
    {
        public string? Search { get; set; }
        public int? SenderId { get; set; }
        public int? ReceiverId { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }
}