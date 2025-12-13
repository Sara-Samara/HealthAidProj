using HealthAidAPI.DTOs.Donors;
using HealthAidAPI.Helpers;
using HealthAidAPI.Models; // للـ ApiResponse & PagedResult
using HealthAidAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // حماية عامة
    [Produces("application/json")]
    public class DonorsController : ControllerBase
    {
        private readonly IDonorService _donorService;
        private readonly ILogger<DonorsController> _logger;

        public DonorsController(IDonorService donorService, ILogger<DonorsController> logger)
        {
            _donorService = donorService;
            _logger = logger;
        }

        /// <summary>
        /// Get all donors with filtering and pagination
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<ApiResponse<PagedResult<DonorDto>>>> GetDonors([FromQuery] DonorFilterDto filter)
        {
            try
            {
                var result = await _donorService.GetDonorsAsync(filter);
                return Ok(new ApiResponse<PagedResult<DonorDto>>
                {
                    Success = true,
                    Message = "Donors retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving donors");
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get donor by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<DonorDto>>> GetDonor(int id)
        {
            var donor = await _donorService.GetDonorByIdAsync(id);
            if (donor == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = "Donor not found" });

            return Ok(new ApiResponse<DonorDto> { Success = true, Data = donor });
        }

        /// <summary>
        /// Create a new donor profile
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Donor")]
        public async Task<ActionResult<ApiResponse<DonorDto>>> CreateDonor(CreateDonorDto dto)
        {
            try
            {
                var donor = await _donorService.CreateDonorAsync(dto);
                return CreatedAtAction(nameof(GetDonor), new { id = donor.DonorId },
                    new ApiResponse<DonorDto> { Success = true, Message = "Donor profile created", Data = donor });
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating donor");
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update donor profile
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Donor")]
        public async Task<ActionResult<ApiResponse<DonorDto>>> UpdateDonor(int id, UpdateDonorDto dto)
        {
            var donor = await _donorService.UpdateDonorAsync(id, dto);
            if (donor == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = "Donor not found" });

            return Ok(new ApiResponse<DonorDto> { Success = true, Message = "Donor updated successfully", Data = donor });
        }

        /// <summary>
        /// Delete donor profile
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteDonor(int id)
        {
            var result = await _donorService.DeleteDonorAsync(id);
            if (!result)
                return NotFound(new ApiResponse<object> { Success = false, Message = "Donor not found" });

            return Ok(new ApiResponse<object> { Success = true, Message = "Donor deleted successfully" });
        }

        /// <summary>
        /// Get top donors by donation amount
        /// </summary>
        [HttpGet("top")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<List<DonorDto>>>> GetTopDonors([FromQuery] int count = 5)
        {
            var donors = await _donorService.GetTopDonorsAsync(count);
            return Ok(new ApiResponse<List<DonorDto>> { Success = true, Data = donors });
        }
    }
}