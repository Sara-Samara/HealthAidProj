// إذا كان NgoMissionDto في مجلد آخر، قم بإلغاء تعليق السطر التالي وتعديل المسار
// using HealthAidAPI.DTOs.NGOmission; 

using HealthAidAPI.DTOs.NgoMissions;

namespace HealthAidAPI.DTOs.NGOs
{
    public class NgoDetailDto
    {
        public int NGOId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
        public string AreaOfWork { get; set; } = string.Empty;
        public string VerifiedStatus { get; set; } = string.Empty;
        public string ContactedPerson { get; set; } = string.Empty;

        // تأكد من أن النظام يتعرف على NgoMissionDto
        public List<NgoMissionDto> Missions { get; set; } = new();

        // public List<EquipmentDto> Equipments { get; set; } = new();
        // public List<PatientDto> Patients { get; set; } = new();

        public DateTime? CreatedAt { get; set; }
    }
}