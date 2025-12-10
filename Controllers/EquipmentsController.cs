// Controllers/EquipmentController.cs
using HealthAidAPI.DTOs.Equipments;
using HealthAidAPI.Helpers;
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
    public class EquipmentController : ControllerBase
    {
        private readonly IEquipmentService _equipmentService;
        private readonly ILogger<EquipmentController> _logger;

        public EquipmentController(IEquipmentService equipmentService, ILogger<EquipmentController> logger)
        {
            _equipmentService = equipmentService;
            _logger = logger;
        }

        /// <summary>
        /// Get all equipment with filtering and pagination
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<EquipmentDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PagedResult<EquipmentDto>>> GetEquipment([FromQuery] EquipmentFilterDto filter)
        {
            try
            {
                var result = await _equipmentService.GetAllEquipmentAsync(filter);
                return Ok(new ApiResponse<PagedResult<EquipmentDto>>
                {
                    Success = true,
                    Message = "Equipment retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving equipment"
                });
            }
        }

        /// <summary>
        /// Get equipment by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EquipmentDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<EquipmentDto>> GetEquipment(int id)
        {
            try
            {
                var equipment = await _equipmentService.GetEquipmentByIdAsync(id);
                if (equipment == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Equipment with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<EquipmentDto>
                {
                    Success = true,
                    Message = "Equipment retrieved successfully",
                    Data = equipment
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment {EquipmentId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the equipment"
                });
            }
        }

        /// <summary>
        /// Create new equipment
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,NGOManager")]
        [ProducesResponseType(typeof(EquipmentDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<EquipmentDto>> CreateEquipment(CreateEquipmentDto createEquipmentDto)
        {
            try
            {
                var equipment = await _equipmentService.CreateEquipmentAsync(createEquipmentDto);
                return CreatedAtAction(nameof(GetEquipment), new { id = equipment.EquipmentId },
                    new ApiResponse<EquipmentDto>
                    {
                        Success = true,
                        Message = "Equipment created successfully",
                        Data = equipment
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
                _logger.LogError(ex, "Error creating equipment");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the equipment"
                });
            }
        }

        /// <summary>
        /// Update equipment
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,NGOManager")]
        [ProducesResponseType(typeof(EquipmentDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<EquipmentDto>> UpdateEquipment(int id, UpdateEquipmentDto updateEquipmentDto)
        {
            try
            {
                var equipment = await _equipmentService.UpdateEquipmentAsync(id, updateEquipmentDto);
                if (equipment == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Equipment with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<EquipmentDto>
                {
                    Success = true,
                    Message = "Equipment updated successfully",
                    Data = equipment
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
                _logger.LogError(ex, "Error updating equipment {EquipmentId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating the equipment"
                });
            }
        }

        /// <summary>
        /// Schedule maintenance for equipment
        /// </summary>
        [HttpPatch("{id}/maintenance")]
        [Authorize(Roles = "Admin,NGOManager")]
        [ProducesResponseType(typeof(EquipmentDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<EquipmentDto>> ScheduleMaintenance(int id, MaintenanceScheduleDto maintenanceDto)
        {
            try
            {
                var equipment = await _equipmentService.ScheduleMaintenanceAsync(id, maintenanceDto);
                if (equipment == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Equipment with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<EquipmentDto>
                {
                    Success = true,
                    Message = "Maintenance scheduled successfully",
                    Data = equipment
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling maintenance for equipment {EquipmentId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while scheduling maintenance"
                });
            }
        }

        /// <summary>
        /// Transfer equipment to new location
        /// </summary>
        [HttpPatch("{id}/transfer")]
        [Authorize(Roles = "Admin,NGOManager")]
        [ProducesResponseType(typeof(EquipmentDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<EquipmentDto>> TransferEquipment(int id, EquipmentTransferDto transferDto)
        {
            try
            {
                var equipment = await _equipmentService.TransferEquipmentAsync(id, transferDto);
                if (equipment == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Equipment with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<EquipmentDto>
                {
                    Success = true,
                    Message = "Equipment transferred successfully",
                    Data = equipment
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring equipment {EquipmentId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while transferring equipment"
                });
            }
        }

        /// <summary>
        /// Delete equipment
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,NGOManager")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteEquipment(int id)
        {
            try
            {
                var result = await _equipmentService.DeleteEquipmentAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Equipment with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Equipment deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting equipment {EquipmentId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the equipment"
                });
            }
        }

        /// <summary>
        /// Delete all equipment for a specific NGO
        /// </summary>
        [HttpDelete("ngo/{ngoId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteEquipmentByNgo(int ngoId)
        {
            try
            {
                var result = await _equipmentService.DeleteEquipmentByNgoAsync(ngoId);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"No equipment found for NGO {ngoId}"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"All equipment for NGO {ngoId} deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting equipment for NGO {NGOId}", ngoId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting NGO equipment"
                });
            }
        }

        /// <summary>
        /// Get equipment by type
        /// </summary>
        [HttpGet("type/{type}")]
        [ProducesResponseType(typeof(IEnumerable<EquipmentDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<EquipmentDto>>> GetEquipmentByType(string type)
        {
            try
            {
                var equipment = await _equipmentService.GetEquipmentByTypeAsync(type);
                if (!equipment.Any())
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"No equipment found of type '{type}'"
                    });
                }

                return Ok(new ApiResponse<IEnumerable<EquipmentDto>>
                {
                    Success = true,
                    Message = $"Equipment of type '{type}' retrieved successfully",
                    Data = equipment
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment by type {Type}", type);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving equipment by type"
                });
            }
        }

        /// <summary>
        /// Get equipment by location
        /// </summary>
        [HttpGet("location/{location}")]
        [ProducesResponseType(typeof(IEnumerable<EquipmentDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<EquipmentDto>>> GetEquipmentByLocation(string location)
        {
            try
            {
                var equipment = await _equipmentService.GetEquipmentByLocationAsync(location);
                if (!equipment.Any())
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"No equipment found in location '{location}'"
                    });
                }

                return Ok(new ApiResponse<IEnumerable<EquipmentDto>>
                {
                    Success = true,
                    Message = $"Equipment in location '{location}' retrieved successfully",
                    Data = equipment
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment by location {Location}", location);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving equipment by location"
                });
            }
        }

        /// <summary>
        /// Get critical equipment (poor condition or in maintenance)
        /// </summary>
        [HttpGet("critical")]
        [Authorize(Roles = "Admin,NGOManager")]
        [ProducesResponseType(typeof(IEnumerable<EquipmentDto>), 200)]
        public async Task<ActionResult<IEnumerable<EquipmentDto>>> GetCriticalEquipment()
        {
            try
            {
                var equipment = await _equipmentService.GetCriticalEquipmentAsync();
                return Ok(new ApiResponse<IEnumerable<EquipmentDto>>
                {
                    Success = true,
                    Message = "Critical equipment retrieved successfully",
                    Data = equipment
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving critical equipment");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving critical equipment"
                });
            }
        }

        /// <summary>
        /// Get equipment needing maintenance
        /// </summary>
        [HttpGet("maintenance-needed")]
        [Authorize(Roles = "Admin,NGOManager")]
        [ProducesResponseType(typeof(IEnumerable<EquipmentDto>), 200)]
        public async Task<ActionResult<IEnumerable<EquipmentDto>>> GetEquipmentNeedingMaintenance()
        {
            try
            {
                var equipment = await _equipmentService.GetEquipmentNeedingMaintenanceAsync();
                return Ok(new ApiResponse<IEnumerable<EquipmentDto>>
                {
                    Success = true,
                    Message = "Equipment needing maintenance retrieved successfully",
                    Data = equipment
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment needing maintenance");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving equipment needing maintenance"
                });
            }
        }

        /// <summary>
        /// Get equipment statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin,NGOManager")]
        [ProducesResponseType(typeof(EquipmentStatsDto), 200)]
        public async Task<ActionResult<EquipmentStatsDto>> GetEquipmentStats()
        {
            try
            {
                var stats = await _equipmentService.GetEquipmentStatsAsync();
                return Ok(new ApiResponse<EquipmentStatsDto>
                {
                    Success = true,
                    Message = "Equipment statistics retrieved successfully",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment statistics");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving equipment statistics"
                });
            }
        }

        /// <summary>
        /// Get equipment count for a specific NGO
        /// </summary>
        [HttpGet("count/ngo/{ngoId}")]
        [ProducesResponseType(typeof(int), 200)]
        public async Task<ActionResult<int>> GetEquipmentCountByNgo(int ngoId)
        {
            try
            {
                var count = await _equipmentService.GetEquipmentCountByNgoAsync(ngoId);
                return Ok(new ApiResponse<int>
                {
                    Success = true,
                    Message = $"Equipment count for NGO {ngoId}",
                    Data = count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting equipment count for NGO {NGOId}", ngoId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while getting equipment count"
                });
            }
        }

        /// <summary>
        /// Perform bulk update on multiple equipment
        /// </summary>
        [HttpPatch("bulk-update")]
        [Authorize(Roles = "Admin,NGOManager")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> PerformBulkUpdate(BulkEquipmentUpdateDto bulkUpdateDto)
        {
            try
            {
                var result = await _equipmentService.PerformBulkUpdateAsync(bulkUpdateDto);
                if (!result)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "No equipment found to update"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Bulk update completed on {bulkUpdateDto.EquipmentIds.Count} equipment items"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing bulk update on equipment");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while performing bulk update"
                });
            }
        }

        /// <summary>
        /// Search equipment
        /// </summary>
        [HttpGet("search/{searchTerm}")]
        [ProducesResponseType(typeof(IEnumerable<EquipmentDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<EquipmentDto>>> SearchEquipment(string searchTerm)
        {
            try
            {
                var equipment = await _equipmentService.SearchEquipmentAsync(searchTerm);
                if (!equipment.Any())
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"No equipment found matching '{searchTerm}'"
                    });
                }

                return Ok(new ApiResponse<IEnumerable<EquipmentDto>>
                {
                    Success = true,
                    Message = $"Equipment matching '{searchTerm}' retrieved successfully",
                    Data = equipment
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching equipment with term {SearchTerm}", searchTerm);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while searching equipment"
                });
            }
        }
    }
}