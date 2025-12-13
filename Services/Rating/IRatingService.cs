using HealthAidAPI.DTOs.Ratings;
using HealthAidAPI.Helpers;

namespace HealthAidAPI.Services.Interfaces
{
    public interface IRatingService
    {
        Task<PagedResult<RatingDto>> GetAllRatingsAsync(RatingFilterDto filter);
        Task<RatingDto?> GetRatingByIdAsync(int id);
        Task<RatingDto> CreateRatingAsync(CreateRatingDto createRatingDto);

        // secured versions (owner-only)
        Task<RatingDto?> UpdateRatingAsync(int id, UpdateRatingDto updateRatingDto, int userId);
        Task<bool> DeleteRatingAsync(int ratingId, int userId, bool isAdmin);


        Task<bool> DeleteRatingsByUserAsync(int userId);

        Task<IEnumerable<RatingDto>> GetRatingsByTargetAsync(string targetType, int targetId);
        Task<AverageRatingDto> GetAverageRatingAsync(string targetType, int targetId);
        Task<RatingStatsDto> GetRatingStatsAsync();
        Task<bool> HasUserRatedAsync(int userId, string targetType, int targetId);
        Task<IEnumerable<RatingDto>> GetRecentRatingsAsync(int count = 10);
    }
}
