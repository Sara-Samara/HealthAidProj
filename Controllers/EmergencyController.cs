using HealthAidAPI.Data;
using HealthAidAPI.Models;
using HealthAidAPI.Models.Emergency;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmergencyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmergencyController> _logger;

        public EmergencyController(ApplicationDbContext context, ILogger<EmergencyController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/emergency
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<EmergencyCase>>>> GetEmergencyCases([FromQuery] string? status = null)
        {
            try
            {
                var query = _context.EmergencyCases.AsQueryable();

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(ec => ec.Status == status);

                var cases = await query
                    .Include(ec => ec.Patient)
                        .ThenInclude(p => p.User)
                    .Include(ec => ec.Responder)
                        .ThenInclude(r => r!.User)
                    .Include(ec => ec.EmergencyLogs)
                    .OrderByDescending(ec => ec.CreatedAt)
                    .ToListAsync();

                return Ok(new ApiResponse<List<EmergencyCase>>
                {
                    Success = true,
                    Message = "Emergency cases retrieved successfully",
                    Data = cases
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving emergency cases");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // GET: api/emergency/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<EmergencyCase>>> GetEmergencyCase(int id)
        {
            try
            {
                var emergencyCase = await _context.EmergencyCases
                    .Include(ec => ec.Patient)
                        .ThenInclude(p => p.User)
                    .Include(ec => ec.Responder)
                        .ThenInclude(r => r!.User)
                    .Include(ec => ec.EmergencyLogs)
                    .FirstOrDefaultAsync(ec => ec.Id == id);

                if (emergencyCase == null)
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Emergency case not found"
                    });

                return Ok(new ApiResponse<EmergencyCase>
                {
                    Success = true,
                    Message = "Emergency case retrieved successfully",
                    Data = emergencyCase
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving emergency case {EmergencyId}", id);
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // POST: api/emergency/alert
        [HttpPost("alert")]
        public async Task<ActionResult<ApiResponse<EmergencyCase>>> CreateEmergencyAlert(EmergencyCaseRequest request)
        {
            try
            {
                var emergencyCase = new EmergencyCase
                {
                    PatientId = request.PatientId,
                    EmergencyType = request.EmergencyType,
                    Priority = request.Priority,
                    Location = request.Location,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Description = request.Description,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow
                };

                _context.EmergencyCases.Add(emergencyCase);
                await _context.SaveChangesAsync();

                // Add log
                var log = new EmergencyLog
                {
                    EmergencyCaseId = emergencyCase.Id,
                    Action = "Emergency_Created",
                    PerformedBy = request.PatientId,
                    Notes = $"Emergency alert created for {request.EmergencyType}",
                    CreatedAt = DateTime.UtcNow
                };
                _context.EmergencyLogs.Add(log);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetEmergencyCase), new { id = emergencyCase.Id },
                    new ApiResponse<EmergencyCase>
                    {
                        Success = true,
                        Message = "Emergency alert created successfully",
                        Data = emergencyCase
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating emergency alert");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // PUT: api/emergency/5/assign-responder
        [HttpPut("{id}/assign-responder")]
        public async Task<ActionResult<ApiResponse<EmergencyCase>>> AssignResponder(int id, AssignResponderRequest request)
        {
            try
            {
                var emergencyCase = await _context.EmergencyCases.FindAsync(id);
                if (emergencyCase == null)
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Emergency case not found"
                    });

                emergencyCase.ResponderId = request.ResponderId;
                emergencyCase.Status = "Assigned";

                // Add log
                var log = new EmergencyLog
                {
                    EmergencyCaseId = emergencyCase.Id,
                    Action = "Responder_Assigned",
                    PerformedBy = request.AssignedByUserId,
                    Notes = $"Responder {request.ResponderId} assigned",
                    CreatedAt = DateTime.UtcNow
                };
                _context.EmergencyLogs.Add(log);

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<EmergencyCase>
                {
                    Success = true,
                    Message = "Responder assigned successfully",
                    Data = emergencyCase
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning responder to emergency {EmergencyId}", id);
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // GET: api/emergency/nearby-responders
        [HttpGet("nearby-responders")]
        public async Task<ActionResult<ApiResponse<List<EmergencyResponder>>>> GetNearbyResponders(
            [FromQuery] decimal latitude,
            [FromQuery] decimal longitude,
            [FromQuery] decimal radius = 10.00m)
        {
            try
            {
                var responders = await _context.EmergencyResponders
                    .Where(r => r.IsAvailable)
                    .Include(r => r.User)
                    .ToListAsync();

                return Ok(new ApiResponse<List<EmergencyResponder>>
                {
                    Success = true,
                    Message = "Nearby responders retrieved successfully",
                    Data = responders
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving nearby responders");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // POST: api/emergency/responders
        [HttpPost("responders")]
        public async Task<ActionResult<ApiResponse<EmergencyResponder>>> CreateEmergencyResponder(EmergencyResponderRequest request)
        {
            try
            {
                var responder = new EmergencyResponder
                {
                    UserId = request.UserId,
                    Type = request.Type,
                    Specialization = request.Specialization,
                    Location = request.Location,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    ContactNumber = request.ContactNumber,
                    Qualifications = request.Qualifications,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.EmergencyResponders.Add(responder);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetEmergencyResponders), new { id = responder.Id },
                    new ApiResponse<EmergencyResponder>
                    {
                        Success = true,
                        Message = "Emergency responder created successfully",
                        Data = responder
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating emergency responder");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // GET: api/emergency/responders
        [HttpGet("responders")]
        public async Task<ActionResult<ApiResponse<List<EmergencyResponder>>>> GetEmergencyResponders()
        {
            try
            {
                var responders = await _context.EmergencyResponders
                    .Include(r => r.User)
                    .ToListAsync();

                return Ok(new ApiResponse<List<EmergencyResponder>>
                {
                    Success = true,
                    Message = "Emergency responders retrieved successfully",
                    Data = responders
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving emergency responders");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }
    }

    public class EmergencyCaseRequest
    {
        public int PatientId { get; set; }
        public required string EmergencyType { get; set; }
        public string Priority { get; set; } = "Medium";
        public string Location { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class AssignResponderRequest
    {
        public int ResponderId { get; set; }
        public int AssignedByUserId { get; set; }
    }

    public class EmergencyResponderRequest
    {
        public int UserId { get; set; }
        public required string Type { get; set; }
        public string Specialization { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string ContactNumber { get; set; } = string.Empty;
        public string Qualifications { get; set; } = string.Empty;
    }
}