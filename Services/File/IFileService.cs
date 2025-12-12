using Microsoft.AspNetCore.Http;

namespace HealthAidAPI.Services.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile file, string folderName);
    }
}