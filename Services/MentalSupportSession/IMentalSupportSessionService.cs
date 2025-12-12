// Services/Interfaces/IMentalSupportSessionService.cs
using HealthAidAPI.DTOs.MentalSupportSessions;
using HealthAidAPI.Helpers;


namespace HealthAidAPI.Services.Interfaces
{
    public interface IMentalSupportSessionService
    {
        Task<PagedResult<MentalSupportSessionDto>> GetSessionsAsync(MentalSupportSessionFilterDto filter);
        Task<MentalSupportSessionDto?> GetSessionByIdAsync(int id);
        Task<MentalSupportSessionDto> CreateSessionAsync(CreateMentalSupportSessionDto sessionDto);
        Task<MentalSupportSessionDto?> UpdateSessionAsync(int id, UpdateMentalSupportSessionDto sessionDto);
        Task<bool> DeleteSessionAsync(int id);
        Task<bool> CompleteSessionAsync(int id, string? notes = null);
        Task<bool> CancelSessionAsync(int id, string? reason = null);
        Task<MentalSupportSessionStatsDto> GetSessionStatsAsync();
        Task<IEnumerable<MentalSupportSessionDto>> GetUpcomingSessionsAsync(int days = 7);
        Task<IEnumerable<MentalSupportSessionDto>> GetPatientSessionsAsync(int patientId);
        Task<IEnumerable<MentalSupportSessionDto>> GetDoctorSessionsAsync(int doctorId);
        Task<SessionAvailabilityDto> GetDoctorAvailabilityAsync(int doctorId, DateTime date);
        Task<bool> RescheduleSessionAsync(int sessionId, DateTime newDate);
        Task<IEnumerable<string>> GetSessionTypesAsync();
    }
}