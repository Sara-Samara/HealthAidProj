// DTOs/EquipmentDto.cs
using HealthAidAPI.Helpers;
using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs
{
    public class EquipmentDto
    {
        public int EquipmentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string CurrentLocation { get; set; } = string.Empty;
        public string AvailableStatus { get; set; } = "Available";
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? Description { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal? EstimatedValue { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public string Condition { get; set; } = "Good";
        public int NGOId { get; set; }
        public string NGOName { get; set; } = string.Empty;
        public DateTime AddedDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool NeedsMaintenance { get; set; }
        public bool IsCritical { get; set; }
        public int DaysUntilMaintenance { get; set; }
    }

    public class CreateEquipmentDto
    {
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

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;

        [Range(0, double.MaxValue, ErrorMessage = "Value must be a positive number")]
        public decimal? EstimatedValue { get; set; }

        public DateTime? LastMaintenanceDate { get; set; }

        public DateTime? NextMaintenanceDate { get; set; }

        [StringLength(20, ErrorMessage = "Condition cannot exceed 20 characters")]
        [RegularExpression("^(Excellent|Good|Fair|Poor)$", ErrorMessage = "Condition must be Excellent, Good, Fair, or Poor")]
        public string Condition { get; set; } = "Good";

        [Required(ErrorMessage = "NGO ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "NGO ID must be a positive number")]
        public int NGOId { get; set; }
    }

    public class UpdateEquipmentDto
    {
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Equipment name must be between 3 and 100 characters")]
        public string? Name { get; set; }

        [StringLength(50, ErrorMessage = "Equipment type cannot exceed 50 characters")]
        public string? Type { get; set; }

        [StringLength(200, MinimumLength = 3, ErrorMessage = "Location must be between 3 and 200 characters")]
        public string? CurrentLocation { get; set; }

        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters")]
        [RegularExpression("^(Available|InUse|Maintenance|Reserved)$", ErrorMessage = "Invalid status")]
        public string? AvailableStatus { get; set; }

        [StringLength(50, ErrorMessage = "Model cannot exceed 50 characters")]
        public string? Model { get; set; }

        [StringLength(100, ErrorMessage = "Serial number cannot exceed 100 characters")]
        public string? SerialNumber { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int? Quantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Value must be a positive number")]
        public decimal? EstimatedValue { get; set; }

        public DateTime? LastMaintenanceDate { get; set; }

        public DateTime? NextMaintenanceDate { get; set; }

        [StringLength(20, ErrorMessage = "Condition cannot exceed 20 characters")]
        [RegularExpression("^(Excellent|Good|Fair|Poor)$", ErrorMessage = "Condition must be Excellent, Good, Fair, or Poor")]
        public string? Condition { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "NGO ID must be a positive number")]
        public int? NGOId { get; set; }
    }

    public class EquipmentFilterDto
    {
        public string? Search { get; set; }
        public string? Type { get; set; }
        public string? Location { get; set; }
        public string? Status { get; set; }
        public string? Condition { get; set; }
        public int? NGOId { get; set; }
        public bool? NeedsMaintenance { get; set; }
        public bool? IsCritical { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }

    public class MaintenanceScheduleDto
    {
        [Required(ErrorMessage = "Next maintenance date is required")]
        [FutureDate(ErrorMessage = "Next maintenance date must be in the future")]
        public DateTime NextMaintenanceDate { get; set; }

        [StringLength(500, ErrorMessage = "Maintenance notes cannot exceed 500 characters")]
        public string? Notes { get; set; }
    }

    public class EquipmentTransferDto
    {
        [Required(ErrorMessage = "New location is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Location must be between 3 and 200 characters")]
        public string NewLocation { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Transfer reason cannot exceed 500 characters")]
        public string? Reason { get; set; }
    }

    public class EquipmentStatsDto
    {
        public int TotalEquipment { get; set; }
        public int AvailableEquipment { get; set; }
        public int InUseEquipment { get; set; }
        public int MaintenanceEquipment { get; set; }
        public int ReservedEquipment { get; set; }
        public int CriticalEquipment { get; set; }
        public int NeedsMaintenanceCount { get; set; }
        public decimal TotalValue { get; set; }
        public Dictionary<string, int> EquipmentByType { get; set; } = new();
        public Dictionary<string, int> EquipmentByCondition { get; set; } = new();
        public Dictionary<string, int> EquipmentByLocation { get; set; } = new();
    }

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