using HealthAidAPI.DTOs.Users;
using HealthAidAPI.DTOs.Consultations;
using HealthAidAPI.DTOs.MedicineRequests;
using HealthAidAPI.DTOs.Donations; // تأكد من إنشاء هذا الـ Namespace للكلاس الأخير

namespace HealthAidAPI.DTOs.Transactions
{
    public class TransactionDto
    {
        public int TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int? ConsultationId { get; set; }
        public int? DonationId { get; set; }
        public int? MedicineRequestId { get; set; }
        public int? UserId { get; set; }

        // Navigation Properties DTOs
        public ConsultationDto? Consultation { get; set; }
        public DonationDto? Donation { get; set; }
        public MedicineRequestDto? MedicineRequest { get; set; }
        public UserDto? User { get; set; }

        public string TransactionType { get; set; } = string.Empty;
        public string FormattedAmount => Amount.ToString("C");
    }
}