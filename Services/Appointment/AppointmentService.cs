// Services/Implementations/AppointmentService.cs
using AutoMapper;
using HealthAidAPI.Data;
using HealthAidAPI.DTOs.Appointments;
using HealthAidAPI.DTOs.PublicAlerts;
using HealthAidAPI.Helpers;
using HealthAidAPI.Models;
using HealthAidAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace HealthAidAPI.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<AppointmentService> _logger;
        private readonly INotificationService _notificationService;

        public AppointmentService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<AppointmentService> logger,
            INotificationService notificationService)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<PagedResult<AppointmentDto>> GetAppointmentsAsync(AppointmentFilterDto filter)
        {
            try
            {
                var query = _context.Appointments
                    .Include(a => a.Doctor)
                        .ThenInclude(d => d.User)
                    .Include(a => a.Patient)
                        .ThenInclude(p => p.User)
                    .AsQueryable();

                // Apply filters
                if (filter.DoctorId.HasValue)
                    query = query.Where(a => a.DoctorId == filter.DoctorId.Value);

                if (filter.PatientId.HasValue)
                    query = query.Where(a => a.PatientId == filter.PatientId.Value);

                if (!string.IsNullOrEmpty(filter.Status))
                    query = query.Where(a => a.Status == filter.Status);

                if (filter.StartDate.HasValue)
                    query = query.Where(a => a.AppointmentDate >= filter.StartDate.Value);

                if (filter.EndDate.HasValue)
                    query = query.Where(a => a.AppointmentDate <= filter.EndDate.Value);

                if (!string.IsNullOrEmpty(filter.Search))
                {
                    query = query.Where(a =>
                        a.Note != null && a.Note.Contains(filter.Search) ||
                        a.Doctor.User.FirstName.Contains(filter.Search) ||
                        a.Doctor.User.LastName.Contains(filter.Search) ||
                        a.Patient.User.FirstName.Contains(filter.Search) ||
                        a.Patient.User.LastName.Contains(filter.Search));
                }

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "date" => filter.SortDesc ?
                        query.OrderByDescending(a => a.AppointmentDate) : query.OrderBy(a => a.AppointmentDate),
                    "doctor" => filter.SortDesc ?
                        query.OrderByDescending(a => a.Doctor.User.LastName) :
                        query.OrderBy(a => a.Doctor.User.LastName),
                    "patient" => filter.SortDesc ?
                        query.OrderByDescending(a => a.Patient.User.LastName) :
                        query.OrderBy(a => a.Patient.User.LastName),
                    "status" => filter.SortDesc ?
                        query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
                    _ => filter.SortDesc ?
                        query.OrderByDescending(a => a.AppointmentId) : query.OrderBy(a => a.AppointmentId)
                };

                var totalCount = await query.CountAsync();
                var appointments = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(a => new AppointmentDto
                    {
                        AppointmentId = a.AppointmentId,
                        AppointmentDate = a.AppointmentDate,
                        Status = a.Status,
                        Note = a.Note,
                        DoctorId = a.DoctorId,
                        PatientId = a.PatientId,
                        DoctorName = $"{a.Doctor.User.FirstName} {a.Doctor.User.LastName}",
                        PatientName = $"{a.Patient.User.FirstName} {a.Patient.User.LastName}",
                        DoctorSpecialization = a.Doctor.Specialization,
                        CreatedAt = a.AppointmentDate
                    })
                    .ToListAsync();
                return new PagedResult<AppointmentDto>(appointments, totalCount);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving appointments with filter");
                throw;
            }
        }

        public async Task<AppointmentDto?> GetAppointmentByIdAsync(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null) return null;

            return new AppointmentDto
            {
                AppointmentId = appointment.AppointmentId,
                AppointmentDate = appointment.AppointmentDate,
                Status = appointment.Status,
                Note = appointment.Note,
                DoctorId = appointment.DoctorId,
                PatientId = appointment.PatientId,
                DoctorName = $"{appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
                PatientName = $"{appointment.Patient.User.FirstName} {appointment.Patient.User.LastName}",
                DoctorSpecialization = appointment.Doctor.Specialization,
                CreatedAt = appointment.AppointmentDate
            };
        }

        public async Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto createAppointmentDto)
        {
            // Validate doctor exists
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DoctorId == createAppointmentDto.DoctorId);

            if (doctor == null)
                throw new ArgumentException($"Doctor with ID {createAppointmentDto.DoctorId} not found");

            // Validate patient exists
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PatientId == createAppointmentDto.PatientId);

            if (patient == null)
                throw new ArgumentException($"Patient with ID {createAppointmentDto.PatientId} not found");

            // Check if time slot is available
            if (!await IsTimeSlotAvailableAsync(createAppointmentDto.DoctorId, createAppointmentDto.AppointmentDate))
            {
                throw new InvalidOperationException("The selected time slot is not available. Please choose a different time.");
            }

            var appointment = new Appointment
            {
                AppointmentDate = createAppointmentDto.AppointmentDate,
                Status = "Scheduled",
                Note = createAppointmentDto.Note,
                DoctorId = createAppointmentDto.DoctorId,
                PatientId = createAppointmentDto.PatientId
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Send notifications
            await SendAppointmentCreatedNotifications(appointment, doctor, patient);

            _logger.LogInformation("Appointment created: {AppointmentId} for patient {PatientName} with doctor {DoctorName}",
                appointment.AppointmentId, patient.User.FirstName, doctor.User.FirstName);

            return new AppointmentDto
            {
                AppointmentId = appointment.AppointmentId,
                AppointmentDate = appointment.AppointmentDate,
                Status = appointment.Status,
                Note = appointment.Note,
                DoctorId = appointment.DoctorId,
                PatientId = appointment.PatientId,
                DoctorName = $"{doctor.User.FirstName} {doctor.User.LastName}",
                PatientName = $"{patient.User.FirstName} {patient.User.LastName}",
                DoctorSpecialization = doctor.Specialization,
                CreatedAt = appointment.AppointmentDate
            };
        }

        public async Task<AppointmentDto?> UpdateAppointmentAsync(int id, UpdateAppointmentDto updateAppointmentDto)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null) return null;

            // Update only provided fields
            if (updateAppointmentDto.AppointmentDate.HasValue)
            {
                // Get the doctor ID to check (use existing or new)
                int doctorIdToCheck = updateAppointmentDto.DoctorId ?? appointment.DoctorId;

                // Check if new time slot is available
                if (!await IsTimeSlotAvailableAsync(doctorIdToCheck, updateAppointmentDto.AppointmentDate.Value, id))
                {
                    throw new InvalidOperationException("The selected time slot is not available. Please choose a different time.");
                }
                appointment.AppointmentDate = updateAppointmentDto.AppointmentDate.Value;
            }

            if (!string.IsNullOrEmpty(updateAppointmentDto.Status))
                appointment.Status = updateAppointmentDto.Status;

            if (updateAppointmentDto.Note != null)
                appointment.Note = updateAppointmentDto.Note;

            if (updateAppointmentDto.DoctorId.HasValue)
            {
                var doctor = await _context.Doctors
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.DoctorId == updateAppointmentDto.DoctorId.Value);

                if (doctor == null)
                    throw new ArgumentException($"Doctor with ID {updateAppointmentDto.DoctorId.Value} not found");

                appointment.DoctorId = updateAppointmentDto.DoctorId.Value;
            }

            if (updateAppointmentDto.PatientId.HasValue)
            {
                var patient = await _context.Patients
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.PatientId == updateAppointmentDto.PatientId.Value);

                if (patient == null)
                    throw new ArgumentException($"Patient with ID {updateAppointmentDto.PatientId.Value} not found");

                appointment.PatientId = updateAppointmentDto.PatientId.Value;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Appointment updated: {AppointmentId}", id);

            return new AppointmentDto
            {
                AppointmentId = appointment.AppointmentId,
                AppointmentDate = appointment.AppointmentDate,
                Status = appointment.Status,
                Note = appointment.Note,
                DoctorId = appointment.DoctorId,
                PatientId = appointment.PatientId,
                DoctorName = $"{appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
                PatientName = $"{appointment.Patient.User.FirstName} {appointment.Patient.User.LastName}",
                DoctorSpecialization = appointment.Doctor.Specialization,
                CreatedAt = appointment.AppointmentDate
            };
        }

        public async Task<AppointmentDto?> RescheduleAppointmentAsync(int id, RescheduleAppointmentDto rescheduleDto)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null) return null;

            // Check if new time slot is available
            if (!await IsTimeSlotAvailableAsync(appointment.DoctorId, rescheduleDto.NewAppointmentDate, id))
            {
                throw new InvalidOperationException("The selected time slot is not available. Please choose a different time.");
            }

            var oldDate = appointment.AppointmentDate;
            appointment.AppointmentDate = rescheduleDto.NewAppointmentDate;
            appointment.Status = "Rescheduled";

            if (!string.IsNullOrEmpty(rescheduleDto.Reason))
            {
                appointment.Note = $"Rescheduled from {oldDate:yyyy-MM-dd HH:mm}. Reason: {rescheduleDto.Reason}";
            }

            await _context.SaveChangesAsync();

            // Send reschedule notification
            await SendAppointmentRescheduledNotification(appointment, oldDate);

            _logger.LogInformation("Appointment rescheduled: {AppointmentId} from {OldDate} to {NewDate}",
                id, oldDate, rescheduleDto.NewAppointmentDate);

            return new AppointmentDto
            {
                AppointmentId = appointment.AppointmentId,
                AppointmentDate = appointment.AppointmentDate,
                Status = appointment.Status,
                Note = appointment.Note,
                DoctorId = appointment.DoctorId,
                PatientId = appointment.PatientId,
                DoctorName = $"{appointment.Doctor.User.FirstName} {appointment.Doctor.User.LastName}",
                PatientName = $"{appointment.Patient.User.FirstName} {appointment.Patient.User.LastName}",
                DoctorSpecialization = appointment.Doctor.Specialization,
                CreatedAt = appointment.AppointmentDate
            };
        }

        public async Task<bool> CancelAppointmentAsync(int id, string? cancellationReason = null)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null) return false;

            if (appointment.Status == "Canceled")
                return true; // Already canceled

            appointment.Status = "Canceled";

            if (!string.IsNullOrEmpty(cancellationReason))
            {
                appointment.Note = $"Canceled. Reason: {cancellationReason}";
            }

            await _context.SaveChangesAsync();

            // Send cancellation notification
            await SendAppointmentCancelledNotification(appointment, cancellationReason);

            _logger.LogInformation("Appointment canceled: {AppointmentId}", id);
            return true;
        }

        public async Task<bool> ConfirmAppointmentAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return false;

            appointment.Status = "Confirmed";
            await _context.SaveChangesAsync();

            _logger.LogInformation("Appointment confirmed: {AppointmentId}", id);
            return true;
        }

        public async Task<bool> CompleteAppointmentAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return false;

            appointment.Status = "Completed";
            await _context.SaveChangesAsync();

            _logger.LogInformation("Appointment completed: {AppointmentId}", id);
            return true;
        }

        public async Task<bool> DeleteAppointmentAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return false;

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Appointment deleted: {AppointmentId}", id);
            return true;
        }

        public async Task<AppointmentStatsDto> GetAppointmentStatsAsync()
        {
            var totalAppointments = await _context.Appointments.CountAsync();
            var scheduledAppointments = await _context.Appointments.CountAsync(a => a.Status == "Scheduled");
            var confirmedAppointments = await _context.Appointments.CountAsync(a => a.Status == "Confirmed");
            var completedAppointments = await _context.Appointments.CountAsync(a => a.Status == "Completed");
            var canceledAppointments = await _context.Appointments.CountAsync(a => a.Status == "Canceled");

            var todayAppointments = await _context.Appointments
                .CountAsync(a => a.AppointmentDate.Date == DateTime.Today);

            var statusDistribution = await _context.Appointments
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            return new AppointmentStatsDto
            {
                TotalAppointments = totalAppointments,
                ScheduledAppointments = scheduledAppointments,
                ConfirmedAppointments = confirmedAppointments,
                CompletedAppointments = completedAppointments,
                CanceledAppointments = canceledAppointments,
                TodayAppointments = todayAppointments,
                StatusDistribution = statusDistribution
            };
        }

        public async Task<IEnumerable<AppointmentDto>> GetUpcomingAppointmentsAsync(int days = 7)
        {
            var startDate = DateTime.Today;
            var endDate = startDate.AddDays(days);

            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate)
                .OrderBy(a => a.AppointmentDate)
                .Select(a => new AppointmentDto
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status,
                    Note = a.Note,
                    DoctorId = a.DoctorId,
                    PatientId = a.PatientId,
                    DoctorName = $"{a.Doctor.User.FirstName} {a.Doctor.User.LastName}",
                    PatientName = $"{a.Patient.User.FirstName} {a.Patient.User.LastName}",
                    DoctorSpecialization = a.Doctor.Specialization,
                    CreatedAt = a.AppointmentDate
                })
                .ToListAsync();

            return appointments;
        }

        public async Task<IEnumerable<AppointmentDto>> GetDoctorAppointmentsAsync(int doctorId, DateTime? date = null)
        {
            var query = _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Where(a => a.DoctorId == doctorId);

            if (date.HasValue)
            {
                query = query.Where(a => a.AppointmentDate.Date == date.Value.Date);
            }

            var appointments = await query
                .OrderBy(a => a.AppointmentDate)
                .Select(a => new AppointmentDto
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status,
                    Note = a.Note,
                    DoctorId = a.DoctorId,
                    PatientId = a.PatientId,
                    DoctorName = $"{a.Doctor.User.FirstName} {a.Doctor.User.LastName}",
                    PatientName = $"{a.Patient.User.FirstName} {a.Patient.User.LastName}",
                    DoctorSpecialization = a.Doctor.Specialization,
                    CreatedAt = a.AppointmentDate
                })
                .ToListAsync();

            return appointments;
        }

        public async Task<IEnumerable<AppointmentDto>> GetPatientAppointmentsAsync(int patientId)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new AppointmentDto
                {
                    AppointmentId = a.AppointmentId,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status,
                    Note = a.Note,
                    DoctorId = a.DoctorId,
                    PatientId = a.PatientId,
                    DoctorName = $"{a.Doctor.User.FirstName} {a.Doctor.User.LastName}",
                    PatientName = $"{a.Patient.User.FirstName} {a.Patient.User.LastName}",
                    DoctorSpecialization = a.Doctor.Specialization,
                    CreatedAt = a.AppointmentDate
                })
                .ToListAsync();

            return appointments;
        }

        public async Task<bool> IsTimeSlotAvailableAsync(int doctorId, DateTime dateTime)
        {
            return await IsTimeSlotAvailableAsync(doctorId, dateTime, null);
        }

        private async Task<bool> IsTimeSlotAvailableAsync(int doctorId, DateTime dateTime, int? excludeAppointmentId)
        {
            // Check if there's already an appointment at this time
            var query = _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                           a.AppointmentDate == dateTime &&
                           a.Status != "Canceled");

            if (excludeAppointmentId.HasValue)
            {
                query = query.Where(a => a.AppointmentId != excludeAppointmentId.Value);
            }

            var existingAppointment = await query.FirstOrDefaultAsync();
            return existingAppointment == null;
        }

        private async Task SendAppointmentCreatedNotifications(Appointment appointment, Doctor doctor, Patient patient)
        {
            try
            {
                // Send notification to patient
                var patientMessage = $"Your appointment with Dr. {doctor.User.LastName} has been scheduled for {appointment.AppointmentDate:MMMM dd, yyyy 'at' hh:mm tt}";
                await _notificationService.SendNotificationAsync(patient.UserId, "Appointment Scheduled", patientMessage);

                // Send notification to doctor
                var doctorMessage = $"New appointment scheduled with {patient.User.FirstName} {patient.User.LastName} on {appointment.AppointmentDate:MMMM dd, yyyy 'at' hh:mm tt}";
                await _notificationService.SendNotificationAsync(doctor.UserId, "New Appointment", doctorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send appointment creation notifications");
            }
        }

        private async Task SendAppointmentRescheduledNotification(Appointment appointment, DateTime oldDate)
        {
            try
            {
                var message = $"Your appointment has been rescheduled from {oldDate:MMMM dd, yyyy 'at' hh:mm tt} to {appointment.AppointmentDate:MMMM dd, yyyy 'at' hh:mm tt}";
                await _notificationService.SendNotificationAsync(appointment.PatientId, "Appointment Rescheduled", message);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send appointment reschedule notification");
            }
        }

        private async Task SendAppointmentCancelledNotification(Appointment appointment, string? reason)
        {
            try
            {
                var message = $"Your appointment scheduled for {appointment.AppointmentDate:MMMM dd, yyyy 'at' hh:mm tt} has been canceled";
                if (!string.IsNullOrEmpty(reason))
                {
                    message += $". Reason: {reason}";
                }
                await _notificationService.SendNotificationAsync(appointment.PatientId, "Appointment Canceled", message);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send appointment cancellation notification");
            }
        }
    }
}