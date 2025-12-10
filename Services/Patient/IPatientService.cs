using HealthAidAPI.DTOs.Patients;
using HealthAidAPI.Helpers;
namespace HealthAidAPI.Services.Interfaces
{
    public interface IPatientService
    {
        Task<PagedResult<PatientDto>> GetPatientsAsync(PatientFilterDto filter);
        Task<PatientDto?> GetPatientByIdAsync(int id);
        Task<PatientDto?> GetPatientByUserIdAsync(int userId);
        Task<PatientDto> CreatePatientAsync(CreatePatientDto patientDto);
        Task<PatientDto?> UpdatePatientAsync(int id, UpdatePatientDto patientDto);
        Task<bool> DeletePatientAsync(int id);
        Task<bool> ToggleActiveStatusAsync(int id);
        Task<IEnumerable<string>> GetBloodTypesAsync();
        Task<PatientStatsDto> GetPatientStatsAsync();
        Task<IEnumerable<PatientDto>> GetPatientsByNGOAsync(int ngoId);
        Task<IEnumerable<PatientDto>> GetPatientsByBloodTypeAsync(string bloodType);
        Task<bool> PatientExistsAsync(int id);
        Task<int> GetTotalPatientsCountAsync();
        Task<IEnumerable<PatientMedicalSummaryDto>> GetPatientsMedicalSummaryAsync();
    }
}