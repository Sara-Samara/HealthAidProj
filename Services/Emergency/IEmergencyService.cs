using HealthAidAPI.DTOs.Emergency; // تأكد من الـ Namespace
using HealthAidAPI.Models.Emergency;

namespace HealthAidAPI.Services
{
    public interface IEmergencyService
    {
        Task<List<EmergencyCaseDto>> GetEmergencyCasesAsync(string? status);
        Task<EmergencyCaseDto?> GetEmergencyCaseByIdAsync(int id);
        Task<EmergencyCaseDto> CreateEmergencyAlertAsync(CreateEmergencyCaseDto dto);
        Task<EmergencyCaseDto?> AssignResponderAsync(int caseId, AssignResponderDto dto);
        Task<List<EmergencyResponder>> GetNearbyRespondersAsync(decimal lat, decimal lon, decimal radius);
        Task<EmergencyResponder> CreateResponderAsync(EmergencyResponder responder); // يفضل عمل DTO لهذا أيضاً
    }
}