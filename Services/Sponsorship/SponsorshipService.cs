// Services/Implementations/SponsorshipService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using HealthAidAPI.Helpers;
using HealthAidAPI.Data;
using HealthAidAPI.DTOs.Sponsorships;
using HealthAidAPI.Services.Interfaces;
using HealthAidAPI.Models;
using HealthAidAPI.Helpers;


namespace HealthAidAPI.Services.Implementations
{
    public class SponsorshipService : ISponsorshipService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<SponsorshipService> _logger;

        public SponsorshipService(ApplicationDbContext context, IMapper mapper, ILogger<SponsorshipService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<SponsorshipDto>> GetSponsorshipsAsync(SponsorshipFilterDto filter)
        {
            try
            {
                var query = _context.Sponsorships
                    .Include(s => s.Patient)
                    .ThenInclude(p => p.User)
                    .Include(s => s.Donations)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(filter.Search))
                {
                    query = query.Where(s =>
                        s.GoalDescription.Contains(filter.Search) ||
                        s.Story != null && s.Story.Contains(filter.Search) ||
                        s.Patient.User.FirstName.Contains(filter.Search) ||
                        s.Patient.User.LastName.Contains(filter.Search));
                }

                if (!string.IsNullOrEmpty(filter.Category))
                    query = query.Where(s => s.Category == filter.Category);

                if (!string.IsNullOrEmpty(filter.Status))
                    query = query.Where(s => s.Status == filter.Status);

                if (filter.PatientId.HasValue)
                    query = query.Where(s => s.PatientId == filter.PatientId.Value);

                if (filter.IsUrgent.HasValue)
                    query = query.Where(s => s.IsUrgent == filter.IsUrgent.Value);

                if (filter.IsFullyFunded.HasValue)
                    query = query.Where(s => s.IsFullyFunded == filter.IsFullyFunded.Value);

                if (filter.MinGoalAmount.HasValue)
                    query = query.Where(s => s.GoalAmount >= filter.MinGoalAmount.Value);

                if (filter.MaxGoalAmount.HasValue)
                    query = query.Where(s => s.GoalAmount <= filter.MaxGoalAmount.Value);

                if (filter.DeadlineBefore.HasValue)
                    query = query.Where(s => s.Deadline <= filter.DeadlineBefore.Value);

                if (filter.CreatedAfter.HasValue)
                    query = query.Where(s => s.CreatedAt >= filter.CreatedAfter.Value);

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "goalamount" => filter.SortDesc ?
                        query.OrderByDescending(s => s.GoalAmount) : query.OrderBy(s => s.GoalAmount),
                    "amountraised" => filter.SortDesc ?
                        query.OrderByDescending(s => s.AmountRaised) : query.OrderBy(s => s.AmountRaised),
                    "progress" => filter.SortDesc ?
                        query.OrderByDescending(s => s.ProgressPercentage) : query.OrderBy(s => s.ProgressPercentage),
                    "deadline" => filter.SortDesc ?
                        query.OrderByDescending(s => s.Deadline) : query.OrderBy(s => s.Deadline),
                    "createdat" => filter.SortDesc ?
                        query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
                    "patient" => filter.SortDesc ?
                        query.OrderByDescending(s => s.Patient.User.LastName) : query.OrderBy(s => s.Patient.User.LastName),
                    _ => filter.SortDesc ?
                        query.OrderByDescending(s => s.SponsorshipId) : query.OrderBy(s => s.SponsorshipId)
                };

                var totalCount = await query.CountAsync();
                var sponsorships = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(s => new SponsorshipDto
                    {
                        SponsorshipId = s.SponsorshipId,
                        GoalDescription = s.GoalDescription,
                        GoalAmount = s.GoalAmount,
                        AmountRaised = s.AmountRaised,
                        Status = s.Status,
                        Category = s.Category,
                        Story = s.Story,
                        ImageUrl = s.ImageUrl,
                        Deadline = s.Deadline,
                        DonorCount = s.DonorCount,
                        PatientId = s.PatientId,
                        PatientName = $"{s.Patient.User.FirstName} {s.Patient.User.LastName}",
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt,
                        ProgressPercentage = s.ProgressPercentage,
                        IsFullyFunded = s.IsFullyFunded,
                        IsUrgent = s.IsUrgent,
                        AmountNeeded = s.AmountNeeded
                    })
                    .ToListAsync();

                return new PagedResult<SponsorshipDto>(sponsorships, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sponsorships with filter");
                throw;
            }
        }

        public async Task<SponsorshipDto?> GetSponsorshipByIdAsync(int id)
        {
            var sponsorship = await _context.Sponsorships
                .Include(s => s.Patient)
                .ThenInclude(p => p.User)
                .Include(s => s.Donations)
                .ThenInclude(d => d.Donor)
                .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(s => s.SponsorshipId == id);

            if (sponsorship == null) return null;

            return new SponsorshipDto
            {
                SponsorshipId = sponsorship.SponsorshipId,
                GoalDescription = sponsorship.GoalDescription,
                GoalAmount = sponsorship.GoalAmount,
                AmountRaised = sponsorship.AmountRaised,
                Status = sponsorship.Status,
                Category = sponsorship.Category,
                Story = sponsorship.Story,
                ImageUrl = sponsorship.ImageUrl,
                Deadline = sponsorship.Deadline,
                DonorCount = sponsorship.DonorCount,
                PatientId = sponsorship.PatientId,
                PatientName = $"{sponsorship.Patient.User.FirstName} {sponsorship.Patient.User.LastName}",
                CreatedAt = sponsorship.CreatedAt,
                UpdatedAt = sponsorship.UpdatedAt,
                ProgressPercentage = sponsorship.ProgressPercentage,
                IsFullyFunded = sponsorship.IsFullyFunded,
                IsUrgent = sponsorship.IsUrgent,
                AmountNeeded = sponsorship.AmountNeeded
            };
        }

        public async Task<SponsorshipDto> CreateSponsorshipAsync(CreateSponsorshipDto createSponsorshipDto)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PatientId == createSponsorshipDto.PatientId);

            if (patient == null)
                throw new ArgumentException($"Patient with ID {createSponsorshipDto.PatientId} not found");

            var sponsorship = new Sponsorship
            {
                GoalDescription = createSponsorshipDto.GoalDescription,
                GoalAmount = createSponsorshipDto.GoalAmount,
                Category = createSponsorshipDto.Category,
                Story = createSponsorshipDto.Story,
                ImageUrl = createSponsorshipDto.ImageUrl,
                Deadline = createSponsorshipDto.Deadline,
                PatientId = createSponsorshipDto.PatientId,
                Status = "Active",
                AmountRaised = 0,
                DonorCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.Sponsorships.Add(sponsorship);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Sponsorship created for patient {PatientName}",
                $"{patient.User.FirstName} {patient.User.LastName}");

            return new SponsorshipDto
            {
                SponsorshipId = sponsorship.SponsorshipId,
                GoalDescription = sponsorship.GoalDescription,
                GoalAmount = sponsorship.GoalAmount,
                AmountRaised = sponsorship.AmountRaised,
                Status = sponsorship.Status,
                Category = sponsorship.Category,
                Story = sponsorship.Story,
                ImageUrl = sponsorship.ImageUrl,
                Deadline = sponsorship.Deadline,
                DonorCount = sponsorship.DonorCount,
                PatientId = sponsorship.PatientId,
                PatientName = $"{patient.User.FirstName} {patient.User.LastName}",
                CreatedAt = sponsorship.CreatedAt,
                ProgressPercentage = sponsorship.ProgressPercentage,
                IsFullyFunded = sponsorship.IsFullyFunded,
                IsUrgent = sponsorship.IsUrgent,
                AmountNeeded = sponsorship.AmountNeeded
            };
        }

        public async Task<SponsorshipDto?> UpdateSponsorshipAsync(int id, UpdateSponsorshipDto updateSponsorshipDto)
        {
            var sponsorship = await _context.Sponsorships
                .Include(s => s.Patient)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(s => s.SponsorshipId == id);

            if (sponsorship == null) return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateSponsorshipDto.GoalDescription))
                sponsorship.GoalDescription = updateSponsorshipDto.GoalDescription;

            if (updateSponsorshipDto.GoalAmount.HasValue)
                sponsorship.GoalAmount = updateSponsorshipDto.GoalAmount.Value;

            if (!string.IsNullOrEmpty(updateSponsorshipDto.Status))
                sponsorship.Status = updateSponsorshipDto.Status;

            if (!string.IsNullOrEmpty(updateSponsorshipDto.Category))
                sponsorship.Category = updateSponsorshipDto.Category;

            if (updateSponsorshipDto.Story != null)
                sponsorship.Story = updateSponsorshipDto.Story;

            if (updateSponsorshipDto.ImageUrl != null)
                sponsorship.ImageUrl = updateSponsorshipDto.ImageUrl;

            if (updateSponsorshipDto.Deadline.HasValue)
                sponsorship.Deadline = updateSponsorshipDto.Deadline.Value;

            sponsorship.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Sponsorship {SponsorshipId} updated", id);

            return new SponsorshipDto
            {
                SponsorshipId = sponsorship.SponsorshipId,
                GoalDescription = sponsorship.GoalDescription,
                GoalAmount = sponsorship.GoalAmount,
                AmountRaised = sponsorship.AmountRaised,
                Status = sponsorship.Status,
                Category = sponsorship.Category,
                Story = sponsorship.Story,
                ImageUrl = sponsorship.ImageUrl,
                Deadline = sponsorship.Deadline,
                DonorCount = sponsorship.DonorCount,
                PatientId = sponsorship.PatientId,
                PatientName = $"{sponsorship.Patient.User.FirstName} {sponsorship.Patient.User.LastName}",
                CreatedAt = sponsorship.CreatedAt,
                UpdatedAt = sponsorship.UpdatedAt,
                ProgressPercentage = sponsorship.ProgressPercentage,
                IsFullyFunded = sponsorship.IsFullyFunded,
                IsUrgent = sponsorship.IsUrgent,
                AmountNeeded = sponsorship.AmountNeeded
            };
        }

        public async Task<bool> DeleteSponsorshipAsync(int id)
        {
            var sponsorship = await _context.Sponsorships.FindAsync(id);
            if (sponsorship == null) return false;

            _context.Sponsorships.Remove(sponsorship);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Sponsorship {SponsorshipId} deleted", id);
            return true;
        }

        public async Task<SponsorshipDto?> AddDonationAsync(int sponsorshipId, DonateToSponsorshipDto donateDto)
        {
            var sponsorship = await _context.Sponsorships
                .Include(s => s.Patient)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(s => s.SponsorshipId == sponsorshipId);

            if (sponsorship == null) return null;

            var donor = await _context.Donors.FindAsync(donateDto.DonorId);
            if (donor == null)
                throw new ArgumentException($"Donor with ID {donateDto.DonorId} not found");

            // Create donation
            var donation = new Donation
            {
                Amount = donateDto.Amount,
                DonationDate = DateTime.UtcNow,
                Message = donateDto.Message,
                PaymentMethod = donateDto.PaymentMethod,
                DonorId = donateDto.DonorId,
                SponsorshipId = sponsorshipId,
                Status = "Completed"
            };

            // Update sponsorship
            sponsorship.AmountRaised += donateDto.Amount;
            sponsorship.DonorCount = await _context.Donations
                .Where(d => d.SponsorshipId == sponsorshipId)
                .Select(d => d.DonorId)
                .Distinct()
                .CountAsync();

            sponsorship.UpdatedAt = DateTime.UtcNow;

            // Update donor total donated
            donor.TotalDonated += donateDto.Amount;

            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Donation of {Amount} added to sponsorship {SponsorshipId} by donor {DonorId}",
                donateDto.Amount, sponsorshipId, donateDto.DonorId);

            return new SponsorshipDto
            {
                SponsorshipId = sponsorship.SponsorshipId,
                GoalDescription = sponsorship.GoalDescription,
                GoalAmount = sponsorship.GoalAmount,
                AmountRaised = sponsorship.AmountRaised,
                Status = sponsorship.Status,
                Category = sponsorship.Category,
                Story = sponsorship.Story,
                ImageUrl = sponsorship.ImageUrl,
                Deadline = sponsorship.Deadline,
                DonorCount = sponsorship.DonorCount,
                PatientId = sponsorship.PatientId,
                PatientName = $"{sponsorship.Patient.User.FirstName} {sponsorship.Patient.User.LastName}",
                CreatedAt = sponsorship.CreatedAt,
                UpdatedAt = sponsorship.UpdatedAt,
                ProgressPercentage = sponsorship.ProgressPercentage,
                IsFullyFunded = sponsorship.IsFullyFunded,
                IsUrgent = sponsorship.IsUrgent,
                AmountNeeded = sponsorship.AmountNeeded
            };
        }

        public async Task<bool> UpdateStatusAsync(int id, string status)
        {
            var sponsorship = await _context.Sponsorships.FindAsync(id);
            if (sponsorship == null) return false;

            sponsorship.Status = status;
            sponsorship.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Sponsorship {SponsorshipId} status updated to {Status}", id, status);
            return true;
        }

        public async Task<SponsorshipStatsDto> GetSponsorshipStatsAsync()
        {
            var sponsorships = await _context.Sponsorships.ToListAsync();
            var donations = await _context.Donations.ToListAsync();

            var categoryCount = await _context.Sponsorships
                .GroupBy(s => s.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Category, x => x.Count);

            var statusCount = await _context.Sponsorships
                .GroupBy(s => s.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            return new SponsorshipStatsDto
            {
                TotalSponsorships = sponsorships.Count,
                ActiveSponsorships = sponsorships.Count(s => s.Status == "Active"),
                CompletedSponsorships = sponsorships.Count(s => s.Status == "Completed"),
                TotalGoalAmount = sponsorships.Sum(s => s.GoalAmount),
                TotalAmountRaised = sponsorships.Sum(s => s.AmountRaised),
                TotalDonations = donations.Sum(d => d.Amount),
                TotalDonors = await _context.Donations.Select(d => d.DonorId).Distinct().CountAsync(),
                CategoryCount = categoryCount,
                StatusCount = statusCount,
                UrgentSponsorships = sponsorships.Count(s => s.IsUrgent)
            };
        }

        public async Task<IEnumerable<SponsorshipDto>> GetUrgentSponsorshipsAsync()
        {
            var sponsorships = await _context.Sponsorships
                .Include(s => s.Patient)
                .ThenInclude(p => p.User)
                .Where(s => s.IsUrgent && s.Status == "Active")
                .OrderBy(s => s.Deadline)
                .Select(s => new SponsorshipDto
                {
                    SponsorshipId = s.SponsorshipId,
                    GoalDescription = s.GoalDescription,
                    GoalAmount = s.GoalAmount,
                    AmountRaised = s.AmountRaised,
                    Status = s.Status,
                    Category = s.Category,
                    Story = s.Story,
                    ImageUrl = s.ImageUrl,
                    Deadline = s.Deadline,
                    DonorCount = s.DonorCount,
                    PatientId = s.PatientId,
                    PatientName = $"{s.Patient.User.FirstName} {s.Patient.User.LastName}",
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    ProgressPercentage = s.ProgressPercentage,
                    IsFullyFunded = s.IsFullyFunded,
                    IsUrgent = s.IsUrgent,
                    AmountNeeded = s.AmountNeeded
                })
                .ToListAsync();

            return sponsorships;
        }

        public async Task<IEnumerable<SponsorshipDto>> GetFeaturedSponsorshipsAsync(int count = 5)
        {
            var sponsorships = await _context.Sponsorships
                .Include(s => s.Patient)
                .ThenInclude(p => p.User)
                .Where(s => s.Status == "Active")
                .OrderByDescending(s => s.ProgressPercentage)
                .ThenBy(s => s.Deadline)
                .Take(count)
                .Select(s => new SponsorshipDto
                {
                    SponsorshipId = s.SponsorshipId,
                    GoalDescription = s.GoalDescription,
                    GoalAmount = s.GoalAmount,
                    AmountRaised = s.AmountRaised,
                    Status = s.Status,
                    Category = s.Category,
                    Story = s.Story,
                    ImageUrl = s.ImageUrl,
                    Deadline = s.Deadline,
                    DonorCount = s.DonorCount,
                    PatientId = s.PatientId,
                    PatientName = $"{s.Patient.User.FirstName} {s.Patient.User.LastName}",
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    ProgressPercentage = s.ProgressPercentage,
                    IsFullyFunded = s.IsFullyFunded,
                    IsUrgent = s.IsUrgent,
                    AmountNeeded = s.AmountNeeded
                })
                .ToListAsync();

            return sponsorships;
        }

        public async Task<decimal> GetTotalRaisedForPatientAsync(int patientId)
        {
            return await _context.Sponsorships
                .Where(s => s.PatientId == patientId)
                .SumAsync(s => s.AmountRaised);
        }

        public async Task<bool> CloseSponsorshipAsync(int id)
        {
            var sponsorship = await _context.Sponsorships.FindAsync(id);
            if (sponsorship == null) return false;

            sponsorship.Status = sponsorship.IsFullyFunded ? "Completed" : "Cancelled";
            sponsorship.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Sponsorship {SponsorshipId} closed with status {Status}",
                id, sponsorship.Status);

            return true;
        }
    }
}