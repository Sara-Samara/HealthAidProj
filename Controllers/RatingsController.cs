using HealthAidAPI.DTOs;
using HealthAidAPI.DTOs.Ratings;
using HealthAidAPI.Helpers;
using HealthAidAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingService _ratingService;
        private readonly ILogger<RatingsController> _logger;

        public RatingsController(IRatingService ratingService, ILogger<RatingsController> logger)
        {
            _ratingService = ratingService;
            _logger = logger;
        }

        private int CurrentUserId =>
            int.TryParse(
                User.FindFirst("id")?.Value ??
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("nameid")?.Value,
                out var id)
            ? id
            : -1;

        private ActionResult UnauthorizedResponse<T>() =>
            Unauthorized(new ApiResponse<T>
            {
                Success = false,
                Message = "You must be logged in to perform this action."
            });

        // ===============================
        // Public: Get all ratings
        // ===============================
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<PagedResult<RatingDto>>>> GetRatings([FromQuery] RatingFilterDto filter)
        {
            var result = await _ratingService.GetAllRatingsAsync(filter);

            return Ok(new ApiResponse<PagedResult<RatingDto>>
            {
                Success = true,
                Message = "Ratings retrieved successfully",
                Data = result
            });
        }

        // ===============================
        // Create Rating (Owner)
        // ===============================
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ApiResponse<RatingDto>>> CreateRating(CreateRatingDto dto)
        {
            if (CurrentUserId == -1)
                return UnauthorizedResponse<RatingDto>();

            dto.UserId = CurrentUserId; // overwrite client input

            var rating = await _ratingService.CreateRatingAsync(dto);

            return CreatedAtAction(nameof(GetRating), new { id = rating.RatingId },
                new ApiResponse<RatingDto>
                {
                    Success = true,
                    Message = "Rating created successfully",
                    Data = rating
                });
        }

        // ===============================
        // Update Rating (Owner only)
        // ===============================
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<RatingDto>>> UpdateRating(int id, UpdateRatingDto dto)
        {
            if (CurrentUserId == -1)
                return UnauthorizedResponse<RatingDto>();

            var updated = await _ratingService.UpdateRatingAsync(id, dto, CurrentUserId);

            if (updated == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Rating not found or you do not have permission to update it"
                });
            }

            return Ok(new ApiResponse<RatingDto>
            {
                Success = true,
                Message = "Rating updated successfully",
                Data = updated
            });
        }

        // ===============================
        // Delete Rating (Owner only)
        // ===============================
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<object>>> DeleteRating(int id)
        {
            if (CurrentUserId == -1)
                return UnauthorizedResponse<object>();

            var deleted = await _ratingService.DeleteRatingAsync(id, CurrentUserId);

            if (!deleted)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Rating not found or you do not have permission to delete it"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Rating deleted successfully"
            });
        }

        // ===============================
        // Public: Get Rating by ID
        // ===============================
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<RatingDto>>> GetRating(int id)
        {
            var rating = await _ratingService.GetRatingByIdAsync(id);
            if (rating == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Rating not found"
                });
            }

            return Ok(new ApiResponse<RatingDto>
            {
                Success = true,
                Message = "Rating retrieved successfully",
                Data = rating
            });
        }

        // ===============================
        // Public: Get Ratings by target
        // ===============================
        [HttpGet("target/{targetType}/{targetId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<RatingDto>>>> GetRatingsByTarget(string targetType, int targetId)
        {
            var ratings = await _ratingService.GetRatingsByTargetAsync(targetType, targetId);

            return Ok(new ApiResponse<IEnumerable<RatingDto>>
            {
                Success = true,
                Message = "Ratings loaded successfully",
                Data = ratings
            });
        }

        // ===============================
        // Public: Average rating
        // ===============================
        [HttpGet("average/{targetType}/{targetId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<AverageRatingDto>>> GetAverageRating(string targetType, int targetId)
        {
            var avg = await _ratingService.GetAverageRatingAsync(targetType, targetId);

            return Ok(new ApiResponse<AverageRatingDto>
            {
                Success = true,
                Message = "Average rating calculated successfully",
                Data = avg
            });
        }

        // ===============================
        // Public: Recent ratings
        // ===============================
        [HttpGet("recent")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<IEnumerable<RatingDto>>>> GetRecentRatings([FromQuery] int count = 10)
        {
            var ratings = await _ratingService.GetRecentRatingsAsync(count);

            return Ok(new ApiResponse<IEnumerable<RatingDto>>
            {
                Success = true,
                Message = "Recent ratings retrieved",
                Data = ratings
            });
        }
    }
}
