namespace HealthAidAPI.Services.Interfaces
{
    public interface IAiService
    {
        Task<string> AnalyzeSymptomsAsync(string symptoms);
    }
}