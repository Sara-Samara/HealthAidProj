using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Locations
{
    public class CreateServiceAreaDto
    {
        [Required]
        public string AreaName { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Region { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal Radius { get; set; } = 10.00m;
        public string Description { get; set; } = string.Empty;
    }
}