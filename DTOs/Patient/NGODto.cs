namespace HealthAidAPI.DTOs.Patients
{
    public class NGODto
    {
        public int NGOId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public string? ContactInfo { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}