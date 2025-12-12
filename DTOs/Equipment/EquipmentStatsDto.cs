namespace HealthAidAPI.DTOs.Equipments
{
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
}