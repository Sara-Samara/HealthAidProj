using HealthAidAPI.DTOs.Extras;
using HealthAidAPI.Models.Extras;

namespace HealthAidAPI.Services.Interfaces
{
    public interface IVitalsService
    {
        Task<PatientVital> LogVitalsAsync(LogVitalDto dto, int patientId);
    }
}