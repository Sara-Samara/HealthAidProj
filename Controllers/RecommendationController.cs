using HealthAidAPI.Data;
using HealthAidAPI.Models;
using HealthAidAPI.Models.Recommendations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RecommendationController> _logger;

        public RecommendationController(ApplicationDbContext context, ILogger<RecommendationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/recommendation/doctors/5
        [HttpGet("doctors/{patientId}")]
        public async Task<ActionResult<ApiResponse<List<DoctorRecommendation>>>> GetDoctorRecommendations(int patientId)
        {
            try
            {
                var patient = await _context.Patients
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.PatientId == patientId);

                if (patient == null)
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Patient not found"
                    });

                var availableDoctors = await _context.Doctors
                    .Include(d => d.User)
                    .ToListAsync();

                var recommendations = availableDoctors
                    .Select(doctor =>
                    {

                        var averageRating = _context.Ratings
                            .Where(r => r.TargetType == "Doctor" && r.TargetId == doctor.DoctorId)
                            .Select(r => (double?)r.Value)
                            .Average() ?? 0;

                        return new DoctorRecommendation
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
                    })
                    .OrderByDescending(r => r.MatchScore)
                    .Take(5)
                    .ToList();

                // Save recommendations to database
                _context.DoctorRecommendations.AddRange(recommendations);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<List<DoctorRecommendation>>
                {
                    Success = true,
                    Message = "Doctor recommendations retrieved successfully",
                    Data = recommendations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating doctor recommendations for patient {PatientId}", patientId);
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // POST: api/recommendation/health-profile
        [HttpPost("health-profile")]
        public async Task<ActionResult<ApiResponse<PatientHealthProfile>>> CreateHealthProfile(PatientHealthProfileRequest request)
        {
            try
            {
                var profile = new PatientHealthProfile
                {
                    PatientId = request.PatientId,
                    BloodType = request.BloodType,
                    Height = request.Height,
                    Weight = request.Weight,
                    ChronicDiseases = request.ChronicDiseases,
                    Allergies = request.Allergies,
                    Medications = request.Medications,
                    FamilyMedicalHistory = request.FamilyMedicalHistory,
                    Lifestyle = request.Lifestyle,
                    Smoking = request.Smoking,
                    Alcohol = request.Alcohol,
                    LastCheckup = request.LastCheckup,
                    EmergencyContactName = request.EmergencyContactName,
                    EmergencyContactNumber = request.EmergencyContactNumber,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.PatientHealthProfiles.Add(profile);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetHealthProfile), new { patientId = request.PatientId },
                    new ApiResponse<PatientHealthProfile>
                    {
                        Success = true,
                        Message = "Health profile created successfully",
                        Data = profile
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating health profile");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // GET: api/recommendation/health-profile/5
        [HttpGet("health-profile/{patientId}")]
        public async Task<ActionResult<ApiResponse<PatientHealthProfile>>> GetHealthProfile(int patientId)
        {
            try
            {
                var profile = await _context.PatientHealthProfiles
                    .FirstOrDefaultAsync(p => p.PatientId == patientId);

                if (profile == null)
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Health profile not found"
                    });

                return Ok(new ApiResponse<PatientHealthProfile>
                {
                    Success = true,
                    Message = "Health profile retrieved successfully",
                    Data = profile
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving health profile for patient {PatientId}", patientId);
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // PUT: api/recommendation/health-profile/5
        [HttpPut("health-profile/{patientId}")]
        public async Task<ActionResult<ApiResponse<PatientHealthProfile>>> UpdateHealthProfile(int patientId, PatientHealthProfileRequest request)
        {
            try
            {
                var profile = await _context.PatientHealthProfiles
                    .FirstOrDefaultAsync(p => p.PatientId == patientId);

                if (profile == null)
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Health profile not found"
                    });

                profile.BloodType = request.BloodType;
                profile.Height = request.Height;
                profile.Weight = request.Weight;
                profile.ChronicDiseases = request.ChronicDiseases;
                profile.Allergies = request.Allergies;
                profile.Medications = request.Medications;
                profile.FamilyMedicalHistory = request.FamilyMedicalHistory;
                profile.Lifestyle = request.Lifestyle;
                profile.Smoking = request.Smoking;
                profile.Alcohol = request.Alcohol;
                profile.LastCheckup = request.LastCheckup;
                profile.EmergencyContactName = request.EmergencyContactName;
                profile.EmergencyContactNumber = request.EmergencyContactNumber;
                profile.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<PatientHealthProfile>
                {
                    Success = true,
                    Message = "Health profile updated successfully",
                    Data = profile
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating health profile for patient {PatientId}", patientId);
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // GET: api/recommendation/recommendations/5
        [HttpGet("recommendations/{patientId}")]
        public async Task<ActionResult<ApiResponse<List<DoctorRecommendation>>>> GetPatientRecommendations(int patientId)
        {
            try
            {
                var recommendations = await _context.DoctorRecommendations
                    .Where(dr => dr.PatientId == patientId && !dr.IsViewed)
                    .Include(dr => dr.Doctor)
                        .ThenInclude(d => d.User)
                    .OrderByDescending(dr => dr.MatchScore)
                    .ToListAsync();

                return Ok(new ApiResponse<List<DoctorRecommendation>>
                {
                    Success = true,
                    Message = "Patient recommendations retrieved successfully",
                    Data = recommendations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recommendations for patient {PatientId}", patientId);
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        private static decimal CalculateMatchScore(Patient patient, Doctor doctor)
        {
            // Simplified matching algorithm
            decimal score = 0.7m;
            // Add more complex logic here based on patient needs and doctor specialization
            return Math.Min(score, 1.0m);
        }
    }

    public class PatientHealthProfileRequest
    {
        public int PatientId { get; set; }
        public string BloodType { get; set; } = string.Empty;
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string ChronicDiseases { get; set; } = string.Empty;
        public string Allergies { get; set; } = string.Empty;
        public string Medications { get; set; } = string.Empty;
        public string FamilyMedicalHistory { get; set; } = string.Empty;
        public string Lifestyle { get; set; } = string.Empty;
        public bool Smoking { get; set; }
        public bool Alcohol { get; set; }
        public DateTime? LastCheckup { get; set; }
        public string EmergencyContactName { get; set; } = string.Empty;
        public string EmergencyContactNumber { get; set; } = string.Empty;
    }
}