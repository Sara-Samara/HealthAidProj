namespace HealthAidAPI.DTOs.MedicalFacilities
{
    public class MedicalFacilityDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string ContactNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Services { get; set; } = string.Empty;
        public string OperatingHours { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool Verified { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<FacilityReviewDto> Reviews { get; set; } = new();
    }
}