using HealthAidAPI.DTOs.Common;
using HealthAidAPI.Helpers;
using HealthAidAPI.Models;
using HealthAidAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")] 
        public async Task<ActionResult<ApiResponse<string>>> UploadFile([FromForm] FileUploadDto dto)
        {
            try
            {
                var fileUrl = await _fileService.UploadFileAsync(dto.File, dto.FolderName);
                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "File uploaded successfully",
                    Data = fileUrl
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }
    }
}