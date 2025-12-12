using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Equipments
{
    public class BulkEquipmentUpdateDto
    {
        [Required(ErrorMessage = "Equipment IDs are required")]
        public List<int> EquipmentIds { get; set; } = new();

        [StringLength(200, MinimumLength = 3, ErrorMessage = "Location must be between 3 and 200 characters")]
        public string? NewLocation { get; set; }

        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
        [RegularExpression("^(Available|InUse|Maintenance|Reserved)$", ErrorMessage = "Invalid status")]
        public string? NewStatus { get; set; }

        [StringLength(20, ErrorMessage = "Condition cannot exceed 20 characters")]
        [RegularExpression("^(Excellent|Good|Fair|Poor)$", ErrorMessage = "Condition must be Excellent, Good, Fair, or Poor")]
        public string? NewCondition { get; set; }
    }
}