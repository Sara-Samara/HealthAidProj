using HealthAidAPI.Services.Interfaces;

namespace HealthAidAPI.Services.Implementations
{
    public class AiService : IAiService
    {
        public async Task<string> AnalyzeSymptomsAsync(string symptoms)
        {
            await Task.Delay(1000);

            var input = symptoms.ToLower();

            if (input.Contains("headache") || input.Contains("fever"))
                return "Based on your symptoms (Headache/Fever), you might be experiencing a viral infection or flu. Recommendation: Rest, stay hydrated, and consult a General Practitioner if temperature exceeds 39°C.";

            if (input.Contains("chest pain") || input.Contains("heart"))
                return "CRITICAL ALERT: Chest pain can be serious. Please trigger an EMERGENCY ALERT immediately using the SOS feature in the app.";

            if (input.Contains("stomach") || input.Contains("pain"))
                return "Abdominal pain can have various causes. Avoid heavy meals. Recommended Specialist: Gastroenterologist.";

            return "Your symptoms have been noted. It is recommended to book a consultation with a General Practitioner for a thorough checkup.";
        }
    }
}