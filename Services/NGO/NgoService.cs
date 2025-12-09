// Services/Implementations/NgoService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using HealthAidAPI.Data;
using HealthAidAPI.Services.Interfaces;
using HealthAidAPI.Models;
using HealthAidAPI.DTOs.NGO;
using HealthAidAPI.DTOs.NGOmission;
using HealthAidAPI.DTOs;

namespace HealthAidAPI.Services.Implementations
{
    public class NgoService : INgoService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<NgoService> _logger;

        public NgoService(ApplicationDbContext context, IMapper mapper, ILogger<NgoService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<NgoDto>> GetAllNgosAsync(NgoFilterDto filter)
        {
            try
            {
                var query = _context.NGOs
                    .Include(n => n.NgoMessions)
                    .Include(n => n.Equipments)
                    .Include(n => n.Patients)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(filter.Search))
                {
                    query = query.Where(n =>
                        n.OrganizationName.Contains(filter.Search) ||
                        n.AreaOfWork.Contains(filter.Search) ||
                        n.ContactedPerson.Contains(filter.Search));
                }

                if (!string.IsNullOrEmpty(filter.Status))
                {
                    query = query.Where(n => n.VerifiedStatus == filter.Status);
                }

                if (!string.IsNullOrEmpty(filter.AreaOfWork))
                {
                    query = query.Where(n => n.AreaOfWork.Contains(filter.AreaOfWork));
                }

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "name" => filter.SortDesc ?
                        query.OrderByDescending(n => n.OrganizationName) : query.OrderBy(n => n.OrganizationName),
                    "area" => filter.SortDesc ?
                        query.OrderByDescending(n => n.AreaOfWork) : query.OrderBy(n => n.AreaOfWork),
                    "status" => filter.SortDesc ?
                        query.OrderByDescending(n => n.VerifiedStatus) : query.OrderBy(n => n.VerifiedStatus),
                    _ => filter.SortDesc ?
                        query.OrderByDescending(n => n.NGOId) : query.OrderBy(n => n.NGOId)
                };

                var totalCount = await query.CountAsync();
                var ngos = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(n => new NgoDto
                    {
                        NGOId = n.NGOId,
                        OrganizationName = n.OrganizationName,
                        AreaOfWork = n.AreaOfWork,
                        VerifiedStatus = n.VerifiedStatus,
                        ContactedPerson = n.ContactedPerson,
                        MissionCount = n.NgoMessions.Count,
                        EquipmentCount = n.Equipments.Count,
                        PatientCount = n.Patients.Count,
                        CreatedAt = n.CreatedAt
                    })
                    .ToListAsync();

                return new PagedResult<NgoDto>
                {
                    Items = ngos,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving NGOs with filter");
                throw;
            }
        }

        public async Task<NgoDetailDto?> GetNgoByIdAsync(int id)
        {
            var ngo = await _context.NGOs
                .Include(n => n.NgoMessions)
                .Include(n => n.Equipments)
                .Include(n => n.Patients)
                .FirstOrDefaultAsync(n => n.NGOId == id);

            if (ngo == null) return null;

            return new NgoDetailDto
            {
                NGOId = ngo.NGOId,
                OrganizationName = ngo.OrganizationName,
                AreaOfWork = ngo.AreaOfWork,
                VerifiedStatus = ngo.VerifiedStatus,
                ContactedPerson = ngo.ContactedPerson,
                Missions = ngo.NgoMessions.Select(m => new NgoMissionDto
                {
                    NgoMissionId = m.NgoMessionId,
                    Description = m.Description,
                    StartDate = m.StartDate,
                    EndDate = m.EndDate,
                    Location = m.Location
                }).ToList(),
                //Equipments = ngo.Equipments.Select(e => new EquipmentDto
                //{
                //    EquipmentId = e.EquipmentId,
                //    Name = e.Name,
                //    Type = e.Type,
                //    AvailableStatus = e.AvailableStatus
                //}).ToList(),
                //Patients = ngo.Patients.Select(p => new PatientDto
                //{
                //    PatientId = p.PatientId,
                //    FirstName = p.User.FirstName,
                //    LastName = p.User.LastName,
                //    MedicalHistory = p.MedicalHistory
                //}).ToList(),
                //CreatedAt = ngo.CreatedAt
            };
        }

        public async Task<NgoDto> CreateNgoAsync(CreateNgoDto createNgoDto)
        {
            // Check if NGO name already exists
            var existingNgo = await _context.NGOs
                .FirstOrDefaultAsync(n => n.OrganizationName == createNgoDto.OrganizationName);

            if (existingNgo != null)
                throw new ArgumentException($"NGO with name '{createNgoDto.OrganizationName}' already exists");

            var ngo = new NGO
            {
                OrganizationName = createNgoDto.OrganizationName,
                AreaOfWork = createNgoDto.AreaOfWork,
                VerifiedStatus = createNgoDto.VerifiedStatus,
                ContactedPerson = createNgoDto.ContactedPerson,
                CreatedAt = DateTime.UtcNow
            };

            _context.NGOs.Add(ngo);
            await _context.SaveChangesAsync();

            _logger.LogInformation("NGO created: {OrganizationName} with ID {NGOId}",
                ngo.OrganizationName, ngo.NGOId);

            return new NgoDto
            {
                NGOId = ngo.NGOId,
                OrganizationName = ngo.OrganizationName,
                AreaOfWork = ngo.AreaOfWork,
                VerifiedStatus = ngo.VerifiedStatus,
                ContactedPerson = ngo.ContactedPerson,
                MissionCount = 0,
                EquipmentCount = 0,
                PatientCount = 0,
                CreatedAt = ngo.CreatedAt
            };
        }

        public async Task<NgoDto?> UpdateNgoAsync(int id, UpdateNgoDto updateNgoDto)
        {
            var ngo = await _context.NGOs
                .Include(n => n.NgoMessions)
                .Include(n => n.Equipments)
                .Include(n => n.Patients)
                .FirstOrDefaultAsync(n => n.NGOId == id);

            if (ngo == null) return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateNgoDto.OrganizationName))
                ngo.OrganizationName = updateNgoDto.OrganizationName;

            if (!string.IsNullOrEmpty(updateNgoDto.AreaOfWork))
                ngo.AreaOfWork = updateNgoDto.AreaOfWork;

            if (!string.IsNullOrEmpty(updateNgoDto.VerifiedStatus))
                ngo.VerifiedStatus = updateNgoDto.VerifiedStatus;

            if (!string.IsNullOrEmpty(updateNgoDto.ContactedPerson))
                ngo.ContactedPerson = updateNgoDto.ContactedPerson;

            await _context.SaveChangesAsync();

            _logger.LogInformation("NGO updated: {NGOId}", id);

            return new NgoDto
            {
                NGOId = ngo.NGOId,
                OrganizationName = ngo.OrganizationName,
                AreaOfWork = ngo.AreaOfWork,
                VerifiedStatus = ngo.VerifiedStatus,
                ContactedPerson = ngo.ContactedPerson,
                MissionCount = ngo.NgoMessions.Count,
                EquipmentCount = ngo.Equipments.Count,
                PatientCount = ngo.Patients.Count,
                CreatedAt = ngo.CreatedAt
            };
        }

        public async Task<bool> DeleteNgoAsync(int id)
        {
            var ngo = await _context.NGOs.FindAsync(id);
            if (ngo == null) return false;

            _context.NGOs.Remove(ngo);
            await _context.SaveChangesAsync();

            _logger.LogInformation("NGO deleted: {NGOId} - {OrganizationName}", id, ngo.OrganizationName);
            return true;
        }

        public async Task<bool> DeleteNgoByNameAsync(string name)
        {
            var ngo = await _context.NGOs
                .FirstOrDefaultAsync(n => n.OrganizationName == name);

            if (ngo == null) return false;

            _context.NGOs.Remove(ngo);
            await _context.SaveChangesAsync();

            _logger.LogInformation("NGO deleted by name: {OrganizationName}", name);
            return true;
        }

        public async Task<IEnumerable<NgoDto>> GetNgosByStatusAsync(string status)
        {
            var ngos = await _context.NGOs
                .Include(n => n.NgoMessions)
                .Include(n => n.Equipments)
                .Include(n => n.Patients)
                .Where(n => n.VerifiedStatus == status)
                .Select(n => new NgoDto
                {
                    NGOId = n.NGOId,
                    OrganizationName = n.OrganizationName,
                    AreaOfWork = n.AreaOfWork,
                    VerifiedStatus = n.VerifiedStatus,
                    ContactedPerson = n.ContactedPerson,
                    MissionCount = n.NgoMessions.Count,
                    EquipmentCount = n.Equipments.Count,
                    PatientCount = n.Patients.Count,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

            return ngos;
        }

        public async Task<IEnumerable<NgoDto>> SearchNgosAsync(string keyword)
        {
            var ngos = await _context.NGOs
                .Include(n => n.NgoMessions)
                .Include(n => n.Equipments)
                .Include(n => n.Patients)
                .Where(n => n.OrganizationName.Contains(keyword) ||
                           n.AreaOfWork.Contains(keyword) ||
                           n.ContactedPerson.Contains(keyword))
                .Select(n => new NgoDto
                {
                    NGOId = n.NGOId,
                    OrganizationName = n.OrganizationName,
                    AreaOfWork = n.AreaOfWork,
                    VerifiedStatus = n.VerifiedStatus,
                    ContactedPerson = n.ContactedPerson,
                    MissionCount = n.NgoMessions.Count,
                    EquipmentCount = n.Equipments.Count,
                    PatientCount = n.Patients.Count,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

            return ngos;
        }

        public async Task<IEnumerable<NgoDto>> GetNgosByAreaAsync(string area)
        {
            var ngos = await _context.NGOs
                .Include(n => n.NgoMessions)
                .Include(n => n.Equipments)
                .Include(n => n.Patients)
                .Where(n => n.AreaOfWork.Contains(area))
                .Select(n => new NgoDto
                {
                    NGOId = n.NGOId,
                    OrganizationName = n.OrganizationName,
                    AreaOfWork = n.AreaOfWork,
                    VerifiedStatus = n.VerifiedStatus,
                    ContactedPerson = n.ContactedPerson,
                    MissionCount = n.NgoMessions.Count,
                    EquipmentCount = n.Equipments.Count,
                    PatientCount = n.Patients.Count,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

            return ngos;
        }

        public async Task<IEnumerable<NgoDto>> GetNgosWithMissionCountAsync()
        {
            var ngos = await _context.NGOs
                .Include(n => n.NgoMessions)
                .Include(n => n.Equipments)
                .Include(n => n.Patients)
                .Select(n => new NgoDto
                {
                    NGOId = n.NGOId,
                    OrganizationName = n.OrganizationName,
                    AreaOfWork = n.AreaOfWork,
                    VerifiedStatus = n.VerifiedStatus,
                    ContactedPerson = n.ContactedPerson,
                    MissionCount = n.NgoMessions.Count,
                    EquipmentCount = n.Equipments.Count,
                    PatientCount = n.Patients.Count,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

            return ngos;
        }

        public async Task<NgoStatsDto> GetNgoStatsAsync()
        {
            var totalNgos = await _context.NGOs.CountAsync();
            var verifiedNgos = await _context.NGOs.CountAsync(n => n.VerifiedStatus == "Verified");
            var pendingNgos = await _context.NGOs.CountAsync(n => n.VerifiedStatus == "Pending");
            var rejectedNgos = await _context.NGOs.CountAsync(n => n.VerifiedStatus == "Rejected");

            var ngosByArea = await _context.NGOs
                .GroupBy(n => n.AreaOfWork)
                .Select(g => new { Area = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Area, x => x.Count);

            var totalMissions = await _context.NgoMessions.CountAsync();
            var totalEquipment = await _context.Equipments.CountAsync();
            var totalPatients = await _context.Patients.CountAsync();

            return new NgoStatsDto
            {
                TotalNgos = totalNgos,
                VerifiedNgos = verifiedNgos,
                PendingNgos = pendingNgos,
                RejectedNgos = rejectedNgos,
                NgosByArea = ngosByArea,
                TotalMissions = totalMissions,
                TotalEquipment = totalEquipment,
                TotalPatients = totalPatients
            };
        }

        public async Task<bool> VerifyNgoAsync(int id, string status)
        {
            var validStatuses = new[] { "Verified", "Pending", "Rejected" };
            if (!validStatuses.Contains(status))
                throw new ArgumentException("Invalid status. Must be 'Verified', 'Pending', or 'Rejected'");

            var ngo = await _context.NGOs.FindAsync(id);
            if (ngo == null) return false;

            ngo.VerifiedStatus = status;
            await _context.SaveChangesAsync();

            _logger.LogInformation("NGO {NGOId} status updated to {Status}", id, status);
            return true;
        }
    }
}