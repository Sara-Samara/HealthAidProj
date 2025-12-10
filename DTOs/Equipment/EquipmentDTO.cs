namespace HealthAidAPI.DTOs.Equipments
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
}