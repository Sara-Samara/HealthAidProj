namespace HealthAidAPI.DTOs.MedicineRequests
{
    public class MedicineRequestDto
    {
        public int MedicineRequestId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Dosage { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? PreferredPharmacy { get; set; }
        public string Urgency { get; set; } = string.Empty;
        public DateTime? RequiredByDate { get; set; }
        public DateTime? FulfilledDate { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsUrgent { get; set; }
        public int DaysPending { get; set; }
        public int TransactionCount { get; set; }
    }
}