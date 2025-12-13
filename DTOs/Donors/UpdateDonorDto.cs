using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Donors
{
    public class UpdateDonorDto
    {
        [StringLength(100, MinimumLength = 2)]
        public string? Organization { get; set; }
    }
}