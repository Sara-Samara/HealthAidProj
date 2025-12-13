using HealthAidAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace HealthAidAPI.Services.Implementations
{
    public class AiService : IAiService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AiService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> AnalyzeSymptomsAsync(string symptoms)
        {
            await Task.Delay(1000);

            bool isArabicRequest = IsArabicRequest(symptoms);

            var input = symptoms.ToLower();

            if (isArabicRequest)
            {
                if (input.Contains("صداع") || input.Contains("حرار"))
                    return "بناءً على أعراضك (صداع/حرارة)، قد تكون مصاباً بعدوى فيروسية. النصيحة: الراحة، شرب السوائل، واستشر طبيباً إذا استمرت الحرارة.";

                if (input.Contains("صدر") || input.Contains("قلب") || input.Contains("تنفس"))
                    return "⚠️ تنبيه: ألم الصدر حالة خطيرة. استخدم ميزة الطوارئ فوراً.";

                if (input.Contains("بطن") || input.Contains("معدة"))
                    return "آلام البطن متعددة الأسباب. تجنب الأطعمة الثقيلة. يُنصح بزيارة طبيب جهاز هضمي.";

                return "تم تسجيل أعراضك. ننصحك بزيارة طبيب عام للفحص.";
            }

            if (input.Contains("headache") || input.Contains("fever"))
                return "Based on your symptoms (Headache/Fever), you might be experiencing a viral infection or flu.";

            if (input.Contains("chest pain") || input.Contains("heart"))
                return "CRITICAL ALERT: Chest pain can be serious. Please trigger an EMERGENCY ALERT.";

            if (input.Contains("stomach") || input.Contains("pain"))
                return "Abdominal pain can have various causes. Recommended Specialist: Gastroenterologist.";

            return "Your symptoms have been noted. Consult a General Practitioner.";
        }

        private bool IsArabicRequest(string text)
        {
            var acceptLanguage = _httpContextAccessor.HttpContext?.Request.Headers["Accept-Language"].ToString();
            if (!string.IsNullOrEmpty(acceptLanguage) && acceptLanguage.Contains("ar"))
                return true;


            foreach (char c in text)
            {
                if (c >= 'آ' && c <= 'ي')
                    return true;
                if (c >= 'ا' && c <= 'ى')
                    return true;
            }

            return false;
        }
    }
}