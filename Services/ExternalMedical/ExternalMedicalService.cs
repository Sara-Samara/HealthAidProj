using HealthAidAPI.DTOs.External;
using HealthAidAPI.Services.Interfaces;
using Newtonsoft.Json; // تأكد أنك مثبت مكتبة Newtonsoft.Json أو استخدم System.Text.Json

namespace HealthAidAPI.Services.Implementations
{
    public class ExternalMedicalService : IExternalMedicalService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExternalMedicalService> _logger;

        public ExternalMedicalService(HttpClient httpClient, ILogger<ExternalMedicalService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<DrugInfoDto?> GetDrugInfoAsync(string drugName)
        {
            try
            {
                // OpenFDA API URL
                var url = $"https://api.fda.gov/drug/label.json?search=openfda.brand_name:\"{drugName}\"&limit=1";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Drug info not found for: {DrugName}", drugName);
                    return null;
                }

                var jsonString = await response.Content.ReadAsStringAsync();

                // تحويل الـ JSON المعقد إلى كائنات C#
                var fdaData = JsonConvert.DeserializeObject<OpenFdaResponse>(jsonString);

                if (fdaData?.Results == null || !fdaData.Results.Any())
                    return null;

                var result = fdaData.Results.First();

                // تبسيط البيانات وإرجاعها
                return new DrugInfoDto
                {
                    BrandName = result.Openfda.Brand_name?.FirstOrDefault() ?? "Unknown",
                    GenericName = result.Openfda.Generic_name?.FirstOrDefault() ?? "Unknown",
                    Purpose = result.Purpose?.FirstOrDefault() ?? "Not available",
                    Warnings = result.Warnings?.FirstOrDefault() ?? "Not available",
                    DosageAndAdministration = result.Dosage_and_administration?.FirstOrDefault() ?? "Not available"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching data from OpenFDA API");
                return null; // أو يمكنك رمي الخطأ حسب الحاجة
            }
        }
    }
}