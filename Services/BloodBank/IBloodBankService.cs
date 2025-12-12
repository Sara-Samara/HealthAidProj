using HealthAidAPI.DTOs.Extras;

namespace HealthAidAPI.Services.Interfaces
{
    public interface IBloodBankService
    {
        Task<BloodRequestDto> CreateRequestAsync(CreateBloodRequestDto dto, int userId);
        Task<List<BloodRequestDto>> GetActiveRequestsAsync();
    }
}