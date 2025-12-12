using HealthAidAPI.DTOs; // للـ PagedResult
using HealthAidAPI.DTOs.Donors;
using HealthAidAPI.Helpers;

namespace HealthAidAPI.Services.Interfaces
{
    public interface IDonorService
    {
        Task<PagedResult<DonorDto>> GetDonorsAsync(DonorFilterDto filter);
        Task<DonorDto?> GetDonorByIdAsync(int id);
        Task<DonorDto?> GetDonorByUserIdAsync(int userId);
        Task<DonorDto> CreateDonorAsync(CreateDonorDto dto);
        Task<DonorDto?> UpdateDonorAsync(int id, UpdateDonorDto dto);
        Task<bool> DeleteDonorAsync(int id);
        Task<List<DonorDto>> GetTopDonorsAsync(int count = 5);
    }
}