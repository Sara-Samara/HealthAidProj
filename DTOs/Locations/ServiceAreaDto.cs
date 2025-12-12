using HealthAidAPI.DTOs.Messages;
using System.Text.Json.Serialization;

namespace HealthAidAPI.DTOs.Locations
{
    public class ServiceAreaDto
    {
      
        public int Id { get; set; }
        public string AreaName { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Region { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal Radius { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}