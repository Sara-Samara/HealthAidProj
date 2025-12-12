using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Equipments
{
    public class EquipmentTransferDto
    {
        [Required(ErrorMessage = "New location is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Location must be between 3 and 200 characters")]
        public string NewLocation { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Transfer reason cannot exceed 500 characters")]
        public string? Reason { get; set; }
    }
}