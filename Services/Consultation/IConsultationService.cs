using HealthAidAPI.DTOs.Consultations;
using HealthAidAPI.Helpers;

namespace HealthAidAPI.Services.Interfaces
{
    public interface IConsultationService
    {
        Task<PagedResult<ConsultationDto>> GetConsultationsAsync(ConsultationFilterDto filter);
        Task<ConsultationDto?> GetConsultationByIdAsync(int id);
        Task<ConsultationDto> CreateConsultationAsync(CreateConsultationDto consultationDto);
        Task<ConsultationDto?> UpdateConsultationAsync(int id, UpdateConsultationDto consultationDto);
        Task<bool> DeleteConsultationAsync(int id);
        Task<bool> UpdateConsultationStatusAsync(int id, string status);
        Task<IEnumerable<ConsultationDto>> GetConsultationsByDoctorAsync(int doctorId);
        Task<IEnumerable<ConsultationDto>> GetConsultationsByPatientAsync(int patientId);
        Task<IEnumerable<ConsultationDto>> GetConsultationsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<ConsultationStatsDto> GetConsultationStatsAsync();
        Task<IEnumerable<string>> GetConsultationModesAsync();
        Task<bool> ConsultationExistsAsync(int id);
    }
}