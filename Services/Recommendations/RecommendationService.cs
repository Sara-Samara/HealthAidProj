using Microsoft.EntityFrameworkCore;
using HealthAidAPI.Data;
using HealthAidAPI.Models.Recommendations;
using HealthAidAPI.Models;
using HealthAidAPI.DTOs.Recommendations;
using HealthAidAPI.Services.Interfaces;

namespace HealthAidAPI.Services.Implementations
{
    public class RecommendationService : IRecommendationService
    {
        private readonly ApplicationDbContext _context;

        public RecommendationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DoctorRecommendationDto>> GenerateDoctorRecommendationsAsync(int patientId)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (patient == null) throw new KeyNotFoundException("Patient not found");

            var availableDoctors = await _context.Doctors
                .Include(d => d.User)
                .ToListAsync();

            // منطق التوصية
            var newRecommendations = new List<DoctorRecommendation>();

            foreach (var doctor in availableDoctors)
            {
                // حساب التقييم المتوسط للطبيب
                var averageRating = await _context.Ratings
                    .Where(r => r.TargetType == "Doctor" && r.TargetId == doctor.DoctorId)
                    .Select(r => (double?)r.Value)
                    .AverageAsync() ?? 0;

                var rec = new DoctorRecommendation
                {
                    PatientId = patient.PatientId,
                    DoctorId = doctor.DoctorId,
                    RecommendationType = "Consultation",
                    Reason = $"Doctor specializes in {doctor.Specialization} with rating {averageRating:F1}",
                    MatchScore = CalculateMatchScore(patient, doctor),
                    Priority = "Medium",
                    IsViewed = false,
                    CreatedAt = DateTime.UtcNow
                };
                newRecommendations.Add(rec);
            }

            // ترتيب وأخذ أفضل 5
            newRecommendations = newRecommendations.OrderByDescending(r => r.MatchScore).Take(5).ToList();

            // حفظ في قاعدة البيانات
            _context.DoctorRecommendations.AddRange(newRecommendations);
            await _context.SaveChangesAsync();

            // إرجاع DTOs
            return newRecommendations.Select(MapToRecommendationDto).ToList();
        }

        public async Task<List<DoctorRecommendationDto>> GetStoredRecommendationsAsync(int patientId)
        {
            var recommendations = await _context.DoctorRecommendations
                .Where(dr => dr.PatientId == patientId && !dr.IsViewed)
                .Include(dr => dr.Doctor).ThenInclude(d => d.User)
                .OrderByDescending(dr => dr.MatchScore)
                .ToListAsync();

            return recommendations.Select(MapToRecommendationDto).ToList();
        }

        public async Task<PatientHealthProfileDto> CreateHealthProfileAsync(CreateHealthProfileDto dto)
        {
            var profile = new PatientHealthProfile
            {
                PatientId = dto.PatientId,
                BloodType = dto.BloodType,
                Height = dto.Height,
                Weight = dto.Weight,
                ChronicDiseases = dto.ChronicDiseases,
                Allergies = dto.Allergies,
                Medications = dto.Medications,
                FamilyMedicalHistory = dto.FamilyMedicalHistory,
                Lifestyle = dto.Lifestyle,
                Smoking = dto.Smoking,
                Alcohol = dto.Alcohol,
                LastCheckup = dto.LastCheckup,
                EmergencyContactName = dto.EmergencyContactName,
                EmergencyContactNumber = dto.EmergencyContactNumber,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.PatientHealthProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return MapToProfileDto(profile);
        }

        public async Task<PatientHealthProfileDto?> GetHealthProfileAsync(int patientId)
        {
            var profile = await _context.PatientHealthProfiles
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            return profile == null ? null : MapToProfileDto(profile);
        }

        public async Task<PatientHealthProfileDto?> UpdateHealthProfileAsync(int patientId, UpdateHealthProfileDto dto)
        {
            var profile = await _context.PatientHealthProfiles
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (profile == null) return null;

            // تحديث الحقول
            if (!string.IsNullOrEmpty(dto.BloodType)) profile.BloodType = dto.BloodType;
            if (dto.Height.HasValue) profile.Height = dto.Height.Value;
            if (dto.Weight.HasValue) profile.Weight = dto.Weight.Value;
            if (dto.ChronicDiseases != null) profile.ChronicDiseases = dto.ChronicDiseases;
            if (dto.Allergies != null) profile.Allergies = dto.Allergies;
            if (dto.Medications != null) profile.Medications = dto.Medications;
            if (dto.FamilyMedicalHistory != null) profile.FamilyMedicalHistory = dto.FamilyMedicalHistory;
            if (dto.Lifestyle != null) profile.Lifestyle = dto.Lifestyle;
            if (dto.Smoking.HasValue) profile.Smoking = dto.Smoking.Value;
            if (dto.Alcohol.HasValue) profile.Alcohol = dto.Alcohol.Value;
            if (dto.LastCheckup.HasValue) profile.LastCheckup = dto.LastCheckup.Value;
            if (dto.EmergencyContactName != null) profile.EmergencyContactName = dto.EmergencyContactName;
            if (dto.EmergencyContactNumber != null) profile.EmergencyContactNumber = dto.EmergencyContactNumber;

            profile.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToProfileDto(profile);
        }

        // ================= Helpers =================

        private decimal CalculateMatchScore(Patient patient, Doctor doctor)
        {
            // خوارزمية بسيطة (يمكن تطويرها)
            decimal score = 0.5m;

            // مثال: زيادة النقاط إذا كان الطبيب لديه تقييم عالي
            // أو إذا كان تخصص الطبيب يطابق أمراض المريض المزمنة

            return Math.Min(score, 1.0m);
        }

        private static DoctorRecommendationDto MapToRecommendationDto(DoctorRecommendation entity)
        {
            return new DoctorRecommendationDto
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                DoctorId = entity.DoctorId,
                DoctorName = entity.Doctor?.User != null ? $"{entity.Doctor.User.FirstName} {entity.Doctor.User.LastName}" : "Unknown",
                Specialization = entity.Doctor?.Specialization ?? "",
                RecommendationType = entity.RecommendationType,
                Reason = entity.Reason,
                MatchScore = entity.MatchScore,
                Priority = entity.Priority,
                IsViewed = entity.IsViewed,
                CreatedAt = entity.CreatedAt
            };
        }

        private static PatientHealthProfileDto MapToProfileDto(PatientHealthProfile entity)
        {
            return new PatientHealthProfileDto
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                BloodType = entity.BloodType,
                Height = entity.Height,
                Weight = entity.Weight,
                ChronicDiseases = entity.ChronicDiseases,
                Allergies = entity.Allergies,
                Medications = entity.Medications,
                FamilyMedicalHistory = entity.FamilyMedicalHistory,
                Lifestyle = entity.Lifestyle,
                Smoking = entity.Smoking,
                Alcohol = entity.Alcohol,
                LastCheckup = entity.LastCheckup,
                EmergencyContactName = entity.EmergencyContactName,
                EmergencyContactNumber = entity.EmergencyContactNumber,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}