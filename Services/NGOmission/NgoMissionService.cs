// Services/Implementations/NgoMissionService.cs
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using HealthAidAPI.Data;
using HealthAidAPI.Services.Interfaces;
using HealthAidAPI.Models;
using HealthAidAPI.Helpers;
using HealthAidAPI.DTOs.NgoMissions;
namespace HealthAidAPI.Services.Implementations
{
    public class NgoMissionService : INgoMissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<NgoMissionService> _logger;

        public NgoMissionService(ApplicationDbContext context, IMapper mapper, ILogger<NgoMissionService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<NgoMissionDto>> GetAllMissionsAsync(NgoMissionFilterDto filter)
        {
            try
            {
                var query = _context.NgoMessions
                    .Include(m => m.NGO)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(filter.Search))
                {
                    query = query.Where(m =>
                        m.Description.Contains(filter.Search) ||
                        m.Location.Contains(filter.Search));
                }

                if (!string.IsNullOrEmpty(filter.Location))
                {
                    query = query.Where(m => m.Location.Contains(filter.Location));
                }

                if (filter.NGOId.HasValue)
                {
                    query = query.Where(m => m.NGOId == filter.NGOId.Value);
                }

                if (filter.StartDateFrom.HasValue)
                {
                    query = query.Where(m => m.StartDate >= filter.StartDateFrom.Value);
                }

                if (filter.StartDateTo.HasValue)
                {
                    query = query.Where(m => m.StartDate <= filter.StartDateTo.Value);
                }

                if (filter.EndDateFrom.HasValue)
                {
                    query = query.Where(m => m.EndDate >= filter.EndDateFrom.Value);
                }

                if (filter.EndDateTo.HasValue)
                {
                    query = query.Where(m => m.EndDate <= filter.EndDateTo.Value);
                }

                if (!string.IsNullOrEmpty(filter.Status))
                {
                    var today = DateTime.Today;
                    query = filter.Status.ToLower() switch
                    {
                        "upcoming" => query.Where(m => m.StartDate > today),
                        "ongoing" => query.Where(m => m.StartDate <= today && m.EndDate >= today),
                        "completed" => query.Where(m => m.EndDate < today),
                        _ => query
                    };
                }

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "startdate" => filter.SortDesc ?
                        query.OrderByDescending(m => m.StartDate) : query.OrderBy(m => m.StartDate),
                    "enddate" => filter.SortDesc ?
                        query.OrderByDescending(m => m.EndDate) : query.OrderBy(m => m.EndDate),
                    "location" => filter.SortDesc ?
                        query.OrderByDescending(m => m.Location) : query.OrderBy(m => m.Location),
                    _ => filter.SortDesc ?
                        query.OrderByDescending(m => m.NgoMessionId) : query.OrderBy(m => m.NgoMessionId)
                };

                var totalCount = await query.CountAsync();
                var missions = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(m => new NgoMissionDto
                    {
                        NgoMissionId = m.NgoMessionId,
                        Description = m.Description,
                        StartDate = m.StartDate,
                        EndDate = m.EndDate,
                        Location = m.Location,
                        NGOId = m.NGOId,
                        NGOName = m.NGO.OrganizationName,
                        Status = GetMissionStatus(m.StartDate, m.EndDate),
                        DaysRemaining = GetDaysRemaining(m.StartDate, m.EndDate)
                    })
                    .ToListAsync();

                return new PagedResult<NgoMissionDto>(missions, totalCount, filter.Page, filter.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving NGO missions with filter");
                throw;
            }
        }

        public async Task<NgoMissionDto?> GetMissionByIdAsync(int id)
        {
            var mission = await _context.NgoMessions
                .Include(m => m.NGO)
                .FirstOrDefaultAsync(m => m.NgoMessionId == id);

            if (mission == null) return null;

            return new NgoMissionDto
            {
                NgoMissionId = mission.NgoMessionId,
                Description = mission.Description,
                StartDate = mission.StartDate,
                EndDate = mission.EndDate,
                Location = mission.Location,
                NGOId = mission.NGOId,
                NGOName = mission.NGO.OrganizationName,
                Status = GetMissionStatus(mission.StartDate, mission.EndDate),
                DaysRemaining = GetDaysRemaining(mission.StartDate, mission.EndDate)
            };
        }

        public async Task<NgoMissionDto> CreateMissionAsync(CreateNgoMissionDto createMissionDto)
        {
            // Validate NGO exists
            var ngo = await _context.NGOs.FindAsync(createMissionDto.NGOId);
            if (ngo == null)
                throw new ArgumentException($"NGO with ID {createMissionDto.NGOId} not found");

            var mission = new NgoMission
            {
                Description = createMissionDto.Description,
                StartDate = createMissionDto.StartDate,
                EndDate = createMissionDto.EndDate,
                Location = createMissionDto.Location,
                NGOId = createMissionDto.NGOId
            };

            _context.NgoMessions.Add(mission);
            await _context.SaveChangesAsync();

            _logger.LogInformation("NGO mission created: {MissionId} for NGO {NGOId}",
                mission.NgoMessionId, mission.NGOId);

            return new NgoMissionDto
            {
                NgoMissionId = mission.NgoMessionId,
                Description = mission.Description,
                StartDate = mission.StartDate,
                EndDate = mission.EndDate,
                Location = mission.Location,
                NGOId = mission.NGOId,
                NGOName = ngo.OrganizationName,
                Status = GetMissionStatus(mission.StartDate, mission.EndDate),
                DaysRemaining = GetDaysRemaining(mission.StartDate, mission.EndDate)
            };
        }

        public async Task<NgoMissionDto?> UpdateMissionAsync(int id, UpdateNgoMissionDto updateMissionDto)
        {
            var mission = await _context.NgoMessions
                .Include(m => m.NGO)
                .FirstOrDefaultAsync(m => m.NgoMessionId == id);

            if (mission == null) return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateMissionDto.Description))
                mission.Description = updateMissionDto.Description;

            if (updateMissionDto.StartDate.HasValue)
                mission.StartDate = updateMissionDto.StartDate.Value;

            if (updateMissionDto.EndDate.HasValue)
                mission.EndDate = updateMissionDto.EndDate.Value;

            if (!string.IsNullOrEmpty(updateMissionDto.Location))
                mission.Location = updateMissionDto.Location;

            if (updateMissionDto.NGOId.HasValue)
            {
                var ngo = await _context.NGOs.FindAsync(updateMissionDto.NGOId.Value);
                if (ngo == null)
                    throw new ArgumentException($"NGO with ID {updateMissionDto.NGOId.Value} not found");

                mission.NGOId = updateMissionDto.NGOId.Value;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("NGO mission updated: {MissionId}", id);

            return new NgoMissionDto
            {
                NgoMissionId = mission.NgoMessionId,
                Description = mission.Description,
                StartDate = mission.StartDate,
                EndDate = mission.EndDate,
                Location = mission.Location,
                NGOId = mission.NGOId,
                NGOName = mission.NGO.OrganizationName,
                Status = GetMissionStatus(mission.StartDate, mission.EndDate),
                DaysRemaining = GetDaysRemaining(mission.StartDate, mission.EndDate)
            };
        }

        public async Task<bool> DeleteMissionAsync(int id)
        {
            var mission = await _context.NgoMessions.FindAsync(id);
            if (mission == null) return false;

            _context.NgoMessions.Remove(mission);
            await _context.SaveChangesAsync();

            _logger.LogInformation("NGO mission deleted: {MissionId}", id);
            return true;
        }

        public async Task<bool> DeleteMissionsByNgoAsync(int ngoId)
        {
            var missions = await _context.NgoMessions
                .Where(m => m.NGOId == ngoId)
                .ToListAsync();

            if (!missions.Any()) return false;

            _context.NgoMessions.RemoveRange(missions);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted {Count} missions for NGO {NGOId}", missions.Count, ngoId);
            return true;
        }

        public async Task<IEnumerable<NgoMissionDto>> GetMissionsByDateRangeAsync(DateRangeDto dateRange)
        {
            var missions = await _context.NgoMessions
                .Include(m => m.NGO)
                .Where(m => m.StartDate >= dateRange.Start && m.EndDate <= dateRange.End)
                .OrderBy(m => m.StartDate)
                .Select(m => new NgoMissionDto
                {
                    NgoMissionId = m.NgoMessionId,
                    Description = m.Description,
                    StartDate = m.StartDate,
                    EndDate = m.EndDate,
                    Location = m.Location,
                    NGOId = m.NGOId,
                    NGOName = m.NGO.OrganizationName,
                    Status = GetMissionStatus(m.StartDate, m.EndDate),
                    DaysRemaining = GetDaysRemaining(m.StartDate, m.EndDate)
                })
                .ToListAsync();

            return missions;
        }

        public async Task<IEnumerable<NgoMissionDto>> SearchMissionsByLocationAsync(string location)
        {
            var missions = await _context.NgoMessions
                .Include(m => m.NGO)
                .Where(m => m.Location.Contains(location))
                .OrderBy(m => m.StartDate)
                .Select(m => new NgoMissionDto
                {
                    NgoMissionId = m.NgoMessionId,
                    Description = m.Description,
                    StartDate = m.StartDate,
                    EndDate = m.EndDate,
                    Location = m.Location,
                    NGOId = m.NGOId,
                    NGOName = m.NGO.OrganizationName,
                    Status = GetMissionStatus(m.StartDate, m.EndDate),
                    DaysRemaining = GetDaysRemaining(m.StartDate, m.EndDate)
                })
                .ToListAsync();

            return missions;
        }

        public async Task<int> GetMissionCountByNgoAsync(int ngoId)
        {
            return await _context.NgoMessions
                .CountAsync(m => m.NGOId == ngoId);
        }

        public async Task<MissionStatsDto> GetMissionStatsAsync()
        {
            var today = DateTime.Today;
            var totalMissions = await _context.NgoMessions.CountAsync();
            var upcomingMissions = await _context.NgoMessions.CountAsync(m => m.StartDate > today);
            var ongoingMissions = await _context.NgoMessions.CountAsync(m => m.StartDate <= today && m.EndDate >= today);
            var completedMissions = await _context.NgoMessions.CountAsync(m => m.EndDate < today);

            var missionsByLocation = await _context.NgoMessions
                .GroupBy(m => m.Location)
                .Select(g => new { Location = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Location, x => x.Count);

            var missionsByNGO = await _context.NgoMessions
                .Include(m => m.NGO)
                .GroupBy(m => m.NGOId)
                .Select(g => new { NGOId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.NGOId, x => x.Count);

            return new MissionStatsDto
            {
                TotalMissions = totalMissions,
                UpcomingMissions = upcomingMissions,
                OngoingMissions = ongoingMissions,
                CompletedMissions = completedMissions,
                MissionsByLocation = missionsByLocation,
                MissionsByNGO = missionsByNGO
            };
        }

        public async Task<IEnumerable<NgoMissionDto>> GetUpcomingMissionsAsync(int days = 30)
        {
            var startDate = DateTime.Today;
            var endDate = startDate.AddDays(days);

            var missions = await _context.NgoMessions
                .Include(m => m.NGO)
                .Where(m => m.StartDate >= startDate && m.StartDate <= endDate)
                .OrderBy(m => m.StartDate)
                .Select(m => new NgoMissionDto
                {
                    NgoMissionId = m.NgoMessionId,
                    Description = m.Description,
                    StartDate = m.StartDate,
                    EndDate = m.EndDate,
                    Location = m.Location,
                    NGOId = m.NGOId,
                    NGOName = m.NGO.OrganizationName,
                    Status = GetMissionStatus(m.StartDate, m.EndDate),
                    DaysRemaining = GetDaysRemaining(m.StartDate, m.EndDate)
                })
                .ToListAsync();

            return missions;
        }

        // Helper methods
        private static string GetMissionStatus(DateTime startDate, DateTime endDate)
        {
            var today = DateTime.Today;
            if (startDate > today) return "Upcoming";
            if (endDate < today) return "Completed";
            return "Ongoing";
        }

        private static int GetDaysRemaining(DateTime startDate, DateTime endDate)
        {
            var today = DateTime.Today;
            if (startDate > today) return (startDate - today).Days;
            if (endDate >= today) return (endDate - today).Days;
            return 0;
        }
    }
}