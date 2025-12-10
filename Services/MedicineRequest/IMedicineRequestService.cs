// Services/Interfaces/IMedicineRequestService.cs
using HealthAidAPI.DTOs.MedicineRequests;
using HealthAidAPI.Helpers;

namespace HealthAidAPI.Services.MedicineRequest
{
    public interface IMedicineRequestService
    {
        Task<PagedResult<MedicineRequestDto>> GetMedicineRequestsAsync(MedicineRequestFilterDto filter);
        Task<MedicineRequestDto?> GetMedicineRequestByIdAsync(int id);
        Task<MedicineRequestDto> CreateMedicineRequestAsync(CreateMedicineRequestDto createMedicineRequestDto);
        Task<MedicineRequestDto?> UpdateMedicineRequestAsync(int id, UpdateMedicineRequestDto updateMedicineRequestDto);
        Task<bool> DeleteMedicineRequestAsync(int id);
        Task<MedicineRequestDto?> UpdateStatusAsync(int id, UpdateMedicineRequestStatusDto updateStatusDto);
        Task<MedicineRequestStatsDto> GetMedicineRequestStatsAsync();
        Task<IEnumerable<MedicineRequestDto>> GetUrgentMedicineRequestsAsync();
        Task<IEnumerable<MedicineRequestDto>> GetOverdueMedicineRequestsAsync();
        Task<IEnumerable<MedicineRequestDto>> GetMedicineRequestsByPatientAsync(int patientId);
        Task<bool> FulfillMedicineRequestAsync(int id);
        Task<bool> CancelMedicineRequestAsync(int id);
    }
}