using HealthAidAPI.DTOs.Donations;
using HealthAidAPI.Helpers;
using HealthAidAPI.Models;
using HealthAidAPI.Services;
using HealthAidAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class DonationsController : ControllerBase
    {
        private readonly IDonationService _donationService;
        private readonly ILogger<DonationsController> _logger;

        public DonationsController(IDonationService donationService, ILogger<DonationsController> logger)
        {
            _donationService = donationService;
            _logger = logger;
        }

        /// <summary>
        /// Get all donations with filtering and pagination
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<PagedResult<DonationDto>>>> GetDonations([FromQuery] DonationFilterDto filter)
        {
            try
            {
                var result = await _donationService.GetDonationsAsync(filter);
                return Ok(new ApiResponse<PagedResult<DonationDto>>
                {
                    Success = true,
                    Message = "Donations retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving donations");
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get donation by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<DonationDto>>> GetDonation(int id)
        {
            var donation = await _donationService.GetDonationByIdAsync(id);
            if (donation == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = "Donation not found" });

            return Ok(new ApiResponse<DonationDto> { Success = true, Data = donation });
        }

        /// <summary>
        /// Create a new donation
        /// </summary>
        [HttpPost]
        [AllowAnonymous] // نسمح بالتبرع حتى بدون تسجيل دخول أحياناً، أو يمكن تقييدها
        public async Task<ActionResult<ApiResponse<DonationDto>>> CreateDonation(CreateDonationDto dto)
        {
            try
            {
                var donation = await _donationService.CreateDonationAsync(dto);
                return CreatedAtAction(nameof(GetDonation), new { id = donation.DonationId },
                    new ApiResponse<DonationDto> { Success = true, Message = "Donation created successfully", Data = donation });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating donation");
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update donation status (Admin only)
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<DonationDto>>> UpdateStatus(int id, UpdateDonationStatusDto dto)
        {
            var donation = await _donationService.UpdateDonationStatusAsync(id, dto);
            if (donation == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = "Donation not found" });

            return Ok(new ApiResponse<DonationDto> { Success = true, Message = "Donation status updated", Data = donation });
        }

        /// <summary>
        /// Get donations for a specific donor
        /// </summary>
        [HttpGet("donor/{donorId}")]
        [Authorize(Roles = "Admin,Donor")]
        public async Task<ActionResult<ApiResponse<List<DonationDto>>>> GetDonorDonations(int donorId)
        {
            var donations = await _donationService.GetDonationsByDonorAsync(donorId);
            return Ok(new ApiResponse<List<DonationDto>> { Success = true, Data = donations });
        }

        /// <summary>
        /// Get donation statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<DonationStatsDto>>> GetStats()
        {
            var stats = await _donationService.GetDonationStatsAsync();
            return Ok(new ApiResponse<DonationStatsDto> { Success = true, Data = stats });
        }
    }
}