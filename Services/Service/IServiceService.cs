// Services/Interfaces/IServiceService.cs
using HealthAidAPI.DTOs.Services;
using HealthAidAPI.Helpers;

namespace HealthAidAPI.Services.Interfaces
{
    public interface IServiceService
    {
        Task<PagedResult<ServiceDto>> GetServicesAsync(ServiceFilterDto filter);
        Task<ServiceDto?> GetServiceByIdAsync(int id);
        Task<ServiceDto> CreateServiceAsync(CreateServiceDto createServiceDto);
        Task<ServiceDto?> UpdateServiceAsync(int id, UpdateServiceDto updateServiceDto);
        Task<bool> DeleteServiceAsync(int id);
        Task<ServiceDto?> AssignProviderAsync(int id, AssignServiceProviderDto assignProviderDto);
        Task<ServiceDto?> RemoveProviderAsync(int id);
        Task<ServiceStatsDto> GetServiceStatsAsync();
        Task<IEnumerable<ServiceDto>> GetServicesByProviderAsync(int providerId, string providerType);
        Task<IEnumerable<ServiceDto>> GetFreeServicesAsync();
        Task<IEnumerable<ServiceDto>> GetServicesByCategoryAsync(string category);
        Task<bool> ToggleServiceStatusAsync(int id);
    }
}