// Services/Interfaces/ISponsorshipService.cs
using HealthAidAPI.DTOs.Sponsorships;
using HealthAidAPI.Helpers;


namespace HealthAidAPI.Services.Interfaces
{
    public interface ISponsorshipService
    {
        Task<PagedResult<SponsorshipDto>> GetSponsorshipsAsync(SponsorshipFilterDto filter);
        Task<SponsorshipDto?> GetSponsorshipByIdAsync(int id);
        Task<SponsorshipDto> CreateSponsorshipAsync(CreateSponsorshipDto createSponsorshipDto);
        Task<SponsorshipDto?> UpdateSponsorshipAsync(int id, UpdateSponsorshipDto updateSponsorshipDto);
        Task<bool> DeleteSponsorshipAsync(int id);
        Task<SponsorshipDto?> AddDonationAsync(int sponsorshipId, DonateToSponsorshipDto donateDto);
        Task<bool> UpdateStatusAsync(int id, string status);
        Task<SponsorshipStatsDto> GetSponsorshipStatsAsync();
        Task<IEnumerable<SponsorshipDto>> GetUrgentSponsorshipsAsync();
        Task<IEnumerable<SponsorshipDto>> GetFeaturedSponsorshipsAsync(int count = 5);
        Task<decimal> GetTotalRaisedForPatientAsync(int patientId);
        Task<bool> CloseSponsorshipAsync(int id);
    }
}