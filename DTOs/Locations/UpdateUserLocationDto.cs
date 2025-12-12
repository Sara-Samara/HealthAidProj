using HealthAidAPI.DTOs.Messages;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HealthAidAPI.DTOs.Locations
{
    public class UpdateUserLocationDto
    {
        [JsonIgnore]
        [SwaggerSchema(ReadOnly = true)]
        public int UserId { get; set; }
        [Required]
        public decimal Latitude { get; set; }
        [Required]
        public decimal Longitude { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? Region { get; set; }
        public decimal? Accuracy { get; set; }
        public string LocationType { get; set; } = "Current";
        public bool IsPrimary { get; set; } = false;
    }
}