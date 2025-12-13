using HealthAidAPI.Helpers;
using HealthAidAPI.Models;
using HealthAidAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;

        public AiController(IAiService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("check-symptoms")]
        public async Task<ActionResult<ApiResponse<string>>> CheckSymptoms([FromBody] string symptoms)
        {
            var advice = await _aiService.AnalyzeSymptomsAsync(symptoms);
            return Ok(new ApiResponse<string> { Success = true, Data = advice });
        }
    }
}