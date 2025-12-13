namespace HealthAidAPI.Models.Extras
{
    public class VolunteerOpportunity
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string RequiredSkills { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class VolunteerApplication
    {
        public int Id { get; set; }
        public int OpportunityId { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    }
}