// Services/Interfaces/INgoService.cs
using HealthAidAPI.DTOs.NGOs;
using HealthAidAPI.Helpers;

namespace HealthAidAPI.Services.Interfaces
{
    public interface INgoService
    {
        Task<PagedResult<NgoDto>> GetAllNgosAsync(NgoFilterDto filter);
        Task<NgoDetailDto?> GetNgoByIdAsync(int id);
        Task<NgoDto> CreateNgoAsync(CreateNgoDto createNgoDto);
        Task<NgoDto?> UpdateNgoAsync(int id, UpdateNgoDto updateNgoDto);
        Task<bool> DeleteNgoAsync(int id);
        Task<bool> DeleteNgoByNameAsync(string name);
        Task<IEnumerable<NgoDto>> GetNgosByStatusAsync(string status);
        Task<IEnumerable<NgoDto>> SearchNgosAsync(string keyword);
        Task<IEnumerable<NgoDto>> GetNgosByAreaAsync(string area);
        Task<IEnumerable<NgoDto>> GetNgosWithMissionCountAsync();
        Task<NgoStatsDto> GetNgoStatsAsync();
        Task<bool> VerifyNgoAsync(int id, string status);
    }
}