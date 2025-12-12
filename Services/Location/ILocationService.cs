using HealthAidAPI.DTOs.Locations;

namespace HealthAidAPI.Services
{
    public interface ILocationService
    {
        Task<UserLocationDto> UpdateUserLocationAsync(UpdateUserLocationDto dto);

        Task<List<UserLocationDto>> GetUserLocationsAsync(int userId);

        Task<EmergencyServicesResponseDto> GetEmergencyServicesAsync(decimal latitude, decimal longitude, decimal radius);

        Task<List<ServiceAreaDto>> GetServiceAreasAsync();

        Task<ServiceAreaDto> CreateServiceAreaAsync(CreateServiceAreaDto dto);
    }
}