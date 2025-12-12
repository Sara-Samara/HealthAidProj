namespace HealthAidAPI.DTOs.Locations
{
    public class UserLocationDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Region { get; set; }
        public string LocationType { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}