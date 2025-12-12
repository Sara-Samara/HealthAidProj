namespace HealthAidAPI.DTOs.Sync
{
    public class SyncResultDto
    {
        public int TotalSynced { get; set; }
        public int FailedCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}