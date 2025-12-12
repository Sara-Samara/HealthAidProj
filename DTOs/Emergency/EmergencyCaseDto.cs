namespace HealthAidAPI.DTOs.Emergency
{
    public class EmergencyCaseDto
    {
        public int Id { get; set; }
        public string EmergencyType { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // يمكن إضافة هذه الحقول لاحقاً إذا احتجتها
        // public string? ResponderName { get; set; }
        // public string? Description { get; set; }
    }
}