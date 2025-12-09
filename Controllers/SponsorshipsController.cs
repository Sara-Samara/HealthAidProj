// Controllers/SponsorshipsController.cs
using HealthAidAPI.DTOs;
using HealthAidAPI.Models;
using HealthAidAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class SponsorshipsController : ControllerBase
    {
        private readonly ISponsorshipService _sponsorshipService;
        private readonly ILogger<SponsorshipsController> _logger;

        public SponsorshipsController(ISponsorshipService sponsorshipService, ILogger<SponsorshipsController> logger)
        {
            _sponsorshipService = sponsorshipService;
            _logger = logger;
        }

        /// <summary>
        /// Get all sponsorships with filtering and pagination
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResult<SponsorshipDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PagedResult<SponsorshipDto>>> GetSponsorships([FromQuery] SponsorshipFilterDto filter)
        {
            try
            {
                var result = await _sponsorshipService.GetSponsorshipsAsync(filter);
                return Ok(new ApiResponse<PagedResult<SponsorshipDto>>
                {
                    Success = true,
                    Message = "Sponsorships retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sponsorships");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving sponsorships"
                });
            }
        }

        /// <summary>
        /// Get sponsorship by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SponsorshipDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<SponsorshipDto>> GetSponsorship(int id)
        {
            try
            {
                var sponsorship = await _sponsorshipService.GetSponsorshipByIdAsync(id);
                if (sponsorship == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Sponsorship with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<SponsorshipDto>
                {
                    Success = true,
                    Message = "Sponsorship retrieved successfully",
                    Data = sponsorship
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sponsorship {SponsorshipId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the sponsorship"
                });
            }
        }

        /// <summary>
        /// Create a new sponsorship
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(SponsorshipDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<SponsorshipDto>> CreateSponsorship(CreateSponsorshipDto createSponsorshipDto)
        {
            try
            {
                var sponsorship = await _sponsorshipService.CreateSponsorshipAsync(createSponsorshipDto);
                return CreatedAtAction(nameof(GetSponsorship), new { id = sponsorship.SponsorshipId },
                    new ApiResponse<SponsorshipDto>
                    {
                        Success = true,
                        Message = "Sponsorship created successfully",
                        Data = sponsorship
                    });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sponsorship");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the sponsorship"
                });
            }
        }

        /// <summary>
        /// Update a sponsorship
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(SponsorshipDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<SponsorshipDto>> UpdateSponsorship(int id, UpdateSponsorshipDto updateSponsorshipDto)
        {
            try
            {
                var sponsorship = await _sponsorshipService.UpdateSponsorshipAsync(id, updateSponsorshipDto);
                if (sponsorship == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Sponsorship with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<SponsorshipDto>
                {
                    Success = true,
                    Message = "Sponsorship updated successfully",
                    Data = sponsorship
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sponsorship {SponsorshipId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating the sponsorship"
                });
            }
        }

        /// <summary>
        /// Delete a sponsorship
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteSponsorship(int id)
        {
            try
            {
                var result = await _sponsorshipService.DeleteSponsorshipAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Sponsorship with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Sponsorship deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sponsorship {SponsorshipId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the sponsorship"
                });
            }
        }

        /// <summary>
        /// Add donation to a sponsorship
        /// </summary>
        [HttpPost("{id}/donate")]
        [Authorize]
        [ProducesResponseType(typeof(SponsorshipDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<SponsorshipDto>> AddDonation(int id, DonateToSponsorshipDto donateDto)
        {
            try
            {
                var sponsorship = await _sponsorshipService.AddDonationAsync(id, donateDto);
                if (sponsorship == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Sponsorship with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<SponsorshipDto>
                {
                    Success = true,
                    Message = "Donation added successfully",
                    Data = sponsorship
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding donation to sponsorship {SponsorshipId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while adding the donation"
                });
            }
        }

        /// <summary>
        /// Update sponsorship status
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            try
            {
                var result = await _sponsorshipService.UpdateStatusAsync(id, status);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Sponsorship with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Sponsorship status updated to {status}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for sponsorship {SponsorshipId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating the status"
                });
            }
        }

        /// <summary>
        /// Get sponsorship statistics
        /// </summary>
        [HttpGet("stats")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SponsorshipStatsDto), 200)]
        public async Task<ActionResult<SponsorshipStatsDto>> GetStats()
        {
            try
            {
                var stats = await _sponsorshipService.GetSponsorshipStatsAsync();
                return Ok(new ApiResponse<SponsorshipStatsDto>
                {
                    Success = true,
                    Message = "Sponsorship statistics retrieved successfully",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sponsorship statistics");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving statistics"
                });
            }
        }

        /// <summary>
        /// Get urgent sponsorships
        /// </summary>
        [HttpGet("urgent")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<SponsorshipDto>), 200)]
        public async Task<ActionResult<IEnumerable<SponsorshipDto>>> GetUrgentSponsorships()
        {
            try
            {
                var sponsorships = await _sponsorshipService.GetUrgentSponsorshipsAsync();
                return Ok(new ApiResponse<IEnumerable<SponsorshipDto>>
                {
                    Success = true,
                    Message = "Urgent sponsorships retrieved successfully",
                    Data = sponsorships
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving urgent sponsorships");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving urgent sponsorships"
                });
            }
        }

        /// <summary>
        /// Get featured sponsorships
        /// </summary>
        [HttpGet("featured")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<SponsorshipDto>), 200)]
        public async Task<ActionResult<IEnumerable<SponsorshipDto>>> GetFeaturedSponsorships([FromQuery] int count = 5)
        {
            try
            {
                var sponsorships = await _sponsorshipService.GetFeaturedSponsorshipsAsync(count);
                return Ok(new ApiResponse<IEnumerable<SponsorshipDto>>
                {
                    Success = true,
                    Message = "Featured sponsorships retrieved successfully",
                    Data = sponsorships
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving featured sponsorships");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving featured sponsorships"
                });
            }
        }

        /// <summary>
        /// Get total raised for a patient
        /// </summary>
        [HttpGet("patient/{patientId}/total-raised")]
        [Authorize]
        [ProducesResponseType(typeof(decimal), 200)]
        public async Task<ActionResult<decimal>> GetTotalRaisedForPatient(int patientId)
        {
            try
            {
                var total = await _sponsorshipService.GetTotalRaisedForPatientAsync(patientId);
                return Ok(new ApiResponse<decimal>
                {
                    Success = true,
                    Message = $"Total raised for patient {patientId}",
                    Data = total
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total raised for patient {PatientId}", patientId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving total raised"
                });
            }
        }

        /// <summary>
        /// Close a sponsorship
        /// </summary>
        [HttpPost("{id}/close")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CloseSponsorship(int id)
        {
            try
            {
                var result = await _sponsorshipService.CloseSponsorshipAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Sponsorship with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Sponsorship closed successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing sponsorship {SponsorshipId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while closing the sponsorship"
                });
            }
        }
    }
}