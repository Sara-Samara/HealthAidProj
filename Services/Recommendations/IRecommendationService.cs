using HealthAidAPI.DTOs.Recommendations;

namespace HealthAidAPI.Services.Interfaces
{
    public interface IRecommendationService
    {
        // توليد توصيات جديدة وحفظها
        Task<List<DoctorRecommendationDto>> GenerateDoctorRecommendationsAsync(int patientId);

        // جلب التوصيات المخزنة مسبقاً
        Task<List<DoctorRecommendationDto>> GetStoredRecommendationsAsync(int patientId);

        // إدارة الملف الصحي
        Task<PatientHealthProfileDto> CreateHealthProfileAsync(CreateHealthProfileDto dto);
        Task<PatientHealthProfileDto?> GetHealthProfileAsync(int patientId);
        Task<PatientHealthProfileDto?> UpdateHealthProfileAsync(int patientId, UpdateHealthProfileDto dto);
    }
}