namespace HealthAidAPI.DTOs.NGOs
{
    public class NgoDto
    {
        public int NGOId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public string AreaOfWork { get; set; } = string.Empty;
        public string VerifiedStatus { get; set; } = string.Empty;
        public string ContactedPerson { get; set; } = string.Empty;
        public int MissionCount { get; set; }
        public int EquipmentCount { get; set; }
        public int PatientCount { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}