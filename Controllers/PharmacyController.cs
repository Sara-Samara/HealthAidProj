using HealthAidAPI.DTOs.Extras;
using HealthAidAPI.Helpers;
using HealthAidAPI.Models;
using HealthAidAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PharmacyController : ControllerBase
    {
        private readonly IPharmacyService _service;

        public PharmacyController(IPharmacyService service)
        {
            _service = service;
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<List<MedicineSearchDto>>>> SearchMedicine([FromQuery] string name)
        {
            var result = await _service.SearchMedicineAsync(name);
            return Ok(new ApiResponse<List<MedicineSearchDto>> { Success = true, Data = result });
        }

        [HttpPost("update-stock")]
        [Authorize(Roles = "Admin,Doctor")] 
        public async Task<ActionResult<ApiResponse<string>>> UpdateStock(UpdateStockDto dto)
        {
            await _service.UpdateStockAsync(dto);
            return Ok(new ApiResponse<string> { Success = true, Message = "Stock updated successfully" });
        }
    }
}