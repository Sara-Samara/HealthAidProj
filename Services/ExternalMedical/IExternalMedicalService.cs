using HealthAidAPI.DTOs.External;

namespace HealthAidAPI.Services.Interfaces
{
    public interface IExternalMedicalService
    {
        Task<DrugInfoDto?> GetDrugInfoAsync(string drugName);
    }
}