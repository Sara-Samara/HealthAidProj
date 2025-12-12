// Services/Implementations/MentalSupportSessionService.cs
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using HealthAidAPI.Data;
using HealthAidAPI.DTOs.MentalSupportSessions;
using HealthAidAPI.Services.Interfaces;
using HealthAidAPI.Models;
using HealthAidAPI.Helpers;

namespace HealthAidAPI.Services.Implementations
{
    public class MentalSupportSessionService : IMentalSupportSessionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<MentalSupportSessionService> _logger;

        public MentalSupportSessionService(ApplicationDbContext context, IMapper mapper, ILogger<MentalSupportSessionService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<MentalSupportSessionDto>> GetSessionsAsync(MentalSupportSessionFilterDto filter)
        {
            try
            {
                var query = _context.MentalSupportSessions
                    .Include(m => m.Patient)
                        .ThenInclude(p => p!.User)
                    .Include(m => m.Doctor)
                        .ThenInclude(d => d!.User)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(filter.SessionType))
                    query = query.Where(m => m.SessionType == filter.SessionType);

                if (filter.PatientId.HasValue)
                    query = query.Where(m => m.PatientId == filter.PatientId.Value);

                if (filter.DoctorId.HasValue)
                    query = query.Where(m => m.DoctorId == filter.DoctorId.Value);

                if (!string.IsNullOrEmpty(filter.Status))
                    query = query.Where(m => m.Status == filter.Status);

                if (filter.IsCompleted.HasValue)
                    query = query.Where(m => m.IsCompleted == filter.IsCompleted.Value);

                if (filter.DateFrom.HasValue)
                    query = query.Where(m => m.Date >= filter.DateFrom.Value);

                if (filter.DateTo.HasValue)
                    query = query.Where(m => m.Date <= filter.DateTo.Value);

                if (filter.CreatedFrom.HasValue)
                    query = query.Where(m => m.CreatedAt >= filter.CreatedFrom.Value);

                if (filter.CreatedTo.HasValue)
                    query = query.Where(m => m.CreatedAt <= filter.CreatedTo.Value);

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "date" => filter.SortDesc ?
                        query.OrderByDescending(m => m.Date) : query.OrderBy(m => m.Date),
                    "patient" => filter.SortDesc ?
                        query.OrderByDescending(m => m.Patient!.User!.FirstName) : query.OrderBy(m => m.Patient!.User!.FirstName),
                    "doctor" => filter.SortDesc ?
                        query.OrderByDescending(m => m.Doctor!.User!.FirstName) : query.OrderBy(m => m.Doctor!.User!.FirstName),
                    "type" => filter.SortDesc ?
                        query.OrderByDescending(m => m.SessionType) : query.OrderBy(m => m.SessionType),
                    "status" => filter.SortDesc ?
                        query.OrderByDescending(m => m.Status) : query.OrderBy(m => m.Status),
                    _ => filter.SortDesc ?
                        query.OrderByDescending(m => m.MentalSupportSessionId) : query.OrderBy(m => m.MentalSupportSessionId)
                };

                var totalCount = await query.CountAsync();
                var sessions = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(m => new MentalSupportSessionDto
                    {
                        MentalSupportSessionId = m.MentalSupportSessionId,
                        SessionType = m.SessionType,
                        Date = m.Date,
                        Notes = m.Notes,
                        PatientId = m.PatientId,
                        DoctorId = m.DoctorId,
                        PatientName = m.Patient != null && m.Patient.User != null ?
                            $"{m.Patient.User.FirstName} {m.Patient.User.LastName}" : "Unknown Patient",
                        DoctorName = m.Doctor != null && m.Doctor.User != null ?
                            $"{m.Doctor.User.FirstName} {m.Doctor.User.LastName}" : "Unknown Doctor",
                        PatientEmail = m.Patient != null && m.Patient.User != null ?
                            m.Patient.User.Email : null,
                        DoctorEmail = m.Doctor != null && m.Doctor.User != null ?
                            m.Doctor.User.Email : null,
                        Status = m.Status,
                        Duration = m.Duration,
                        IsCompleted = m.IsCompleted,
                        CreatedAt = m.CreatedAt,
                        UpdatedAt = m.UpdatedAt
                    })
                    .ToListAsync();

                return new PagedResult<MentalSupportSessionDto>(sessions, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving mental support sessions with filter");
                throw;
            }
        }

        public async Task<MentalSupportSessionDto?> GetSessionByIdAsync(int id)
        {
            var session = await _context.MentalSupportSessions
                .Include(m => m.Patient)
                    .ThenInclude(p => p!.User)
                .Include(m => m.Doctor)
                    .ThenInclude(d => d!.User)
                .FirstOrDefaultAsync(m => m.MentalSupportSessionId == id);

            if (session == null) return null;

            return new MentalSupportSessionDto
            {
                MentalSupportSessionId = session.MentalSupportSessionId,
                SessionType = session.SessionType,
                Date = session.Date,
                Notes = session.Notes,
                PatientId = session.PatientId,
                DoctorId = session.DoctorId,
                PatientName = session.Patient != null && session.Patient.User != null ?
                    $"{session.Patient.User.FirstName} {session.Patient.User.LastName}" : "Unknown Patient",
                DoctorName = session.Doctor != null && session.Doctor.User != null ?
                    $"{session.Doctor.User.FirstName} {session.Doctor.User.LastName}" : "Unknown Doctor",
                PatientEmail = session.Patient != null && session.Patient.User != null ?
                    session.Patient.User.Email : null,
                DoctorEmail = session.Doctor != null && session.Doctor.User != null ?
                    session.Doctor.User.Email : null,
                Status = session.Status,
                Duration = session.Duration,
                IsCompleted = session.IsCompleted,
                CreatedAt = session.CreatedAt,
                UpdatedAt = session.UpdatedAt
            };
        }

        public async Task<MentalSupportSessionDto> CreateSessionAsync(CreateMentalSupportSessionDto sessionDto)
        {
            // Validate patient exists
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PatientId == sessionDto.PatientId);
            if (patient == null)
                throw new ArgumentException($"Patient with ID {sessionDto.PatientId} not found");

            // Validate doctor exists
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DoctorId == sessionDto.DoctorId);
            if (doctor == null)
                throw new ArgumentException($"Doctor with ID {sessionDto.DoctorId} not found");

            // Check for scheduling conflicts
            var sessionEnd = sessionDto.Date.AddMinutes(sessionDto.DurationMinutes);
            var hasConflict = await _context.MentalSupportSessions
                .AnyAsync(m => m.DoctorId == sessionDto.DoctorId &&
                              m.Date < sessionEnd &&
                              m.Date.Add(m.Duration) > sessionDto.Date &&
                              m.Status != "Cancelled");

            if (hasConflict)
                throw new InvalidOperationException("Doctor is not available at the requested time");

            var session = new MentalSupportSession
            {
                SessionType = sessionDto.SessionType,
                Date = sessionDto.Date,
                Notes = sessionDto.Notes,
                PatientId = sessionDto.PatientId,
                DoctorId = sessionDto.DoctorId,
                Duration = TimeSpan.FromMinutes(sessionDto.DurationMinutes),
                Status = "Scheduled",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.MentalSupportSessions.Add(session);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Mental support session created: {SessionType} for patient {PatientId} with doctor {DoctorId}",
                sessionDto.SessionType, sessionDto.PatientId, sessionDto.DoctorId);

            return await GetSessionByIdAsync(session.MentalSupportSessionId) ??
                throw new InvalidOperationException("Failed to retrieve created session");
        }

        public async Task<MentalSupportSessionDto?> UpdateSessionAsync(int id, UpdateMentalSupportSessionDto sessionDto)
        {
            var session = await _context.MentalSupportSessions
                .Include(m => m.Patient)
                    .ThenInclude(p => p!.User)
                .Include(m => m.Doctor)
                    .ThenInclude(d => d!.User)
                .FirstOrDefaultAsync(m => m.MentalSupportSessionId == id);

            if (session == null) return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(sessionDto.SessionType))
                session.SessionType = sessionDto.SessionType;

            if (sessionDto.Date.HasValue)
                session.Date = sessionDto.Date.Value;

            if (sessionDto.Notes != null)
                session.Notes = sessionDto.Notes;

            if (sessionDto.DurationMinutes.HasValue)
                session.Duration = TimeSpan.FromMinutes(sessionDto.DurationMinutes.Value);

            if (!string.IsNullOrEmpty(sessionDto.Status))
                session.Status = sessionDto.Status;

            if (sessionDto.IsCompleted.HasValue)
                session.IsCompleted = sessionDto.IsCompleted.Value;

            session.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Mental support session updated: {SessionId}", id);

            return new MentalSupportSessionDto
            {
                MentalSupportSessionId = session.MentalSupportSessionId,
                SessionType = session.SessionType,
                Date = session.Date,
                Notes = session.Notes,
                PatientId = session.PatientId,
                DoctorId = session.DoctorId,
                PatientName = session.Patient != null && session.Patient.User != null ?
                    $"{session.Patient.User.FirstName} {session.Patient.User.LastName}" : "Unknown Patient",
                DoctorName = session.Doctor != null && session.Doctor.User != null ?
                    $"{session.Doctor.User.FirstName} {session.Doctor.User.LastName}" : "Unknown Doctor",
                PatientEmail = session.Patient != null && session.Patient.User != null ?
                    session.Patient.User.Email : null,
                DoctorEmail = session.Doctor != null && session.Doctor.User != null ?
                    session.Doctor.User.Email : null,
                Status = session.Status,
                Duration = session.Duration,
                IsCompleted = session.IsCompleted,
                CreatedAt = session.CreatedAt,
                UpdatedAt = session.UpdatedAt
            };
        }

        public async Task<bool> DeleteSessionAsync(int id)
        {
            var session = await _context.MentalSupportSessions.FindAsync(id);
            if (session == null) return false;

            _context.MentalSupportSessions.Remove(session);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Mental support session deleted: {SessionId}", id);
            return true;
        }

        public async Task<bool> CompleteSessionAsync(int id, string? notes = null)
        {
            var session = await _context.MentalSupportSessions.FindAsync(id);
            if (session == null) return false;

            session.Status = "Completed";
            session.IsCompleted = true;
            if (!string.IsNullOrEmpty(notes))
                session.Notes = notes;
            session.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Mental support session completed: {SessionId}", id);
            return true;
        }

        public async Task<bool> CancelSessionAsync(int id, string? reason = null)
        {
            var session = await _context.MentalSupportSessions.FindAsync(id);
            if (session == null) return false;

            session.Status = "Cancelled";
            if (!string.IsNullOrEmpty(reason))
                session.Notes = $"Cancelled: {reason}";
            session.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Mental support session cancelled: {SessionId}", id);
            return true;
        }

        public async Task<MentalSupportSessionStatsDto> GetSessionStatsAsync()
        {
            var totalSessions = await _context.MentalSupportSessions.CountAsync();
            var completedSessions = await _context.MentalSupportSessions.CountAsync(m => m.IsCompleted);
            var upcomingSessions = await _context.MentalSupportSessions
                .CountAsync(m => m.Date > DateTime.UtcNow && m.Status == "Scheduled");

            var sessionTypesCount = await _context.MentalSupportSessions
                .GroupBy(m => m.SessionType)
                .Select(g => new { SessionType = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.SessionType, x => x.Count);

            var statusCount = await _context.MentalSupportSessions
                .GroupBy(m => m.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            var totalPatients = await _context.MentalSupportSessions
                .Select(m => m.PatientId)
                .Distinct()
                .CountAsync();

            var totalDoctors = await _context.MentalSupportSessions
                .Select(m => m.DoctorId)
                .Distinct()
                .CountAsync();

            var recentSessions = await _context.MentalSupportSessions
                .Include(m => m.Patient)
                    .ThenInclude(p => p!.User)
                .Include(m => m.Doctor)
                    .ThenInclude(d => d!.User)
                .OrderByDescending(m => m.Date)
                .Take(5)
                .Select(m => new RecentSessionDto
                {
                    MentalSupportSessionId = m.MentalSupportSessionId,
                    SessionType = m.SessionType,
                    Date = m.Date,
                    PatientName = m.Patient != null && m.Patient.User != null ?
                        $"{m.Patient.User.FirstName} {m.Patient.User.LastName}" : "Unknown Patient",
                    DoctorName = m.Doctor != null && m.Doctor.User != null ?
                        $"{m.Doctor.User.FirstName} {m.Doctor.User.LastName}" : "Unknown Doctor",
                    Status = m.Status
                })
                .ToListAsync();

            return new MentalSupportSessionStatsDto
            {
                TotalSessions = totalSessions,
                CompletedSessions = completedSessions,
                UpcomingSessions = upcomingSessions,
                SessionTypesCount = sessionTypesCount,
                StatusCount = statusCount,
                TotalPatients = totalPatients,
                TotalDoctors = totalDoctors,
                RecentSessions = recentSessions
            };
        }

        public async Task<IEnumerable<MentalSupportSessionDto>> GetUpcomingSessionsAsync(int days = 7)
        {
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(days);

            var sessions = await _context.MentalSupportSessions
                .Include(m => m.Patient)
                    .ThenInclude(p => p!.User)
                .Include(m => m.Doctor)
                    .ThenInclude(d => d!.User)
                .Where(m => m.Date >= startDate && m.Date <= endDate && m.Status == "Scheduled")
                .OrderBy(m => m.Date)
                .Select(m => new MentalSupportSessionDto
                {
                    MentalSupportSessionId = m.MentalSupportSessionId,
                    SessionType = m.SessionType,
                    Date = m.Date,
                    Notes = m.Notes,
                    PatientId = m.PatientId,
                    DoctorId = m.DoctorId,
                    PatientName = m.Patient != null && m.Patient.User != null ?
                        $"{m.Patient.User.FirstName} {m.Patient.User.LastName}" : "Unknown Patient",
                    DoctorName = m.Doctor != null && m.Doctor.User != null ?
                        $"{m.Doctor.User.FirstName} {m.Doctor.User.LastName}" : "Unknown Doctor",
                    Status = m.Status,
                    Duration = m.Duration,
                    IsCompleted = m.IsCompleted,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                })
                .ToListAsync();

            return sessions;
        }

        public async Task<IEnumerable<MentalSupportSessionDto>> GetPatientSessionsAsync(int patientId)
        {
            var sessions = await _context.MentalSupportSessions
                .Include(m => m.Patient)
                    .ThenInclude(p => p!.User)
                .Include(m => m.Doctor)
                    .ThenInclude(d => d!.User)
                .Where(m => m.PatientId == patientId)
                .OrderByDescending(m => m.Date)
                .Select(m => new MentalSupportSessionDto
                {
                    MentalSupportSessionId = m.MentalSupportSessionId,
                    SessionType = m.SessionType,
                    Date = m.Date,
                    Notes = m.Notes,
                    PatientId = m.PatientId,
                    DoctorId = m.DoctorId,
                    PatientName = m.Patient != null && m.Patient.User != null ?
                        $"{m.Patient.User.FirstName} {m.Patient.User.LastName}" : "Unknown Patient",
                    DoctorName = m.Doctor != null && m.Doctor.User != null ?
                        $"{m.Doctor.User.FirstName} {m.Doctor.User.LastName}" : "Unknown Doctor",
                    Status = m.Status,
                    Duration = m.Duration,
                    IsCompleted = m.IsCompleted,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                })
                .ToListAsync();

            return sessions;
        }

        public async Task<IEnumerable<MentalSupportSessionDto>> GetDoctorSessionsAsync(int doctorId)
        {
            var sessions = await _context.MentalSupportSessions
                .Include(m => m.Patient)
                    .ThenInclude(p => p!.User)
                .Include(m => m.Doctor)
                    .ThenInclude(d => d!.User)
                .Where(m => m.DoctorId == doctorId)
                .OrderByDescending(m => m.Date)
                .Select(m => new MentalSupportSessionDto
                {
                    MentalSupportSessionId = m.MentalSupportSessionId,
                    SessionType = m.SessionType,
                    Date = m.Date,
                    Notes = m.Notes,
                    PatientId = m.PatientId,
                    DoctorId = m.DoctorId,
                    PatientName = m.Patient != null && m.Patient.User != null ?
                        $"{m.Patient.User.FirstName} {m.Patient.User.LastName}" : "Unknown Patient",
                    DoctorName = m.Doctor != null && m.Doctor.User != null ?
                        $"{m.Doctor.User.FirstName} {m.Doctor.User.LastName}" : "Unknown Doctor",
                    Status = m.Status,
                    Duration = m.Duration,
                    IsCompleted = m.IsCompleted,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                })
                .ToListAsync();

            return sessions;
        }

        public async Task<SessionAvailabilityDto> GetDoctorAvailabilityAsync(int doctorId, DateTime date)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null)
                throw new ArgumentException($"Doctor with ID {doctorId} not found");

            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1);

            // Get existing sessions for the day
            var existingSessions = await _context.MentalSupportSessions
                .Where(m => m.DoctorId == doctorId &&
                           m.Date >= dayStart && m.Date < dayEnd &&
                           m.Status != "Cancelled")
                .OrderBy(m => m.Date)
                .ToListAsync();

            // Generate available time slots (9 AM to 5 PM, 1-hour slots)
            var availableSlots = new List<TimeSlotDto>();
            var startTime = dayStart.AddHours(9); // 9:00 AM
            var endTime = dayStart.AddHours(17);  // 5:00 PM

            for (var slotStart = startTime; slotStart < endTime; slotStart = slotStart.AddHours(1))
            {
                var slotEnd = slotStart.AddHours(1);
                var isAvailable = !existingSessions.Any(session =>
                    session.Date < slotEnd && session.Date.Add(session.Duration) > slotStart);

                availableSlots.Add(new TimeSlotDto
                {
                    StartTime = slotStart,
                    EndTime = slotEnd,
                    IsAvailable = isAvailable
                });
            }

            return new SessionAvailabilityDto
            {
                DoctorId = doctorId,
                Date = date,
                AvailableSlots = availableSlots
            };
        }

        public async Task<bool> RescheduleSessionAsync(int sessionId, DateTime newDate)
        {
            var session = await _context.MentalSupportSessions.FindAsync(sessionId);
            if (session == null) return false;

            // Check for conflicts with new time
            var sessionEnd = newDate.Add(session.Duration);
            var hasConflict = await _context.MentalSupportSessions
                .AnyAsync(m => m.DoctorId == session.DoctorId &&
                              m.MentalSupportSessionId != sessionId &&
                              m.Date < sessionEnd &&
                              m.Date.Add(m.Duration) > newDate &&
                              m.Status != "Cancelled");

            if (hasConflict)
                throw new InvalidOperationException("Doctor is not available at the requested time");

            session.Date = newDate;
            session.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Mental support session rescheduled: {SessionId} to {NewDate}", sessionId, newDate);
            return true;
        }

        public async Task<IEnumerable<string>> GetSessionTypesAsync()
        {
            return await _context.MentalSupportSessions
                .Select(m => m.SessionType)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }
    }
}