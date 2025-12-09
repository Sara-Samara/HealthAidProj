// Services/Interfaces/INgoMissionService.cs
using HealthAidAPI.DTOs.NGOmission;
using HealthAidAPI.Models;

namespace HealthAidAPI.Services.Interfaces
{
    public interface INgoMissionService
    {
        Task<PagedResult<NgoMissionDto>> GetAllMissionsAsync(NgoMissionFilterDto filter);
        Task<NgoMissionDto?> GetMissionByIdAsync(int id);
        Task<NgoMissionDto> CreateMissionAsync(CreateNgoMissionDto createMissionDto);
        Task<NgoMissionDto?> UpdateMissionAsync(int id, UpdateNgoMissionDto updateMissionDto);
        Task<bool> DeleteMissionAsync(int id);
        Task<bool> DeleteMissionsByNgoAsync(int ngoId);
        Task<IEnumerable<NgoMissionDto>> GetMissionsByDateRangeAsync(DateRangeDto dateRange);
        Task<IEnumerable<NgoMissionDto>> SearchMissionsByLocationAsync(string location);
        Task<int> GetMissionCountByNgoAsync(int ngoId);
        Task<MissionStatsDto> GetMissionStatsAsync();
        Task<IEnumerable<NgoMissionDto>> GetUpcomingMissionsAsync(int days = 30);
    }
}