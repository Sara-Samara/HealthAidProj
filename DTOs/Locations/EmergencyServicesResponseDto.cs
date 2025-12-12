using HealthAidAPI.DTOs.MedicalFacilities; 
using HealthAidAPI.DTOs.Emergency; 

namespace HealthAidAPI.DTOs.Locations
{
    public class EmergencyServicesResponseDto
    {
        public List<MedicalFacilityDto> Hospitals { get; set; } = new();
        public List<EmergencyResponderDto> EmergencyResponders { get; set; } = new();
    }
}