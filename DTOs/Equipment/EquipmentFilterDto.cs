namespace HealthAidAPI.DTOs.Equipments
{
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

        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Sorting
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }
}