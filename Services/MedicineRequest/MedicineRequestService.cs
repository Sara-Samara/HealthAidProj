// Services/Implementations/MedicineRequestService.cs
using AutoMapper;
using Azure.Core;
using HealthAidAPI.Data;
using HealthAidAPI.DTOs.MedicineRequests;
using HealthAidAPI.Helpers;
using HealthAidAPI.Services.MedicineRequest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HealthAidAPI.Services.Implementations
{
    public class MedicineRequestService : IMedicineRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<MedicineRequestService> _logger;

        public MedicineRequestService(ApplicationDbContext context, IMapper mapper, ILogger<MedicineRequestService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<MedicineRequestDto>> GetMedicineRequestsAsync(MedicineRequestFilterDto filter)
        {
            try
            {
                var query = _context.MedicineRequests
                    .Include(mr => mr.Patient)
                    .ThenInclude(p => p.User)
                    .Include(mr => mr.Transactions)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(filter.Search))
                {
                    query = query.Where(mr =>
                        mr.MedicineName.Contains(filter.Search) ||
                        mr.Notes != null && mr.Notes.Contains(filter.Search) ||
                        mr.Patient.User.FirstName.Contains(filter.Search) ||
                        mr.Patient.User.LastName.Contains(filter.Search));
                }

                if (!string.IsNullOrEmpty(filter.MedicineName))
                    query = query.Where(mr => mr.MedicineName.Contains(filter.MedicineName));

                if (!string.IsNullOrEmpty(filter.Status))
                    query = query.Where(mr => mr.Status == filter.Status);

                if (!string.IsNullOrEmpty(filter.Priority))
                    query = query.Where(mr => mr.Priority == filter.Priority);

                if (!string.IsNullOrEmpty(filter.Urgency))
                    query = query.Where(mr => mr.Urgency == filter.Urgency);

                if (filter.PatientId.HasValue)
                    query = query.Where(mr => mr.PatientId == filter.PatientId.Value);

                if (filter.IsUrgent.HasValue)
                    query = query.Where(mr => mr.IsUrgent == filter.IsUrgent.Value);

                if (filter.RequestDateFrom.HasValue)
                    query = query.Where(mr => mr.RequestDate >= filter.RequestDateFrom.Value);

                if (filter.RequestDateTo.HasValue)
                    query = query.Where(mr => mr.RequestDate <= filter.RequestDateTo.Value);

                if (filter.RequiredByDateFrom.HasValue)
                    query = query.Where(mr => mr.RequiredByDate >= filter.RequiredByDateFrom.Value);

                if (filter.RequiredByDateTo.HasValue)
                    query = query.Where(mr => mr.RequiredByDate <= filter.RequiredByDateTo.Value);

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "priority" => filter.SortDesc ?
                        query.OrderByDescending(mr => mr.Priority) : query.OrderBy(mr => mr.Priority),
                    "status" => filter.SortDesc ?
                        query.OrderByDescending(mr => mr.Status) : query.OrderBy(mr => mr.Status),
                    "requestdate" => filter.SortDesc ?
                        query.OrderByDescending(mr => mr.RequestDate) : query.OrderBy(mr => mr.RequestDate),
                    "requiredbydate" => filter.SortDesc ?
                        query.OrderByDescending(mr => mr.RequiredByDate) : query.OrderBy(mr => mr.RequiredByDate),
                    "medicine" => filter.SortDesc ?
                        query.OrderByDescending(mr => mr.MedicineName) : query.OrderBy(mr => mr.MedicineName),
                    "patient" => filter.SortDesc ?
                        query.OrderByDescending(mr => mr.Patient.User.LastName) : query.OrderBy(mr => mr.Patient.User.LastName),
                    _ => filter.SortDesc ?
                        query.OrderByDescending(mr => mr.MedicineRequestId) : query.OrderBy(mr => mr.MedicineRequestId)
                };

                var totalCount = await query.CountAsync();
                var medicineRequests = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(mr => new MedicineRequestDto
                    {
                        MedicineRequestId = mr.MedicineRequestId,
                        MedicineName = mr.MedicineName,
                        Quantity = mr.Quantity,
                        Dosage = mr.Dosage,
                        Priority = mr.Priority,
                        Status = mr.Status,
                        Notes = mr.Notes,
                        PreferredPharmacy = mr.PreferredPharmacy,
                        Urgency = mr.Urgency,
                        RequiredByDate = mr.RequiredByDate,
                        FulfilledDate = mr.FulfilledDate,
                        PatientId = mr.PatientId,
                        PatientName = $"{mr.Patient.User.FirstName} {mr.Patient.User.LastName}",
                        RequestDate = mr.RequestDate,
                        UpdatedAt = mr.UpdatedAt,
                        IsUrgent = mr.IsUrgent,
                        DaysPending = mr.DaysPending,
                        TransactionCount = mr.Transactions.Count
                    })
                    .ToListAsync();

                return new PagedResult<MedicineRequestDto>(medicineRequests, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving medicine requests with filter");
                throw;
            }
        }

        public async Task<MedicineRequestDto?> GetMedicineRequestByIdAsync(int id)
        {
            var medicineRequest = await _context.MedicineRequests
                .Include(mr => mr.Patient)
                .ThenInclude(p => p.User)
                .Include(mr => mr.Transactions)
                .FirstOrDefaultAsync(mr => mr.MedicineRequestId == id);

            if (medicineRequest == null) return null;

            return new MedicineRequestDto
            {
                MedicineRequestId = medicineRequest.MedicineRequestId,
                MedicineName = medicineRequest.MedicineName,
                Quantity = medicineRequest.Quantity,
                Dosage = medicineRequest.Dosage,
                Priority = medicineRequest.Priority,
                Status = medicineRequest.Status,
                Notes = medicineRequest.Notes,
                PreferredPharmacy = medicineRequest.PreferredPharmacy,
                Urgency = medicineRequest.Urgency,
                RequiredByDate = medicineRequest.RequiredByDate,
                FulfilledDate = medicineRequest.FulfilledDate,
                PatientId = medicineRequest.PatientId,
                PatientName = $"{medicineRequest.Patient.User.FirstName} {medicineRequest.Patient.User.LastName}",
                RequestDate = medicineRequest.RequestDate,
                UpdatedAt = medicineRequest.UpdatedAt,
                IsUrgent = medicineRequest.IsUrgent,
                DaysPending = medicineRequest.DaysPending,
                TransactionCount = medicineRequest.Transactions.Count
            };
        }

        public async Task<MedicineRequestDto> CreateMedicineRequestAsync(CreateMedicineRequestDto createMedicineRequestDto)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PatientId == createMedicineRequestDto.PatientId);

            if (patient == null)
                throw new ArgumentException($"Patient with ID {createMedicineRequestDto.PatientId} not found");

            var medicineRequest = new HealthAidAPI.Models.MedicineRequest
            {
                MedicineName = createMedicineRequestDto.MedicineName,
                Quantity = createMedicineRequestDto.Quantity,
                Dosage = createMedicineRequestDto.Dosage,
                Priority = createMedicineRequestDto.Priority,
                Status = "Pending",
                Notes = createMedicineRequestDto.Notes,
                PreferredPharmacy = createMedicineRequestDto.PreferredPharmacy,
                Urgency = createMedicineRequestDto.Urgency,
                RequiredByDate = createMedicineRequestDto.RequiredByDate,
                PatientId = createMedicineRequestDto.PatientId,
                RequestDate = DateTime.UtcNow
            };

            _context.MedicineRequests.Add(medicineRequest);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Medicine request created for patient {PatientName}: {MedicineName}",
                $"{patient.User.FirstName} {patient.User.LastName}", medicineRequest.MedicineName);

            return new MedicineRequestDto
            {
                MedicineRequestId = medicineRequest.MedicineRequestId,
                MedicineName = medicineRequest.MedicineName,
                Quantity = medicineRequest.Quantity,
                Dosage = medicineRequest.Dosage,
                Priority = medicineRequest.Priority,
                Status = medicineRequest.Status,
                Notes = medicineRequest.Notes,
                PreferredPharmacy = medicineRequest.PreferredPharmacy,
                Urgency = medicineRequest.Urgency,
                RequiredByDate = medicineRequest.RequiredByDate,
                PatientId = medicineRequest.PatientId,
                PatientName = $"{patient.User.FirstName} {patient.User.LastName}",
                RequestDate = medicineRequest.RequestDate,
                IsUrgent = medicineRequest.IsUrgent,
                DaysPending = medicineRequest.DaysPending,
                TransactionCount = 0
            };
        }

        public async Task<MedicineRequestDto?> UpdateMedicineRequestAsync(int id, UpdateMedicineRequestDto updateMedicineRequestDto)
        {
            var medicineRequest = await _context.MedicineRequests
                .Include(mr => mr.Patient)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(mr => mr.MedicineRequestId == id);

            if (medicineRequest == null) return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateMedicineRequestDto.MedicineName))
                medicineRequest.MedicineName = updateMedicineRequestDto.MedicineName;

            if (updateMedicineRequestDto.Quantity.HasValue)
                medicineRequest.Quantity = updateMedicineRequestDto.Quantity.Value;

            if (!string.IsNullOrEmpty(updateMedicineRequestDto.Dosage))
                medicineRequest.Dosage = updateMedicineRequestDto.Dosage;

            if (!string.IsNullOrEmpty(updateMedicineRequestDto.Priority))
                medicineRequest.Priority = updateMedicineRequestDto.Priority;

            if (!string.IsNullOrEmpty(updateMedicineRequestDto.Status))
                medicineRequest.Status = updateMedicineRequestDto.Status;

            if (updateMedicineRequestDto.Notes != null)
                medicineRequest.Notes = updateMedicineRequestDto.Notes;

            if (updateMedicineRequestDto.PreferredPharmacy != null)
                medicineRequest.PreferredPharmacy = updateMedicineRequestDto.PreferredPharmacy;

            if (!string.IsNullOrEmpty(updateMedicineRequestDto.Urgency))
                medicineRequest.Urgency = updateMedicineRequestDto.Urgency;

            if (updateMedicineRequestDto.RequiredByDate.HasValue)
                medicineRequest.RequiredByDate = updateMedicineRequestDto.RequiredByDate.Value;

            if (updateMedicineRequestDto.FulfilledDate.HasValue)
                medicineRequest.FulfilledDate = updateMedicineRequestDto.FulfilledDate.Value;

            medicineRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Medicine request {MedicineRequestId} updated", id);

            return new MedicineRequestDto
            {
                MedicineRequestId = medicineRequest.MedicineRequestId,
                MedicineName = medicineRequest.MedicineName,
                Quantity = medicineRequest.Quantity,
                Dosage = medicineRequest.Dosage,
                Priority = medicineRequest.Priority,
                Status = medicineRequest.Status,
                Notes = medicineRequest.Notes,
                PreferredPharmacy = medicineRequest.PreferredPharmacy,
                Urgency = medicineRequest.Urgency,
                RequiredByDate = medicineRequest.RequiredByDate,
                FulfilledDate = medicineRequest.FulfilledDate,
                PatientId = medicineRequest.PatientId,
                PatientName = $"{medicineRequest.Patient.User.FirstName} {medicineRequest.Patient.User.LastName}",
                RequestDate = medicineRequest.RequestDate,
                UpdatedAt = medicineRequest.UpdatedAt,
                IsUrgent = medicineRequest.IsUrgent,
                DaysPending = medicineRequest.DaysPending,
                TransactionCount = medicineRequest.Transactions.Count
            };
        }

        public async Task<bool> DeleteMedicineRequestAsync(int id)
        {
            var medicineRequest = await _context.MedicineRequests.FindAsync(id);
            if (medicineRequest == null) return false;

            _context.MedicineRequests.Remove(medicineRequest);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Medicine request {MedicineRequestId} deleted", id);
            return true;
        }

        public async Task<MedicineRequestDto?> UpdateStatusAsync(int id, UpdateMedicineRequestStatusDto updateStatusDto)
        {
            var medicineRequest = await _context.MedicineRequests
                .Include(mr => mr.Patient)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(mr => mr.MedicineRequestId == id);

            if (medicineRequest == null) return null;

            medicineRequest.Status = updateStatusDto.Status;

            if (!string.IsNullOrEmpty(updateStatusDto.Notes))
                medicineRequest.Notes += $"\nStatus Update: {updateStatusDto.Notes}";

            if (updateStatusDto.Status == "Fulfilled")
                medicineRequest.FulfilledDate = DateTime.UtcNow;

            medicineRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Medicine request {MedicineRequestId} status updated to {Status}",
                id, updateStatusDto.Status);

            return new MedicineRequestDto
            {
                MedicineRequestId = medicineRequest.MedicineRequestId,
                MedicineName = medicineRequest.MedicineName,
                Quantity = medicineRequest.Quantity,
                Dosage = medicineRequest.Dosage,
                Priority = medicineRequest.Priority,
                Status = medicineRequest.Status,
                Notes = medicineRequest.Notes,
                PreferredPharmacy = medicineRequest.PreferredPharmacy,
                Urgency = medicineRequest.Urgency,
                RequiredByDate = medicineRequest.RequiredByDate,
                FulfilledDate = medicineRequest.FulfilledDate,
                PatientId = medicineRequest.PatientId,
                PatientName = $"{medicineRequest.Patient.User.FirstName} {medicineRequest.Patient.User.LastName}",
                RequestDate = medicineRequest.RequestDate,
                UpdatedAt = medicineRequest.UpdatedAt,
                IsUrgent = medicineRequest.IsUrgent,
                DaysPending = medicineRequest.DaysPending,
                TransactionCount = medicineRequest.Transactions.Count
            };
        }

        public async Task<MedicineRequestStatsDto> GetMedicineRequestStatsAsync()
        {
            var medicineRequests = await _context.MedicineRequests.ToListAsync();

            var statusCount = await _context.MedicineRequests
                .GroupBy(mr => mr.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            var priorityCount = await _context.MedicineRequests
                .GroupBy(mr => mr.Priority)
                .Select(g => new { Priority = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Priority, x => x.Count);

            var averageDaysPending = medicineRequests.Any() ?
                (int)medicineRequests.Average(mr => mr.DaysPending) : 0;

            var overdueRequests = medicineRequests.Count(mr =>
                mr.RequiredByDate.HasValue &&
                mr.RequiredByDate.Value < DateTime.UtcNow &&
                mr.Status != "Fulfilled" &&
                mr.Status != "Cancelled");

            return new MedicineRequestStatsDto
            {
                TotalRequests = medicineRequests.Count,
                PendingRequests = medicineRequests.Count(mr => mr.Status == "Pending"),
                ApprovedRequests = medicineRequests.Count(mr => mr.Status == "Approved"),
                InProgressRequests = medicineRequests.Count(mr => mr.Status == "InProgress"),
                FulfilledRequests = medicineRequests.Count(mr => mr.Status == "Fulfilled"),
                CancelledRequests = medicineRequests.Count(mr => mr.Status == "Cancelled"),
                UrgentRequests = medicineRequests.Count(mr => mr.Priority == "High"),
                EmergencyRequests = medicineRequests.Count(mr => mr.Priority == "Emergency"),
                StatusCount = statusCount,
                PriorityCount = priorityCount,
                AverageDaysPending = averageDaysPending,
                OverdueRequests = overdueRequests
            };
        }

        public async Task<IEnumerable<MedicineRequestDto>> GetUrgentMedicineRequestsAsync()
        {
            var medicineRequests = await _context.MedicineRequests
                .Include(mr => mr.Patient)
                .ThenInclude(p => p.User)
                .Where(mr => mr.IsUrgent && mr.Status != "Fulfilled" && mr.Status != "Cancelled")
                .OrderByDescending(mr => mr.Priority)
                .ThenBy(mr => mr.RequiredByDate)
                .Select(mr => new MedicineRequestDto
                {
                    MedicineRequestId = mr.MedicineRequestId,
                    MedicineName = mr.MedicineName,
                    Quantity = mr.Quantity,
                    Dosage = mr.Dosage,
                    Priority = mr.Priority,
                    Status = mr.Status,
                    Notes = mr.Notes,
                    PreferredPharmacy = mr.PreferredPharmacy,
                    Urgency = mr.Urgency,
                    RequiredByDate = mr.RequiredByDate,
                    FulfilledDate = mr.FulfilledDate,
                    PatientId = mr.PatientId,
                    PatientName = $"{mr.Patient.User.FirstName} {mr.Patient.User.LastName}",
                    RequestDate = mr.RequestDate,
                    UpdatedAt = mr.UpdatedAt,
                    IsUrgent = mr.IsUrgent,
                    DaysPending = mr.DaysPending,
                    TransactionCount = mr.Transactions.Count
                })
                .ToListAsync();

            return medicineRequests;
        }

        public async Task<IEnumerable<MedicineRequestDto>> GetOverdueMedicineRequestsAsync()
        {
            var medicineRequests = await _context.MedicineRequests
                .Include(mr => mr.Patient)
                .ThenInclude(p => p.User)
                .Where(mr => mr.RequiredByDate.HasValue &&
                           mr.RequiredByDate.Value < DateTime.UtcNow &&
                           mr.Status != "Fulfilled" &&
                           mr.Status != "Cancelled")
                .OrderBy(mr => mr.RequiredByDate)
                .Select(mr => new MedicineRequestDto
                {
                    MedicineRequestId = mr.MedicineRequestId,
                    MedicineName = mr.MedicineName,
                    Quantity = mr.Quantity,
                    Dosage = mr.Dosage,
                    Priority = mr.Priority,
                    Status = mr.Status,
                    Notes = mr.Notes,
                    PreferredPharmacy = mr.PreferredPharmacy,
                    Urgency = mr.Urgency,
                    RequiredByDate = mr.RequiredByDate,
                    FulfilledDate = mr.FulfilledDate,
                    PatientId = mr.PatientId,
                    PatientName = $"{mr.Patient.User.FirstName} {mr.Patient.User.LastName}",
                    RequestDate = mr.RequestDate,
                    UpdatedAt = mr.UpdatedAt,
                    IsUrgent = mr.IsUrgent,
                    DaysPending = mr.DaysPending,
                    TransactionCount = mr.Transactions.Count
                })
                .ToListAsync();

            return medicineRequests;
        }

        public async Task<IEnumerable<MedicineRequestDto>> GetMedicineRequestsByPatientAsync(int patientId)
        {
            var medicineRequests = await _context.MedicineRequests
                .Include(mr => mr.Patient)
                .ThenInclude(p => p.User)
                .Where(mr => mr.PatientId == patientId)
                .OrderByDescending(mr => mr.RequestDate)
                .Select(mr => new MedicineRequestDto
                {
                    MedicineRequestId = mr.MedicineRequestId,
                    MedicineName = mr.MedicineName,
                    Quantity = mr.Quantity,
                    Dosage = mr.Dosage,
                    Priority = mr.Priority,
                    Status = mr.Status,
                    Notes = mr.Notes,
                    PreferredPharmacy = mr.PreferredPharmacy,
                    Urgency = mr.Urgency,
                    RequiredByDate = mr.RequiredByDate,
                    FulfilledDate = mr.FulfilledDate,
                    PatientId = mr.PatientId,
                    PatientName = $"{mr.Patient.User.FirstName} {mr.Patient.User.LastName}",
                    RequestDate = mr.RequestDate,
                    UpdatedAt = mr.UpdatedAt,
                    IsUrgent = mr.IsUrgent,
                    DaysPending = mr.DaysPending,
                    TransactionCount = mr.Transactions.Count
                })
                .ToListAsync();

            return medicineRequests;
        }

        public async Task<bool> FulfillMedicineRequestAsync(int id)
        {
            var medicineRequest = await _context.MedicineRequests.FindAsync(id);
            if (medicineRequest == null) return false;

            medicineRequest.Status = "Fulfilled";
            medicineRequest.FulfilledDate = DateTime.UtcNow;
            medicineRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Medicine request {MedicineRequestId} fulfilled", id);
            return true;
        }

        public async Task<bool> CancelMedicineRequestAsync(int id)
        {
            var medicineRequest = await _context.MedicineRequests.FindAsync(id);
            if (medicineRequest == null) return false;

            medicineRequest.Status = "Cancelled";
            medicineRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Medicine request {MedicineRequestId} cancelled", id);
            return true;
        }
    }
}