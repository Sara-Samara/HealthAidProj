// Controllers/ServicesController.cs
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
    public class ServicesController : ControllerBase
    {
        private readonly IServiceService _serviceService;
        private readonly ILogger<ServicesController> _logger;

        public ServicesController(IServiceService serviceService, ILogger<ServicesController> logger)
        {
            _serviceService = serviceService;
            _logger = logger;
        }

        /// <summary>
        /// Get all services with filtering and pagination
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResult<ServiceDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PagedResult<ServiceDto>>> GetServices([FromQuery] ServiceFilterDto filter)
        {
            try
            {
                var result = await _serviceService.GetServicesAsync(filter);
                return Ok(new ApiResponse<PagedResult<ServiceDto>>
                {
                    Success = true,
                    Message = "Services retrieved successfully",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving services");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving services"
                });
            }
        }

        /// <summary>
        /// Get service by ID
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ServiceDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ServiceDto>> GetService(int id)
        {
            try
            {
                var service = await _serviceService.GetServiceByIdAsync(id);
                if (service == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Service with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<ServiceDto>
                {
                    Success = true,
                    Message = "Service retrieved successfully",
                    Data = service
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service {ServiceId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the service"
                });
            }
        }

        /// <summary>
        /// Create a new service
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(ServiceDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ServiceDto>> CreateService(CreateServiceDto createServiceDto)
        {
            try
            {
                var service = await _serviceService.CreateServiceAsync(createServiceDto);
                return CreatedAtAction(nameof(GetService), new { id = service.ServiceId },
                    new ApiResponse<ServiceDto>
                    {
                        Success = true,
                        Message = "Service created successfully",
                        Data = service
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
                _logger.LogError(ex, "Error creating service");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while creating the service"
                });
            }
        }

        /// <summary>
        /// Update a service
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(ServiceDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ServiceDto>> UpdateService(int id, UpdateServiceDto updateServiceDto)
        {
            try
            {
                var service = await _serviceService.UpdateServiceAsync(id, updateServiceDto);
                if (service == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Service with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<ServiceDto>
                {
                    Success = true,
                    Message = "Service updated successfully",
                    Data = service
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
                _logger.LogError(ex, "Error updating service {ServiceId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating the service"
                });
            }
        }

        /// <summary>
        /// Delete a service
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteService(int id)
        {
            try
            {
                var result = await _serviceService.DeleteServiceAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Service with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Service deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service {ServiceId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the service"
                });
            }
        }

        /// <summary>
        /// Assign provider to a service
        /// </summary>
        [HttpPost("{id}/assign-provider")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(ServiceDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ServiceDto>> AssignProvider(int id, AssignServiceProviderDto assignProviderDto)
        {
            try
            {
                var service = await _serviceService.AssignProviderAsync(id, assignProviderDto);
                if (service == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Service with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<ServiceDto>
                {
                    Success = true,
                    Message = "Provider assigned successfully",
                    Data = service
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
                _logger.LogError(ex, "Error assigning provider to service {ServiceId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while assigning the provider"
                });
            }
        }

        /// <summary>
        /// Remove provider from a service
        /// </summary>
        [HttpPost("{id}/remove-provider")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(ServiceDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ServiceDto>> RemoveProvider(int id)
        {
            try
            {
                var service = await _serviceService.RemoveProviderAsync(id);
                if (service == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Service with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<ServiceDto>
                {
                    Success = true,
                    Message = "Provider removed successfully",
                    Data = service
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing provider from service {ServiceId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while removing the provider"
                });
            }
        }

        /// <summary>
        /// Get service statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(ServiceStatsDto), 200)]
        public async Task<ActionResult<ServiceStatsDto>> GetStats()
        {
            try
            {
                var stats = await _serviceService.GetServiceStatsAsync();
                return Ok(new ApiResponse<ServiceStatsDto>
                {
                    Success = true,
                    Message = "Service statistics retrieved successfully",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service statistics");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving statistics"
                });
            }
        }

        /// <summary>
        /// Get services by provider
        /// </summary>
        [HttpGet("provider/{providerId}/{providerType}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ServiceDto>), 200)]
        public async Task<ActionResult<IEnumerable<ServiceDto>>> GetServicesByProvider(int providerId, string providerType)
        {
            try
            {
                var services = await _serviceService.GetServicesByProviderAsync(providerId, providerType);
                return Ok(new ApiResponse<IEnumerable<ServiceDto>>
                {
                    Success = true,
                    Message = $"Services for {providerType} {providerId} retrieved successfully",
                    Data = services
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving services for provider {ProviderType} {ProviderId}", providerType, providerId);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving services"
                });
            }
        }

        /// <summary>
        /// Get free services
        /// </summary>
        [HttpGet("free")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ServiceDto>), 200)]
        public async Task<ActionResult<IEnumerable<ServiceDto>>> GetFreeServices()
        {
            try
            {
                var services = await _serviceService.GetFreeServicesAsync();
                return Ok(new ApiResponse<IEnumerable<ServiceDto>>
                {
                    Success = true,
                    Message = "Free services retrieved successfully",
                    Data = services
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving free services");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving free services"
                });
            }
        }

        /// <summary>
        /// Get services by category
        /// </summary>
        [HttpGet("category/{category}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ServiceDto>), 200)]
        public async Task<ActionResult<IEnumerable<ServiceDto>>> GetServicesByCategory(string category)
        {
            try
            {
                var services = await _serviceService.GetServicesByCategoryAsync(category);
                return Ok(new ApiResponse<IEnumerable<ServiceDto>>
                {
                    Success = true,
                    Message = $"Services in category '{category}' retrieved successfully",
                    Data = services
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving services for category {Category}", category);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while retrieving services"
                });
            }
        }

        /// <summary>
        /// Toggle service status
        /// </summary>
        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ToggleServiceStatus(int id)
        {
            try
            {
                var result = await _serviceService.ToggleServiceStatusAsync(id);
                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Service with ID {id} not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Service status toggled successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling service status for {ServiceId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while toggling the service status"
                });
            }
        }
    }
}