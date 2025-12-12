using AutoMapper;
using HealthAidAPI.Data;
using HealthAidAPI.DTOs.Users;
using HealthAidAPI.DTOs.Consultations;
using HealthAidAPI.DTOs.Patients;
using HealthAidAPI.Helpers;
using HealthAidAPI.Models;
using HealthAidAPI.DTOs.Doctors;
using HealthAidAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthAidAPI.Services.Implementations
{
    public class ConsultationService : IConsultationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ConsultationService> _logger;

        public ConsultationService(ApplicationDbContext context, IMapper mapper, ILogger<ConsultationService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<ConsultationDto>> GetConsultationsAsync(ConsultationFilterDto filter)
        {
            try
            {
                var query = _context.Consultations
                    .Include(c => c.Doctor)
                        .ThenInclude(d => d.User)
                    .Include(c => c.Patient)
                        .ThenInclude(p => p.User)
                    .Include(c => c.Appointment)
                    .Include(c => c.Prescriptions)
                    .Include(c => c.Transactions)
                    .AsQueryable();

                // Apply filters
                if (filter.DoctorId.HasValue)
                    query = query.Where(c => c.DoctorId == filter.DoctorId.Value);

                if (filter.PatientId.HasValue)
                    query = query.Where(c => c.PatientId == filter.PatientId.Value);

                if (!string.IsNullOrEmpty(filter.Status))
                    query = query.Where(c => c.Status == filter.Status);

                if (!string.IsNullOrEmpty(filter.Mode))
                    query = query.Where(c => c.Mode == filter.Mode);

                if (filter.StartDate.HasValue)
                    query = query.Where(c => c.ConsDate >= filter.StartDate.Value);

                if (filter.EndDate.HasValue)
                    query = query.Where(c => c.ConsDate <= filter.EndDate.Value);

                if (!string.IsNullOrEmpty(filter.Search))
                {
                    query = query.Where(c =>
                        c.Diagnosis != null && c.Diagnosis.Contains(filter.Search) ||
                        c.Note != null && c.Note.Contains(filter.Search) ||
                        c.Doctor.User.FirstName.Contains(filter.Search) ||
                        c.Doctor.User.LastName.Contains(filter.Search) ||
                        c.Patient.PatientName.Contains(filter.Search));
                }

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "date" => filter.SortDesc ?
                        query.OrderByDescending(c => c.ConsDate) :
                        query.OrderBy(c => c.ConsDate),
                    "status" => filter.SortDesc ?
                        query.OrderByDescending(c => c.Status) :
                        query.OrderBy(c => c.Status),
                    "doctor" => filter.SortDesc ?
                        query.OrderByDescending(c => c.Doctor.User.LastName).ThenByDescending(c => c.Doctor.User.FirstName) :
                        query.OrderBy(c => c.Doctor.User.LastName).ThenBy(c => c.Doctor.User.FirstName),
                    "patient" => filter.SortDesc ?
                        query.OrderByDescending(c => c.Patient.PatientName) :
                        query.OrderBy(c => c.Patient.PatientName),
                    _ => filter.SortDesc ?
                        query.OrderByDescending(c => c.ConsultationId) :
                        query.OrderBy(c => c.ConsultationId)
                };

                var totalCount = await query.CountAsync();

                var consultations = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(c => new ConsultationDto
                    {
                        ConsultationId = c.ConsultationId,
                        ConsDate = c.ConsDate,
                        Diagnosis = c.Diagnosis,
                        Status = c.Status,
                        Mode = c.Mode,
                        Note = c.Note,
                        DoctorId = c.DoctorId,
                        PatientId = c.PatientId,
                        AppointmentId = c.AppointmentId,
                        Doctor = new DoctorDto
                        {
                            DoctorId = c.Doctor.DoctorId,
                            Specialization = c.Doctor.Specialization,
                            YearsExperience = c.Doctor.YearsExperience,
                            User = new UserDto
                            {
                                FirstName = c.Doctor.User.FirstName,
                                LastName = c.Doctor.User.LastName,
                                Email = c.Doctor.User.Email
                            }
                        },
                        Patient = new PatientDto
                        {
                            PatientId = c.Patient.PatientId,
                            PatientName = c.Patient.PatientName,
                            Gender = c.Patient.Gender,
                            BloodType = c.Patient.BloodType
                        },
                        //Appointment = c.Appointment != null ? new AppointmentDto
                        //{
                        //    AppointmentId = c.Appointment.AppointmentId,
                        //    AppointmentDate = c.Appointment.AppointmentDate,
                        //    Status = c.Appointment.Status
                        //} : null,
                        PrescriptionCount = c.Prescriptions.Count,
                        TransactionCount = c.Transactions.Count
                    })
                    .ToListAsync();

                return new PagedResult<ConsultationDto>(consultations, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving consultations with filter {@Filter}", filter);
                throw;
            }
        }

        public async Task<ConsultationDto?> GetConsultationByIdAsync(int id)
        {
            try
            {
                var consultation = await _context.Consultations
                    .Include(c => c.Doctor)
                        .ThenInclude(d => d.User)
                    .Include(c => c.Patient)
                        .ThenInclude(p => p.User)
                    .Include(c => c.Appointment)
                    .Include(c => c.Prescriptions)
                    .Include(c => c.Transactions)
                    .FirstOrDefaultAsync(c => c.ConsultationId == id);

                if (consultation == null)
                {
                    _logger.LogWarning("Consultation with ID {ConsultationId} not found", id);
                    return null;
                }

                var consultationDto = _mapper.Map<ConsultationDto>(consultation);
                consultationDto.PrescriptionCount = consultation.Prescriptions.Count;
                consultationDto.TransactionCount = consultation.Transactions.Count;

                return consultationDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving consultation by ID: {ConsultationId}", id);
                throw;
            }
        }

        public async Task<ConsultationDto> CreateConsultationAsync(CreateConsultationDto consultationDto)
        {
            try
            {
                // Validate doctor exists
                var doctorExists = await _context.Doctors.AnyAsync(d => d.DoctorId == consultationDto.DoctorId);
                if (!doctorExists)
                    throw new ArgumentException("Doctor not found");

                // Validate patient exists
                var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == consultationDto.PatientId);
                if (!patientExists)
                    throw new ArgumentException("Patient not found");

                // Validate appointment exists if provided
                if (consultationDto.AppointmentId.HasValue)
                {
                    var appointmentExists = await _context.Appointments.AnyAsync(a => a.AppointmentId == consultationDto.AppointmentId.Value);
                    if (!appointmentExists)
                        throw new ArgumentException("Appointment not found");
                }

                var consultation = new Consultation
                {
                    ConsDate = consultationDto.ConsDate,
                    Diagnosis = consultationDto.Diagnosis?.Trim(),
                    Status = consultationDto.Status,
                    Mode = consultationDto.Mode,
                    Note = consultationDto.Note?.Trim(),
                    DoctorId = consultationDto.DoctorId.Value,
                    PatientId = consultationDto.PatientId.Value,
                    AppointmentId = consultationDto.AppointmentId
                };

                _context.Consultations.Add(consultation);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Consultation {ConsultationId} created successfully for patient {PatientId} with doctor {DoctorId}",
                    consultation.ConsultationId, consultationDto.PatientId, consultationDto.DoctorId);

                return await GetConsultationByIdAsync(consultation.ConsultationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating consultation with data {@ConsultationDto}", consultationDto);
                throw;
            }
        }

        public async Task<ConsultationDto?> UpdateConsultationAsync(int id, UpdateConsultationDto consultationDto)
        {
            try
            {
                var consultation = await _context.Consultations
                    .Include(c => c.Doctor)
                    .Include(c => c.Patient)
                    .FirstOrDefaultAsync(c => c.ConsultationId == id);

                if (consultation == null)
                {
                    _logger.LogWarning("Consultation with ID {ConsultationId} not found for update", id);
                    return null;
                }

                // Update only provided fields
                if (consultationDto.ConsDate.HasValue)
                    consultation.ConsDate = consultationDto.ConsDate;

                if (consultationDto.Diagnosis != null)
                    consultation.Diagnosis = consultationDto.Diagnosis.Trim();

                if (!string.IsNullOrEmpty(consultationDto.Status))
                    consultation.Status = consultationDto.Status;

                if (consultationDto.Mode != null)
                    consultation.Mode = consultationDto.Mode;

                if (consultationDto.Note != null)
                    consultation.Note = consultationDto.Note.Trim();

                if (consultationDto.DoctorId.HasValue)
                {
                    var doctorExists = await _context.Doctors.AnyAsync(d => d.DoctorId == consultationDto.DoctorId.Value);
                    if (!doctorExists)
                        throw new ArgumentException("Doctor not found");
                    consultation.DoctorId = consultationDto.DoctorId.Value;
                }

                if (consultationDto.PatientId.HasValue)
                {
                    var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == consultationDto.PatientId.Value);
                    if (!patientExists)
                        throw new ArgumentException("Patient not found");
                    consultation.PatientId = consultationDto.PatientId.Value;
                }

                if (consultationDto.AppointmentId.HasValue)
                {
                    var appointmentExists = await _context.Appointments.AnyAsync(a => a.AppointmentId == consultationDto.AppointmentId.Value);
                    if (!appointmentExists)
                        throw new ArgumentException("Appointment not found");
                    consultation.AppointmentId = consultationDto.AppointmentId.Value;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Consultation {ConsultationId} updated successfully", id);
                return await GetConsultationByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating consultation with ID: {ConsultationId} and data: {@ConsultationDto}", id, consultationDto);
                throw;
            }
        }

        public async Task<bool> DeleteConsultationAsync(int id)
        {
            try
            {
                var consultation = await _context.Consultations
                    .Include(c => c.Prescriptions)
                    .Include(c => c.Transactions)
                    .FirstOrDefaultAsync(c => c.ConsultationId == id);

                if (consultation == null)
                {
                    _logger.LogWarning("Consultation with ID {ConsultationId} not found for deletion", id);
                    return false;
                }

                // Check for related data
                if (consultation.Prescriptions.Any() || consultation.Transactions.Any())
                {
                    _logger.LogWarning("Cannot delete consultation {ConsultationId} with related prescriptions or transactions", id);
                    throw new InvalidOperationException(
                        "Cannot delete consultation with related prescriptions or transactions. " +
                        "Please delete related records first.");
                }

                _context.Consultations.Remove(consultation);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Consultation {ConsultationId} deleted successfully", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting consultation with ID: {ConsultationId}", id);
                throw;
            }
        }

        public async Task<bool> UpdateConsultationStatusAsync(int id, string status)
        {
            try
            {
                var consultation = await _context.Consultations.FindAsync(id);
                if (consultation == null)
                {
                    _logger.LogWarning("Consultation with ID {ConsultationId} not found for status update", id);
                    return false;
                }

                consultation.Status = status;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Consultation {ConsultationId} status updated to {Status}", id, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating consultation status for ID: {ConsultationId} to {Status}", id, status);
                throw;
            }
        }

        public async Task<IEnumerable<ConsultationDto>> GetConsultationsByDoctorAsync(int doctorId)
        {
            try
            {
                var consultations = await _context.Consultations
                    .Include(c => c.Doctor)
                        .ThenInclude(d => d.User)
                    .Include(c => c.Patient)
                    .Where(c => c.DoctorId == doctorId)
                    .OrderByDescending(c => c.ConsDate)
                    .Select(c => _mapper.Map<ConsultationDto>(c))
                    .ToListAsync();

                return consultations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving consultations for doctor ID: {DoctorId}", doctorId);
                throw;
            }
        }

        public async Task<IEnumerable<ConsultationDto>> GetConsultationsByPatientAsync(int patientId)
        {
            try
            {
                var consultations = await _context.Consultations
                    .Include(c => c.Doctor)
                        .ThenInclude(d => d.User)
                    .Include(c => c.Patient)
                    .Where(c => c.PatientId == patientId)
                    .OrderByDescending(c => c.ConsDate)
                    .Select(c => _mapper.Map<ConsultationDto>(c))
                    .ToListAsync();

                return consultations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving consultations for patient ID: {PatientId}", patientId);
                throw;
            }
        }

        public async Task<IEnumerable<ConsultationDto>> GetConsultationsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var consultations = await _context.Consultations
                    .Include(c => c.Doctor)
                        .ThenInclude(d => d.User)
                    .Include(c => c.Patient)
                    .Where(c => c.ConsDate >= startDate && c.ConsDate <= endDate)
                    .OrderBy(c => c.ConsDate)
                    .Select(c => _mapper.Map<ConsultationDto>(c))
                    .ToListAsync();

                return consultations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving consultations for date range {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<ConsultationStatsDto> GetConsultationStatsAsync()
        {
            try
            {
                var totalConsultations = await _context.Consultations.CountAsync();

                var statusDistribution = await _context.Consultations
                    .GroupBy(c => c.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Status, x => x.Count);

                var modeDistribution = await _context.Consultations
                    .Where(c => c.Mode != null)
                    .GroupBy(c => c.Mode!)
                    .Select(g => new { Mode = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Mode, x => x.Count);

                var completedConsultations = await _context.Consultations
                    .CountAsync(c => c.Status == "Completed");

                var pendingConsultations = await _context.Consultations
                    .CountAsync(c => c.Status == "Pending" || c.Status == "Scheduled");

                var canceledConsultations = await _context.Consultations
                    .CountAsync(c => c.Status == "Canceled");

                var consultationsThisMonth = await _context.Consultations
                    .CountAsync(c => c.ConsDate >= DateTime.UtcNow.AddMonths(-1));

                var averageConsultationsPerDay = await _context.Consultations
                    .Where(c => c.ConsDate.HasValue)
                    .GroupBy(c => c.ConsDate.Value.Date)
                    .Select(g => new { Date = g.Key, Count = g.Count() })
                    .AverageAsync(g => (double)g.Count);

                return new ConsultationStatsDto
                {
                    TotalConsultations = totalConsultations,
                    CompletedConsultations = completedConsultations,
                    PendingConsultations = pendingConsultations,
                    CanceledConsultations = canceledConsultations,
                    StatusDistribution = statusDistribution,
                    ModeDistribution = modeDistribution,
                    ConsultationsThisMonth = consultationsThisMonth,
                    AverageConsultationsPerDay = Math.Round(averageConsultationsPerDay, 2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving consultation statistics");
                throw;
            }
        }

        public async Task<IEnumerable<string>> GetConsultationModesAsync()
        {
            try
            {
                return await _context.Consultations
                    .Where(c => c.Mode != null)
                    .Select(c => c.Mode!)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving consultation modes");
                throw;
            }
        }

        public async Task<bool> ConsultationExistsAsync(int id)
        {
            return await _context.Consultations.AnyAsync(c => c.ConsultationId == id);
        }
    }
}