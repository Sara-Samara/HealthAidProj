using HealthAidAPI.DTOs.Extras;
using HealthAidAPI.Helpers;
using HealthAidAPI.Models;
using HealthAidAPI.Services;
using HealthAidAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BloodBankController : ControllerBase
    {
        private readonly IBloodBankService _service;

        public BloodBankController(IBloodBankService service)
        {
            _service = service;
        }

        [HttpPost("request")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<BloodRequestDto>>> CreateRequest(CreateBloodRequestDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _service.CreateRequestAsync(dto, userId);
            return Ok(new ApiResponse<BloodRequestDto> { Success = true, Message = "Blood request broadcasted", Data = result });
        }

        [HttpGet("active")]
        public async Task<ActionResult<ApiResponse<List<BloodRequestDto>>>> GetActiveRequests()
        {
            var result = await _service.GetActiveRequestsAsync();
            return Ok(new ApiResponse<List<BloodRequestDto>> { Success = true, Data = result });
        }
    }
}
