using HealthAidAPI.DTOs.Recommendations;

namespace HealthAidAPI.Services.Interfaces
{
    public interface IRecommendationService
    {
        Task<List<DoctorRecommendationDto>> GenerateDoctorRecommendationsAsync(int patientId);

        Task<List<DoctorRecommendationDto>> GetStoredRecommendationsAsync(int patientId);

        Task<PatientHealthProfileDto> CreateHealthProfileAsync(CreateHealthProfileDto dto);
        Task<PatientHealthProfileDto?> GetHealthProfileAsync(int patientId);
        Task<PatientHealthProfileDto?> UpdateHealthProfileAsync(int patientId, UpdateHealthProfileDto dto);
    }
}