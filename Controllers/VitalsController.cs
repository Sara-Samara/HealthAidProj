using HealthAidAPI.DTOs.Extras;
using HealthAidAPI.Helpers;
using HealthAidAPI.Models;
using HealthAidAPI.Models.Extras;
using HealthAidAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VitalsController : ControllerBase
    {
        private readonly IVitalsService _service;

        public VitalsController(IVitalsService service)
        {
            _service = service;
        }

        [HttpPost("sync")]
        public async Task<ActionResult<ApiResponse<PatientVital>>> SyncVitals(LogVitalDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _service.LogVitalsAsync(dto, userId);

            if (result.IsCritical)
            {
                return Ok(new ApiResponse<PatientVital> { Success = true, Message = "WARNING: Critical Vitals Detected!", Data = result });
            }

            return Ok(new ApiResponse<PatientVital> { Success = true, Message = "Vitals synced", Data = result });
        }
    }
}