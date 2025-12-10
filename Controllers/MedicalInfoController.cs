using HealthAidAPI.DTOs.External;
using HealthAidAPI.Helpers;
using HealthAidAPI.Models; // للـ ApiResponse
using HealthAidAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MedicalInfoController : ControllerBase
    {
        private readonly IExternalMedicalService _externalService;

        public MedicalInfoController(IExternalMedicalService externalService)
        {
            _externalService = externalService;
        }

        /// <summary>
        /// Search for official drug information using OpenFDA External API
        /// </summary>
        /// <param name="drugName">e.g. Panadol, Aspirin, Ibuprofen</param>
        [HttpGet("drug-search")]
        public async Task<ActionResult<ApiResponse<DrugInfoDto>>> GetDrugInfo([FromQuery] string drugName)
        {
            if (string.IsNullOrWhiteSpace(drugName))
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Drug name is required" });

            var result = await _externalService.GetDrugInfoAsync(drugName);

            if (result == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = "Drug information not found in external database" });

            return Ok(new ApiResponse<DrugInfoDto>
            {
                Success = true,
                Message = "Drug information retrieved successfully from OpenFDA",
                Data = result
            });
        }
    }
}