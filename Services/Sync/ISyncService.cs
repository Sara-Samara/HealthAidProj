using HealthAidAPI.DTOs.Sync;

namespace HealthAidAPI.Services.Interfaces
{
    public interface ISyncService
    {
        Task<SyncResultDto> ProcessOfflineQueueAsync(int userId, List<SyncRequestDto> queueItems);
    }
}