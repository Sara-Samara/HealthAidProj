using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HealthAidAPI.Models
{
    public class Equipment
    {
        public int EquipmentId { get; set; }

        [Required(ErrorMessage = "Equipment name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Equipment name must be between 3 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Equipment type is required")]
        [StringLength(50, ErrorMessage = "Equipment type cannot exceed 50 characters")]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "Current location is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Location must be between 3 and 200 characters")]
        public string CurrentLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Availability status is required")]
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
        [RegularExpression("^(Available|InUse|Maintenance|Reserved)$", ErrorMessage = "Invalid status")]
        public string AvailableStatus { get; set; } = "Available";


        [StringLength(50, ErrorMessage = "Model cannot exceed 50 characters")]
        public string? Model { get; set; }

        [StringLength(100, ErrorMessage = "Serial number cannot exceed 100 characters")]
        public string? SerialNumber { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a positive number")]
        public int Quantity { get; set; } = 1;

        [Range(0, double.MaxValue, ErrorMessage = "Value must be a positive number")]
        public decimal? EstimatedValue { get; set; }

        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }

        [StringLength(20, ErrorMessage = "Condition cannot exceed 20 characters")]
        public string Condition { get; set; } = "Good";


        [Required(ErrorMessage = "NGO ID is required")]
        [ForeignKey("NGO")]
        public int NGOId { get; set; }

        [JsonIgnore]
        public virtual NGO NGO { get; set; } = null!;

        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }


        public bool NeedsMaintenance => NextMaintenanceDate.HasValue && NextMaintenanceDate <= DateTime.UtcNow.AddDays(30);
        public bool IsCritical => Condition == "Poor" || AvailableStatus == "Maintenance";
    }
}