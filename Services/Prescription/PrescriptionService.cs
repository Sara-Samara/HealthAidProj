// Services/Implementations/PrescriptionService.cs
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using HealthAidAPI.Data;
using HealthAidAPI.Helpers;
using HealthAidAPI.DTOs.Prescriptions;
using HealthAidAPI.Services.Interfaces;
using HealthAidAPI.Models;

namespace HealthAidAPI.Services.Implementations
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PrescriptionService> _logger;

        public PrescriptionService(ApplicationDbContext context, IMapper mapper, ILogger<PrescriptionService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<PrescriptionDto>> GetPrescriptionsAsync(PrescriptionFilterDto filter)
        {
            try
            {
                var query = _context.Prescriptions
                    .Include(p => p.Consultation)
                        .ThenInclude(c => c!.Doctor)
                            .ThenInclude(d => d!.User)
                    .Include(p => p.Consultation)
                        .ThenInclude(c => c!.Patient)
                            .ThenInclude(p => p!.User)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(filter.MedicineName))
                    query = query.Where(p => p.MedicineName.Contains(filter.MedicineName));

                if (filter.ConsultationId.HasValue)
                    query = query.Where(p => p.ConsultationId == filter.ConsultationId.Value);

                if (filter.PatientId.HasValue)
                    query = query.Where(p => p.Consultation != null && p.Consultation.PatientId == filter.PatientId.Value);

                if (filter.DoctorId.HasValue)
                    query = query.Where(p => p.Consultation != null && p.Consultation.DoctorId == filter.DoctorId.Value);

                if (!string.IsNullOrEmpty(filter.Status))
                    query = query.Where(p => p.Status == filter.Status);

                if (filter.IsCompleted.HasValue)
                    query = query.Where(p => p.IsCompleted == filter.IsCompleted.Value);

                if (filter.CreatedFrom.HasValue)
                    query = query.Where(p => p.CreatedAt >= filter.CreatedFrom.Value);

                if (filter.CreatedTo.HasValue)
                    query = query.Where(p => p.CreatedAt <= filter.CreatedTo.Value);

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "medicinename" => filter.SortDesc ?
                        query.OrderByDescending(p => p.MedicineName) : query.OrderBy(p => p.MedicineName),
                    "patient" => filter.SortDesc ?
                        query.OrderByDescending(p => p.Consultation!.Patient!.User!.FirstName) :
                        query.OrderBy(p => p.Consultation!.Patient!.User!.FirstName),
                    "doctor" => filter.SortDesc ?
                        query.OrderByDescending(p => p.Consultation!.Doctor!.User!.FirstName) :
                        query.OrderBy(p => p.Consultation!.Doctor!.User!.FirstName),
                    "createdat" => filter.SortDesc ?
                        query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
                    "status" => filter.SortDesc ?
                        query.OrderByDescending(p => p.Status) : query.OrderBy(p => p.Status),
                    _ => filter.SortDesc ?
                        query.OrderByDescending(p => p.PrescriptionId) : query.OrderBy(p => p.PrescriptionId)
                };

                var totalCount = await query.CountAsync();
                var prescriptions = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(p => new PrescriptionDto
                    {
                        PrescriptionId = p.PrescriptionId,
                        MedicineName = p.MedicineName,
                        Dosages = p.Dosages,
                        Duration = p.Duration,
                        ConsultationId = p.ConsultationId,
                        ConsultationNotes = p.Consultation!.Notes,
                        ConsultationDate = p.Consultation.Date,
                        DoctorName = p.Consultation.Doctor != null && p.Consultation.Doctor.User != null ?
                            $"{p.Consultation.Doctor.User.FirstName} {p.Consultation.Doctor.User.LastName}" : "Unknown Doctor",
                        PatientName = p.Consultation.Patient != null && p.Consultation.Patient.User != null ?
                            $"{p.Consultation.Patient.User.FirstName} {p.Consultation.Patient.User.LastName}" : "Unknown Patient",
                        DoctorId = p.Consultation.DoctorId,
                        PatientId = p.Consultation.PatientId,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        Status = p.Status,
                        Instructions = p.Instructions,
                        RefillsRemaining = p.RefillsRemaining,
                        IsCompleted = p.IsCompleted
                    })
                    .ToListAsync();

                  return new PagedResult<PrescriptionDto>(prescriptions, totalCount, filter.Page, filter.PageSize);  
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving prescriptions with filter");
                throw;
            }
        }

        public async Task<PrescriptionDto?> GetPrescriptionByIdAsync(int id)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Doctor)
                        .ThenInclude(d => d!.User)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Patient)
                        .ThenInclude(p => p!.User)
                .FirstOrDefaultAsync(p => p.PrescriptionId == id);

            if (prescription == null) return null;

            return new PrescriptionDto
            {
                PrescriptionId = prescription.PrescriptionId,
                MedicineName = prescription.MedicineName,
                Dosages = prescription.Dosages,
                Duration = prescription.Duration,
                ConsultationId = prescription.ConsultationId,
                ConsultationNotes = prescription.Consultation!.Notes,
                ConsultationDate = prescription.Consultation.Date,
                DoctorName = prescription.Consultation.Doctor != null && prescription.Consultation.Doctor.User != null ?
                    $"{prescription.Consultation.Doctor.User.FirstName} {prescription.Consultation.Doctor.User.LastName}" : "Unknown Doctor",
                PatientName = prescription.Consultation.Patient != null && prescription.Consultation.Patient.User != null ?
                    $"{prescription.Consultation.Patient.User.FirstName} {prescription.Consultation.Patient.User.LastName}" : "Unknown Patient",
                DoctorId = prescription.Consultation.DoctorId,
                PatientId = prescription.Consultation.PatientId,
                CreatedAt = prescription.CreatedAt,
                UpdatedAt = prescription.UpdatedAt,
                Status = prescription.Status,
                Instructions = prescription.Instructions,
                RefillsRemaining = prescription.RefillsRemaining,
                IsCompleted = prescription.IsCompleted
            };
        }

        public async Task<PrescriptionDto> CreatePrescriptionAsync(CreatePrescriptionDto prescriptionDto)
        {
            // Validate consultation exists
            var consultation = await _context.Consultations
                .Include(c => c.Doctor)
                    .ThenInclude(d => d!.User)
                .Include(c => c.Patient)
                    .ThenInclude(p => p!.User)
                .FirstOrDefaultAsync(c => c.ConsultationId == prescriptionDto.ConsultationId);

            if (consultation == null)
                throw new ArgumentException($"Consultation with ID {prescriptionDto.ConsultationId} not found");

            var prescription = new Prescription
            {
                MedicineName = prescriptionDto.MedicineName,
                Dosages = prescriptionDto.Dosages,
                Duration = prescriptionDto.Duration,
                ConsultationId = prescriptionDto.ConsultationId,
                Instructions = prescriptionDto.Instructions,
                RefillsRemaining = prescriptionDto.RefillsRemaining ?? 0,
                Status = prescriptionDto.Status,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Prescription created: {MedicineName} for consultation {ConsultationId}",
                prescriptionDto.MedicineName, prescriptionDto.ConsultationId);

            return await GetPrescriptionByIdAsync(prescription.PrescriptionId) ??
                throw new InvalidOperationException("Failed to retrieve created prescription");
        }

        public async Task<PrescriptionDto?> UpdatePrescriptionAsync(int id, UpdatePrescriptionDto prescriptionDto)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Doctor)
                        .ThenInclude(d => d!.User)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Patient)
                        .ThenInclude(p => p!.User)
                .FirstOrDefaultAsync(p => p.PrescriptionId == id);

            if (prescription == null) return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(prescriptionDto.MedicineName))
                prescription.MedicineName = prescriptionDto.MedicineName;

            if (!string.IsNullOrEmpty(prescriptionDto.Dosages))
                prescription.Dosages = prescriptionDto.Dosages;

            if (!string.IsNullOrEmpty(prescriptionDto.Duration))
                prescription.Duration = prescriptionDto.Duration;

            if (prescriptionDto.Instructions != null)
                prescription.Instructions = prescriptionDto.Instructions;

            if (prescriptionDto.RefillsRemaining.HasValue)
                prescription.RefillsRemaining = prescriptionDto.RefillsRemaining.Value;

            if (!string.IsNullOrEmpty(prescriptionDto.Status))
                prescription.Status = prescriptionDto.Status;

            if (prescriptionDto.IsCompleted.HasValue)
                prescription.IsCompleted = prescriptionDto.IsCompleted.Value;

            prescription.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Prescription updated: {PrescriptionId}", id);

            return new PrescriptionDto
            {
                PrescriptionId = prescription.PrescriptionId,
                MedicineName = prescription.MedicineName,
                Dosages = prescription.Dosages,
                Duration = prescription.Duration,
                ConsultationId = prescription.ConsultationId,
                ConsultationNotes = prescription.Consultation!.Notes,
                ConsultationDate = prescription.Consultation.Date,
                DoctorName = prescription.Consultation.Doctor != null && prescription.Consultation.Doctor.User != null ?
                    $"{prescription.Consultation.Doctor.User.FirstName} {prescription.Consultation.Doctor.User.LastName}" : "Unknown Doctor",
                PatientName = prescription.Consultation.Patient != null && prescription.Consultation.Patient.User != null ?
                    $"{prescription.Consultation.Patient.User.FirstName} {prescription.Consultation.Patient.User.LastName}" : "Unknown Patient",
                DoctorId = prescription.Consultation.DoctorId,
                PatientId = prescription.Consultation.PatientId,
                CreatedAt = prescription.CreatedAt,
                UpdatedAt = prescription.UpdatedAt,
                Status = prescription.Status,
                Instructions = prescription.Instructions,
                RefillsRemaining = prescription.RefillsRemaining,
                IsCompleted = prescription.IsCompleted
            };
        }

        public async Task<bool> DeletePrescriptionAsync(int id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null) return false;

            _context.Prescriptions.Remove(prescription);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Prescription deleted: {PrescriptionId}", id);
            return true;
        }

        public async Task<bool> CompletePrescriptionAsync(int id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null) return false;

            prescription.Status = "Completed";
            prescription.IsCompleted = true;
            prescription.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Prescription completed: {PrescriptionId}", id);
            return true;
        }

        public async Task<bool> RequestRefillAsync(RefillRequestDto refillRequest)
        {
            var prescription = await _context.Prescriptions.FindAsync(refillRequest.PrescriptionId);
            if (prescription == null) return false;

            if (prescription.RefillsRemaining <= 0)
                throw new InvalidOperationException("No refills remaining for this prescription");

            // In a real application, you would create a refill request record
            // For now, we'll just log the request
            _logger.LogInformation("Refill requested for prescription {PrescriptionId}: {Reason} (Quantity: {Quantity})",
                refillRequest.PrescriptionId, refillRequest.Reason, refillRequest.Quantity);

            return true;
        }

        public async Task<bool> ApproveRefillAsync(int prescriptionId, int quantity)
        {
            var prescription = await _context.Prescriptions.FindAsync(prescriptionId);
            if (prescription == null) return false;

            if (prescription.RefillsRemaining < quantity)
                throw new InvalidOperationException("Not enough refills remaining");

            prescription.RefillsRemaining -= quantity;
            prescription.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Refill approved for prescription {PrescriptionId}: {Quantity} refills granted",
                prescriptionId, quantity);

            return true;
        }

        public async Task<PrescriptionStatsDto> GetPrescriptionStatsAsync()
        {
            var totalPrescriptions = await _context.Prescriptions.CountAsync();
            var activePrescriptions = await _context.Prescriptions.CountAsync(p => p.Status == "Active");
            var completedPrescriptions = await _context.Prescriptions.CountAsync(p => p.Status == "Completed");
            var expiredPrescriptions = await _context.Prescriptions.CountAsync(p => p.Status == "Expired");

            var medicineFrequency = await _context.Prescriptions
                .GroupBy(p => p.MedicineName)
                .Select(g => new { MedicineName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToDictionaryAsync(x => x.MedicineName, x => x.Count);

            var statusDistribution = await _context.Prescriptions
                .GroupBy(p => p.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            var totalPatients = await _context.Prescriptions
                .Select(p => p.Consultation!.PatientId)
                .Distinct()
                .CountAsync();

            var totalDoctors = await _context.Prescriptions
                .Select(p => p.Consultation!.DoctorId)
                .Distinct()
                .CountAsync();

            var recentPrescriptions = await _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Doctor)
                        .ThenInclude(d => d!.User)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Patient)
                        .ThenInclude(p => p!.User)
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new RecentPrescriptionDto
                {
                    PrescriptionId = p.PrescriptionId,
                    MedicineName = p.MedicineName,
                    PatientName = p.Consultation!.Patient != null && p.Consultation.Patient.User != null ?
                        $"{p.Consultation.Patient.User.FirstName} {p.Consultation.Patient.User.LastName}" : "Unknown Patient",
                    DoctorName = p.Consultation.Doctor != null && p.Consultation.Doctor.User != null ?
                        $"{p.Consultation.Doctor.User.FirstName} {p.Consultation.Doctor.User.LastName}" : "Unknown Doctor",
                    CreatedAt = p.CreatedAt,
                    Status = p.Status
                })
                .ToListAsync();

            return new PrescriptionStatsDto
            {
                TotalPrescriptions = totalPrescriptions,
                ActivePrescriptions = activePrescriptions,
                CompletedPrescriptions = completedPrescriptions,
                ExpiredPrescriptions = expiredPrescriptions,
                MedicineFrequency = medicineFrequency,
                StatusDistribution = statusDistribution,
                TotalPatients = totalPatients,
                TotalDoctors = totalDoctors,
                RecentPrescriptions = recentPrescriptions
            };
        }

        public async Task<IEnumerable<PrescriptionDto>> GetPatientPrescriptionsAsync(int patientId)
        {
            var prescriptions = await _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Doctor)
                        .ThenInclude(d => d!.User)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Patient)
                        .ThenInclude(p => p!.User)
                .Where(p => p.Consultation != null && p.Consultation.PatientId == patientId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PrescriptionDto
                {
                    PrescriptionId = p.PrescriptionId,
                    MedicineName = p.MedicineName,
                    Dosages = p.Dosages,
                    Duration = p.Duration,
                    ConsultationId = p.ConsultationId,
                    ConsultationNotes = p.Consultation!.Notes,
                    ConsultationDate = p.Consultation.Date,
                    DoctorName = p.Consultation.Doctor != null && p.Consultation.Doctor.User != null ?
                        $"{p.Consultation.Doctor.User.FirstName} {p.Consultation.Doctor.User.LastName}" : "Unknown Doctor",
                    PatientName = p.Consultation.Patient != null && p.Consultation.Patient.User != null ?
                        $"{p.Consultation.Patient.User.FirstName} {p.Consultation.Patient.User.LastName}" : "Unknown Patient",
                    DoctorId = p.Consultation.DoctorId,
                    PatientId = p.Consultation.PatientId,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Status = p.Status,
                    Instructions = p.Instructions,
                    RefillsRemaining = p.RefillsRemaining,
                    IsCompleted = p.IsCompleted
                })
                .ToListAsync();

            return prescriptions;
        }

        public async Task<IEnumerable<PrescriptionDto>> GetDoctorPrescriptionsAsync(int doctorId)
        {
            var prescriptions = await _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Doctor)
                        .ThenInclude(d => d!.User)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Patient)
                        .ThenInclude(p => p!.User)
                .Where(p => p.Consultation != null && p.Consultation.DoctorId == doctorId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PrescriptionDto
                {
                    PrescriptionId = p.PrescriptionId,
                    MedicineName = p.MedicineName,
                    Dosages = p.Dosages,
                    Duration = p.Duration,
                    ConsultationId = p.ConsultationId,
                    ConsultationNotes = p.Consultation!.Notes,
                    ConsultationDate = p.Consultation.Date,
                    DoctorName = p.Consultation.Doctor != null && p.Consultation.Doctor.User != null ?
                        $"{p.Consultation.Doctor.User.FirstName} {p.Consultation.Doctor.User.LastName}" : "Unknown Doctor",
                    PatientName = p.Consultation.Patient != null && p.Consultation.Patient.User != null ?
                        $"{p.Consultation.Patient.User.FirstName} {p.Consultation.Patient.User.LastName}" : "Unknown Patient",
                    DoctorId = p.Consultation.DoctorId,
                    PatientId = p.Consultation.PatientId,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Status = p.Status,
                    Instructions = p.Instructions,
                    RefillsRemaining = p.RefillsRemaining,
                    IsCompleted = p.IsCompleted
                })
                .ToListAsync();

            return prescriptions;
        }

        public async Task<PatientPrescriptionSummaryDto> GetPatientPrescriptionSummaryAsync(int patientId)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (patient == null)
                throw new ArgumentException($"Patient with ID {patientId} not found");

            var prescriptions = await GetPatientPrescriptionsAsync(patientId);
            var activePrescriptions = prescriptions.Count(p => p.Status == "Active");

            return new PatientPrescriptionSummaryDto
            {
                PatientId = patientId,
                PatientName = patient.User != null ?
                    $"{patient.User.FirstName} {patient.User.LastName}" : "Unknown Patient",
                TotalPrescriptions = prescriptions.Count(),
                ActivePrescriptions = activePrescriptions,
                RecentPrescriptions = prescriptions.Take(5).ToList()
            };
        }

        public async Task<IEnumerable<PrescriptionDto>> GetExpiringPrescriptionsAsync(int days = 7)
        {
            var expirationDate = DateTime.UtcNow.AddDays(days);

            var prescriptions = await _context.Prescriptions
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Doctor)
                        .ThenInclude(d => d!.User)
                .Include(p => p.Consultation)
                    .ThenInclude(c => c!.Patient)
                        .ThenInclude(p => p!.User)
                .Where(p => p.Status == "Active" && p.CreatedAt.AddDays(30) <= expirationDate) // Assuming 30-day validity
                .OrderBy(p => p.CreatedAt)
                .Select(p => new PrescriptionDto
                {
                    PrescriptionId = p.PrescriptionId,
                    MedicineName = p.MedicineName,
                    Dosages = p.Dosages,
                    Duration = p.Duration,
                    ConsultationId = p.ConsultationId,
                    ConsultationNotes = p.Consultation!.Notes,
                    ConsultationDate = p.Consultation.Date,
                    DoctorName = p.Consultation.Doctor != null && p.Consultation.Doctor.User != null ?
                        $"{p.Consultation.Doctor.User.FirstName} {p.Consultation.Doctor.User.LastName}" : "Unknown Doctor",
                    PatientName = p.Consultation.Patient != null && p.Consultation.Patient.User != null ?
                        $"{p.Consultation.Patient.User.FirstName} {p.Consultation.Patient.User.LastName}" : "Unknown Patient",
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Status = p.Status,
                    Instructions = p.Instructions,
                    RefillsRemaining = p.RefillsRemaining,
                    IsCompleted = p.IsCompleted
                })
                .ToListAsync();

            return prescriptions;
        }

        public async Task<bool> ValidatePrescriptionAsync(int prescriptionId)
        {
            var prescription = await _context.Prescriptions.FindAsync(prescriptionId);
            if (prescription == null) return false;

            // Check if prescription is still valid (assuming 30-day validity)
            var isValid = prescription.Status == "Active" &&
                         prescription.CreatedAt.AddDays(30) >= DateTime.UtcNow &&
                         !prescription.IsCompleted;

            if (!isValid)
            {
                prescription.Status = "Expired";
                await _context.SaveChangesAsync();
            }

            return isValid;
        }
    }
}