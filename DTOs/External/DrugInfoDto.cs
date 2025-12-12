namespace HealthAidAPI.DTOs.External
{
    // هذا الكلاس يمثل شكل البيانات التي نريد عرضها للمستخدم
    public class DrugInfoDto
    {
        public string GenericName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string Warnings { get; set; } = string.Empty;
        public string DosageAndAdministration { get; set; } = string.Empty;
    }

    // هذه الكلاسات المساعدة لفك تشفير رد الـ OpenFDA المعقد
    public class OpenFdaResponse
    {
        public List<OpenFdaResult> Results { get; set; } = new();
    }

    public class OpenFdaResult
    {
        public OpenFdaDetails Openfda { get; set; } = new();
        public List<string> Purpose { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<string> Dosage_and_administration { get; set; } = new();
    }

    public class OpenFdaDetails
    {
        public List<string> Generic_name { get; set; } = new();
        public List<string> Brand_name { get; set; } = new();
    }
}