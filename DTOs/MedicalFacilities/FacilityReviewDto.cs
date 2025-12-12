namespace HealthAidAPI.DTOs.MedicalFacilities
{
    public class FacilityReviewDto
    {
        public int Id { get; set; }
        public int FacilityId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty; // لإظهار اسم المستخدم
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}