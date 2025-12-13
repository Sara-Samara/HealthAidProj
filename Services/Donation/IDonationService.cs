using HealthAidAPI.DTOs.Donations;
using HealthAidAPI.Helpers;
using HealthAidAPI.Models;

namespace HealthAidAPI.Services.Interfaces
{
    public interface IDonationService
    {
        Task<PagedResult<DonationDto>> GetDonationsAsync(DonationFilterDto filter);
        Task<DonationDto?> GetDonationByIdAsync(int id);
        Task<DonationDto> CreateDonationAsync(CreateDonationDto dto);
        Task<DonationDto?> UpdateDonationStatusAsync(int id, UpdateDonationStatusDto dto);
        Task<DonationStatsDto> GetDonationStatsAsync();
        Task<List<DonationDto>> GetDonationsByDonorAsync(int donorId);
        Task<List<DonationDto>> GetRecentDonationsAsync(int count = 5);
    }
}