using HealthAidAPI.Data;
using HealthAidAPI.Models.Extras;
using HealthAidAPI.DTOs.Extras;
using HealthAidAPI.Services.Interfaces;

namespace HealthAidAPI.Services.Implementations
{
    public class VitalsService : IVitalsService
    {
        private readonly ApplicationDbContext _context;

        public VitalsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PatientVital> LogVitalsAsync(LogVitalDto dto, int patientId)
        {
            var vital = new PatientVital
            {
                PatientId = patientId,
                HeartRate = dto.HeartRate,
                OxygenLevel = dto.OxygenLevel,
                BodyTemperature = dto.BodyTemperature,
                DeviceId = dto.DeviceId,
                IsCritical = dto.HeartRate > 120 || dto.OxygenLevel < 90 || dto.BodyTemperature > 39
            };

            _context.PatientVitals.Add(vital);
            await _context.SaveChangesAsync();
            return vital;
        }
    }
}