// Services/Interfaces/IDoctorService.cs
using HealthAidAPI.DTOs;
using HealthAidAPI.Models;

namespace HealthAidAPI.Services.Interfaces
{
    public interface IDoctorService
    {
        Task<PagedResult<DoctorDto>> GetDoctorsAsync(DoctorFilterDto filter);
        Task<DoctorDto?> GetDoctorByIdAsync(int id);
        Task<DoctorDto?> GetDoctorByUserIdAsync(int userId);
        Task<DoctorDto> CreateDoctorAsync(CreateDoctorDto doctorDto);
        Task<DoctorDto?> UpdateDoctorAsync(int id, UpdateDoctorDto doctorDto);
        Task<bool> DeleteDoctorAsync(int id);
        Task<bool> ToggleAvailabilityAsync(int id);
        Task<IEnumerable<string>> GetSpecializationsAsync();
        Task<DoctorStatsDto> GetDoctorStatsAsync();
        Task<IEnumerable<DoctorDto>> GetAvailableDoctorsAsync();
        Task<IEnumerable<DoctorDto>> GetDoctorsBySpecializationAsync(string specialization);
        Task<bool> LicenseNumberExistsAsync(string licenseNumber);
    }
}