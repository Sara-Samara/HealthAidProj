// Services/Interfaces/IHealthGuideService.cs
using HealthAidAPI.Helpers;
using HealthAidAPI.DTOs.HealthGuides;

namespace HealthAidAPI.Services.Interfaces
{
    public interface IHealthGuideService
    {
        Task<PagedResult<HealthGuideDto>> GetHealthGuidesAsync(HealthGuideFilterDto filter);
        Task<HealthGuideDto?> GetHealthGuideByIdAsync(int id);
        Task<HealthGuideDto> CreateHealthGuideAsync(CreateHealthGuideDto healthGuideDto);
        Task<HealthGuideDto?> UpdateHealthGuideAsync(int id, UpdateHealthGuideDto healthGuideDto);
        Task<bool> DeleteHealthGuideAsync(int id);
        Task<bool> IncrementViewCountAsync(int id);
        Task<bool> IncrementLikeCountAsync(int id);
        Task<bool> TogglePublishStatusAsync(int id);
        Task<IEnumerable<string>> GetCategoriesAsync();
        Task<IEnumerable<string>> GetLanguagesAsync();
        Task<HealthGuideStatsDto> GetHealthGuideStatsAsync();
        Task<IEnumerable<HealthGuideDto>> GetPopularGuidesAsync(int count = 5);
        Task<IEnumerable<HealthGuideDto>> GetRelatedGuidesAsync(int guideId, int count = 3);
    }
}