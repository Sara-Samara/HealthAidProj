using AutoMapper;
using HealthAidAPI.Data;
using HealthAidAPI.DTOs.Patients;
using HealthAidAPI.Models;
using HealthAidAPI.DTOs.Users;
using HealthAidAPI.Helpers;
using HealthAidAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthAidAPI.Services.Implementations
{
    public class PatientService : IPatientService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PatientService> _logger;

        public PatientService(ApplicationDbContext context, IMapper mapper, ILogger<PatientService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<PatientDto>> GetPatientsAsync(PatientFilterDto filter)
        {
            try
            {
                var query = _context.Patients
                    .Include(p => p.User)
                    .Include(p => p.NGO)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(filter.Search))
                {
                    query = query.Where(p =>
                        p.PatientName.Contains(filter.Search) ||
                        p.MedicalHistory != null && p.MedicalHistory.Contains(filter.Search) ||
                        p.User.FirstName.Contains(filter.Search) ||
                        p.User.LastName.Contains(filter.Search));
                }

                if (!string.IsNullOrEmpty(filter.Gender))
                    query = query.Where(p => p.Gender == filter.Gender);

                if (!string.IsNullOrEmpty(filter.BloodType))
                    query = query.Where(p => p.BloodType == filter.BloodType);

                if (filter.NGOId.HasValue) // تم التصحيح من NgosId إلى NGOId
                    query = query.Where(p => p.NGOId == filter.NGOId.Value);

                if (filter.MinAge.HasValue)
                {
                    var minDate = DateTime.UtcNow.AddYears(-filter.MinAge.Value);
                    query = query.Where(p => p.DateOfBirth <= minDate);
                }

                if (filter.MaxAge.HasValue)
                {
                    var maxDate = DateTime.UtcNow.AddYears(-filter.MaxAge.Value);
                    query = query.Where(p => p.DateOfBirth >= maxDate);
                }

                if (filter.HasMedicalHistory.HasValue)
                {
                    query = filter.HasMedicalHistory.Value ?
                        query.Where(p => !string.IsNullOrEmpty(p.MedicalHistory)) :
                        query.Where(p => string.IsNullOrEmpty(p.MedicalHistory));
                }

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "name" => filter.SortDesc ?
                        query.OrderByDescending(p => p.PatientName) :
                        query.OrderBy(p => p.PatientName),
                    "age" => filter.SortDesc ?
                        query.OrderByDescending(p => p.DateOfBirth) :
                        query.OrderBy(p => p.DateOfBirth),
                    "gender" => filter.SortDesc ?
                        query.OrderByDescending(p => p.Gender) :
                        query.OrderBy(p => p.Gender),
                    "bloodtype" => filter.SortDesc ?
                        query.OrderByDescending(p => p.BloodType) :
                        query.OrderBy(p => p.BloodType),
                    _ => filter.SortDesc ?
                        query.OrderByDescending(p => p.PatientId) :
                        query.OrderBy(p => p.PatientId)
                };

                var totalCount = await query.CountAsync();

                var patients = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(p => new PatientDto
                    {
                        PatientId = p.PatientId,
                        PatientName = p.PatientName,
                        MedicalHistory = p.MedicalHistory,
                        DateOfBirth = p.DateOfBirth,
                        Gender = p.Gender,
                        BloodType = p.BloodType,
                        UserId = p.UserId,
                        NGOId = p.NGOId, // تم التصحيح من NgosId إلى NGOId
                        User = _mapper.Map<UserDto>(p.User),
                        NGO = p.NGO != null ? _mapper.Map<NGODto>(p.NGO) : null,
                        TotalConsultations = p.Consultations.Count,
                        TotalAppointments = p.Appointments.Count,
                        TotalMedicineRequests = p.MedicineRequests.Count,
                        Age = p.DateOfBirth.HasValue ?
                            DateTime.UtcNow.Year - p.DateOfBirth.Value.Year : null,
                        IsActive = true
                    })
                    .ToListAsync();

                return new PagedResult<PatientDto>(patients, totalCount, filter.Page, filter.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patients with filter {@Filter}", filter);
                throw;
            }
        }

        public async Task<PatientDto?> GetPatientByIdAsync(int id)
        {
            try
            {
                var patient = await _context.Patients
                    .Include(p => p.User)
                    .Include(p => p.NGO)
                    .Include(p => p.Consultations)
                    .Include(p => p.Appointments)
                    .Include(p => p.MedicineRequests)
                    .FirstOrDefaultAsync(p => p.PatientId == id);

                if (patient == null)
                {
                    _logger.LogWarning("Patient with ID {PatientId} not found", id);
                    return null;
                }

                var patientDto = _mapper.Map<PatientDto>(patient);

                // Add additional statistics
                patientDto.TotalConsultations = patient.Consultations.Count;
                patientDto.TotalAppointments = patient.Appointments.Count;
                patientDto.TotalMedicineRequests = patient.MedicineRequests.Count;
                patientDto.Age = patient.DateOfBirth.HasValue ?
                    DateTime.UtcNow.Year - patient.DateOfBirth.Value.Year : null;

                return patientDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient by ID: {PatientId}", id);
                throw;
            }
        }

        public async Task<PatientDto?> GetPatientByUserIdAsync(int userId)
        {
            try
            {
                var patient = await _context.Patients
                    .Include(p => p.User)
                    .Include(p => p.NGO)
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (patient == null)
                {
                    _logger.LogWarning("Patient with User ID {UserId} not found", userId);
                    return null;
                }

                return _mapper.Map<PatientDto>(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient by User ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<PatientDto> CreatePatientAsync(CreatePatientDto patientDto)
        {
            try
            {
                // Validate user exists
                var user = await _context.Users.FindAsync(patientDto.UserId);
                if (user == null)
                    throw new ArgumentException("User not found");

                // Update user role to Patient if not already
                if (user.Role != "Patient")
                {
                    user.Role = "Patient";
                    _context.Users.Update(user);
                }

                // Validate NGO exists if provided - تم التصحيح هنا
                if (patientDto.NGOId.HasValue)
                {
                    var ngoExists = await _context.NGOs.AnyAsync(n => n.NGOId == patientDto.NGOId.Value);
                    if (!ngoExists)
                        throw new ArgumentException("NGO not found");
                }

                var patient = new Patient
                {
                    PatientName = patientDto.PatientName.Trim(),
                    MedicalHistory = patientDto.MedicalHistory?.Trim(),
                    DateOfBirth = patientDto.DateOfBirth,
                    Gender = patientDto.Gender,
                    BloodType = patientDto.BloodType,
                    UserId = patientDto.UserId,
                    NGOId = patientDto.NGOId // تم التصحيح من NgosId إلى NGOId
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Patient {PatientId} created successfully for user {UserId}",
                    patient.PatientId, patientDto.UserId);

                return await GetPatientByIdAsync(patient.PatientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating patient with data {@PatientDto}", patientDto);
                throw;
            }
        }

        public async Task<PatientDto?> UpdatePatientAsync(int id, UpdatePatientDto patientDto)
        {
            try
            {
                var patient = await _context.Patients
                    .Include(p => p.User)
                    .Include(p => p.NGO)
                    .FirstOrDefaultAsync(p => p.PatientId == id);

                if (patient == null)
                {
                    _logger.LogWarning("Patient with ID {PatientId} not found for update", id);
                    return null;
                }

                // Update only provided fields
                if (!string.IsNullOrEmpty(patientDto.PatientName))
                    patient.PatientName = patientDto.PatientName.Trim();

                if (patientDto.MedicalHistory != null)
                    patient.MedicalHistory = patientDto.MedicalHistory.Trim();

                if (patientDto.DateOfBirth.HasValue)
                    patient.DateOfBirth = patientDto.DateOfBirth;

                if (!string.IsNullOrEmpty(patientDto.Gender))
                    patient.Gender = patientDto.Gender;

                if (!string.IsNullOrEmpty(patientDto.BloodType))
                    patient.BloodType = patientDto.BloodType;

                if (patientDto.NGOId.HasValue) // تم التصحيح من NgosId إلى NGOId
                {
                    // Validate NGO exists - تم التصحيح هنا
                    var ngoExists = await _context.NGOs.AnyAsync(n => n.NGOId == patientDto.NGOId.Value);
                    if (!ngoExists)
                        throw new ArgumentException("NGO not found");

                    patient.NGOId = patientDto.NGOId.Value; // تم التصحيح من NgosId إلى NGOId
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Patient {PatientId} updated successfully", id);
                return await GetPatientByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating patient with ID: {PatientId} and data: {@PatientDto}", id, patientDto);
                throw;
            }
        }

        public async Task<bool> DeletePatientAsync(int id)
        {
            try
            {
                var patient = await _context.Patients
                    .Include(p => p.Consultations)
                    .Include(p => p.Appointments)
                    .Include(p => p.MedicineRequests)
                    .FirstOrDefaultAsync(p => p.PatientId == id);

                if (patient == null)
                {
                    _logger.LogWarning("Patient with ID {PatientId} not found for deletion", id);
                    return false;
                }

                // Check for related data
                if (patient.Consultations.Any() || patient.Appointments.Any() || patient.MedicineRequests.Any())
                {
                    _logger.LogWarning("Cannot delete patient {PatientId} with related data", id);
                    throw new InvalidOperationException(
                        "Cannot delete patient with related consultations, appointments, or medicine requests. " +
                        "Please delete related records first.");
                }

                // Update user role if this is their only patient profile
                var user = await _context.Users.FindAsync(patient.UserId);
                if (user != null && user.Role == "Patient")
                {
                    var otherPatientProfiles = await _context.Patients
                        .CountAsync(p => p.UserId == patient.UserId && p.PatientId != id);

                    if (otherPatientProfiles == 0)
                    {
                        user.Role = "User"; // Default role
                        _context.Users.Update(user);
                    }
                }

                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Patient {PatientId} deleted successfully", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting patient with ID: {PatientId}", id);
                throw;
            }
        }

        public async Task<bool> ToggleActiveStatusAsync(int id)
        {
            try
            {
                var patient = await _context.Patients.FindAsync(id);
                if (patient == null) return false;

                // If you have an IsActive field in your Patient model:
                // patient.IsActive = !patient.IsActive;
                // await _context.SaveChangesAsync();

                _logger.LogInformation("Patient {PatientId} active status toggled", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling active status for patient {PatientId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetBloodTypesAsync()
        {
            try
            {
                return await _context.Patients
                    .Where(p => p.BloodType != null)
                    .Select(p => p.BloodType!)
                    .Distinct()
                    .OrderBy(b => b)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving blood types");
                throw;
            }
        }

        public async Task<PatientStatsDto> GetPatientStatsAsync()
        {
            try
            {
                var totalPatients = await _context.Patients.CountAsync();

                var genderStats = await _context.Patients
                    .GroupBy(p => p.Gender ?? "Unknown")
                    .Select(g => new { Gender = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Gender, x => x.Count);

                var bloodTypeStats = await _context.Patients
                    .Where(p => p.BloodType != null)
                    .GroupBy(p => p.BloodType!)
                    .Select(g => new { BloodType = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.BloodType, x => x.Count);

                var averageAge = await _context.Patients
                    .Where(p => p.DateOfBirth.HasValue)
                    .AverageAsync(p => (double)(DateTime.UtcNow.Year - p.DateOfBirth.Value.Year));

                var patientsWithMedicalHistory = await _context.Patients
                    .CountAsync(p => !string.IsNullOrEmpty(p.MedicalHistory));

                var newPatientsThisMonth = await _context.Patients
                    .Where(p => p.User.CreatedAt >= DateTime.UtcNow.AddMonths(-1))
                    .CountAsync();

                return new PatientStatsDto
                {
                    TotalPatients = totalPatients,
                    GenderDistribution = genderStats,
                    BloodTypeDistribution = bloodTypeStats,
                    AverageAge = Math.Round(averageAge, 1),
                    PatientsWithMedicalHistory = patientsWithMedicalHistory,
                    NewPatientsThisMonth = newPatientsThisMonth
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patient statistics");
                throw;
            }
        }

        public async Task<IEnumerable<PatientDto>> GetPatientsByNGOAsync(int ngoId)
        {
            try
            {
                var patients = await _context.Patients
                    .Include(p => p.User)
                    .Include(p => p.NGO)
                    .Where(p => p.NGOId == ngoId) // تم التصحيح من NgosId إلى NGOId
                    .Select(p => _mapper.Map<PatientDto>(p))
                    .ToListAsync();

                return patients;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patients by NGO ID: {NgoId}", ngoId);
                throw;
            }
        }

        public async Task<IEnumerable<PatientDto>> GetPatientsByBloodTypeAsync(string bloodType)
        {
            try
            {
                var patients = await _context.Patients
                    .Include(p => p.User)
                    .Include(p => p.NGO)
                    .Where(p => p.BloodType == bloodType)
                    .Select(p => _mapper.Map<PatientDto>(p))
                    .ToListAsync();

                return patients;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patients by blood type: {BloodType}", bloodType);
                throw;
            }
        }

        public async Task<bool> PatientExistsAsync(int id)
        {
            return await _context.Patients.AnyAsync(p => p.PatientId == id);
        }

        public async Task<int> GetTotalPatientsCountAsync()
        {
            return await _context.Patients.CountAsync();
        }

        public async Task<IEnumerable<PatientMedicalSummaryDto>> GetPatientsMedicalSummaryAsync()
        {
            try
            {
                var summaries = await _context.Patients
                    .Include(p => p.Consultations)
                    .Include(p => p.MedicineRequests)
                    .Select(p => new PatientMedicalSummaryDto
                    {
                        PatientId = p.PatientId,
                        PatientName = p.PatientName,
                        BloodType = p.BloodType,
                        HasMedicalHistory = !string.IsNullOrEmpty(p.MedicalHistory),
                        TotalConsultations = p.Consultations.Count,
                        TotalMedicineRequests = p.MedicineRequests.Count,
                        LastConsultationDate = p.Consultations
                            .OrderByDescending(c => c.ConsDate) // تم التصحيح من ConsultationDate إلى ConsDate
                            .Select(c => c.ConsDate)
                            .FirstOrDefault(),
                        HasChronicConditions = p.MedicalHistory != null &&
                            (p.MedicalHistory.Contains("diabetes", StringComparison.OrdinalIgnoreCase) ||
                             p.MedicalHistory.Contains("hypertension", StringComparison.OrdinalIgnoreCase) ||
                             p.MedicalHistory.Contains("asthma", StringComparison.OrdinalIgnoreCase))
                    })
                    .ToListAsync();

                return summaries;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patients medical summary");
                throw;
            }
        }
    }
}