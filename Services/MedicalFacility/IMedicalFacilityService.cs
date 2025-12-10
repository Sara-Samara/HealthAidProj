using HealthAidAPI.DTOs.MedicalFacilities;

namespace HealthAidAPI.Services.Interfaces
{
    public interface IMedicalFacilityService
    {
        Task<List<MedicalFacilityDto>> GetMedicalFacilitiesAsync(string? type, bool? verified, decimal? minRating);

        Task<MedicalFacilityDto?> GetMedicalFacilityByIdAsync(int id);

        Task<MedicalFacilityDto> CreateMedicalFacilityAsync(CreateMedicalFacilityDto dto);

        Task<MedicalFacilityDto?> UpdateMedicalFacilityAsync(int id, UpdateMedicalFacilityDto dto);

        Task<FacilityReviewDto> AddFacilityReviewAsync(int facilityId, CreateFacilityReviewDto dto);

        Task<List<MedicalFacilityDto>> GetNearbyFacilitiesAsync(decimal latitude, decimal longitude, decimal radius, string? type);
    }
}