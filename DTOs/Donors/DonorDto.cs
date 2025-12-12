namespace HealthAidAPI.DTOs.Donors
{
    public class DonorDto
    {
        public int DonorId { get; set; }
        public string Organization { get; set; } = string.Empty;
        public decimal TotalDonated { get; set; }
        public int? UserId { get; set; }
        public string DonorName { get; set; } = string.Empty; // الاسم من جدول User
        public string Email { get; set; } = string.Empty;
        public int DonationCount { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}