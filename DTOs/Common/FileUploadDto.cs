using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace HealthAidAPI.DTOs.Common
{
    public class FileUploadDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;
        public string FolderName { get; set; } = "General"; 
    }
}