namespace HealthAidAPI.DTOs.Services
{
    public class ServiceDto
    {
        public int ServiceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? ProviderId { get; set; }
        public string? ProviderType { get; set; }
        public string? ProviderName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsFree => !Price.HasValue || Price == 0;
    }
}