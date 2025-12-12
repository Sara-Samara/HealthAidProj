using HealthAidAPI.DTOs.Extras;

namespace HealthAidAPI.Services.Interfaces
{
    public interface IPharmacyService
    {
        Task<List<MedicineSearchDto>> SearchMedicineAsync(string name);
        Task<bool> UpdateStockAsync(UpdateStockDto dto);
    }
}