namespace HealthAidAPI.DTOs.Recommendations
{
    public class DoctorRecommendationDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string RecommendationType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public decimal MatchScore { get; set; }
        public string Priority { get; set; } = "Medium";
        public bool IsViewed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}