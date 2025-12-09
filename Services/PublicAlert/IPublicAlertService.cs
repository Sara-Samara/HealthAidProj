// Services/Interfaces/IPublicAlertService.cs
using HealthAidAPI.DTOs;
using HealthAidAPI.Models;

namespace HealthAidAPI.Services.Interfaces
{
    public interface IPublicAlertService
    {
        Task<PagedResult<PublicAlertDto>> GetAllAlertsAsync(PublicAlertFilterDto filter);
        Task<PublicAlertDto?> GetAlertByIdAsync(int id);
        Task<PublicAlertDto> CreateAlertAsync(CreatePublicAlertDto createAlertDto);
        Task<PublicAlertDto?> UpdateAlertAsync(int id, UpdatePublicAlertDto updateAlertDto);
        Task<bool> DeleteAlertAsync(int id);
        Task<bool> DeleteAllAlertsAsync();
        Task<IEnumerable<PublicAlertDto>> GetRecentAlertsAsync(int count = 5);
        Task<IEnumerable<PublicAlertDto>> GetAlertsByUserAsync(int userId);
        Task<AlertStatsDto> GetAlertStatsAsync();
        Task<IEnumerable<PublicAlertDto>> GetActiveAlertsAsync();
        Task<bool> ToggleAlertStatusAsync(int id, bool isActive);
    }
}