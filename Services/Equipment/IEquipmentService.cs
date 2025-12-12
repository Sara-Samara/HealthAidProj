// Services/Interfaces/IEquipmentService.cs
using HealthAidAPI.DTOs.Equipments;
using HealthAidAPI.Helpers;
namespace HealthAidAPI.Services.Interfaces
{
    public interface IEquipmentService
    {
        Task<PagedResult<EquipmentDto>> GetAllEquipmentAsync(EquipmentFilterDto filter);
        Task<EquipmentDto?> GetEquipmentByIdAsync(int id);
        Task<EquipmentDto> CreateEquipmentAsync(CreateEquipmentDto createEquipmentDto);
        Task<EquipmentDto?> UpdateEquipmentAsync(int id, UpdateEquipmentDto updateEquipmentDto);
        Task<EquipmentDto?> ScheduleMaintenanceAsync(int id, MaintenanceScheduleDto maintenanceDto);
        Task<EquipmentDto?> TransferEquipmentAsync(int id, EquipmentTransferDto transferDto);
        Task<bool> DeleteEquipmentAsync(int id);
        Task<bool> DeleteEquipmentByNgoAsync(int ngoId);
        Task<IEnumerable<EquipmentDto>> GetEquipmentByTypeAsync(string type);
        Task<IEnumerable<EquipmentDto>> GetEquipmentByLocationAsync(string location);
        Task<IEnumerable<EquipmentDto>> GetCriticalEquipmentAsync();
        Task<IEnumerable<EquipmentDto>> GetEquipmentNeedingMaintenanceAsync();
        Task<EquipmentStatsDto> GetEquipmentStatsAsync();
        Task<int> GetEquipmentCountByNgoAsync(int ngoId);
        Task<bool> PerformBulkUpdateAsync(BulkEquipmentUpdateDto bulkUpdateDto);
        Task<IEnumerable<EquipmentDto>> SearchEquipmentAsync(string searchTerm);
    }
}