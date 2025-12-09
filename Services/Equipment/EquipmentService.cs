// Services/Implementations/EquipmentService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using HealthAidAPI.Data;
using HealthAidAPI.DTOs;
using HealthAidAPI.Services.Interfaces;
using HealthAidAPI.Models;

namespace HealthAidAPI.Services.Implementations
{
    public class EquipmentService : IEquipmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<EquipmentService> _logger;

        public EquipmentService(ApplicationDbContext context, IMapper mapper, ILogger<EquipmentService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<EquipmentDto>> GetAllEquipmentAsync(EquipmentFilterDto filter)
        {
            try
            {
                var query = _context.Equipments
                    .Include(e => e.NGO)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(filter.Search))
                {
                    query = query.Where(e =>
                        e.Name.Contains(filter.Search) ||
                        e.Description != null && e.Description.Contains(filter.Search) ||
                        e.Model != null && e.Model.Contains(filter.Search) ||
                        e.SerialNumber != null && e.SerialNumber.Contains(filter.Search));
                }

                if (!string.IsNullOrEmpty(filter.Type))
                {
                    query = query.Where(e => e.Type == filter.Type);
                }

                if (!string.IsNullOrEmpty(filter.Location))
                {
                    query = query.Where(e => e.CurrentLocation.Contains(filter.Location));
                }

                if (!string.IsNullOrEmpty(filter.Status))
                {
                    query = query.Where(e => e.AvailableStatus == filter.Status);
                }

                if (!string.IsNullOrEmpty(filter.Condition))
                {
                    query = query.Where(e => e.Condition == filter.Condition);
                }

                if (filter.NGOId.HasValue)
                {
                    query = query.Where(e => e.NGOId == filter.NGOId.Value);
                }

                if (filter.NeedsMaintenance.HasValue && filter.NeedsMaintenance.Value)
                {
                    query = query.Where(e => e.NextMaintenanceDate.HasValue &&
                                           e.NextMaintenanceDate <= DateTime.UtcNow.AddDays(30));
                }

                if (filter.IsCritical.HasValue && filter.IsCritical.Value)
                {
                    query = query.Where(e => e.Condition == "Poor" || e.AvailableStatus == "Maintenance");
                }

                if (filter.MinValue.HasValue)
                {
                    query = query.Where(e => e.EstimatedValue >= filter.MinValue.Value);
                }

                if (filter.MaxValue.HasValue)
                {
                    query = query.Where(e => e.EstimatedValue <= filter.MaxValue.Value);
                }

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "name" => filter.SortDesc ?
                        query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name),
                    "type" => filter.SortDesc ?
                        query.OrderByDescending(e => e.Type) : query.OrderBy(e => e.Type),
                    "location" => filter.SortDesc ?
                        query.OrderByDescending(e => e.CurrentLocation) : query.OrderBy(e => e.CurrentLocation),
                    "value" => filter.SortDesc ?
                        query.OrderByDescending(e => e.EstimatedValue) : query.OrderBy(e => e.EstimatedValue),
                    "condition" => filter.SortDesc ?
                        query.OrderByDescending(e => e.Condition) : query.OrderBy(e => e.Condition),
                    "status" => filter.SortDesc ?
                        query.OrderByDescending(e => e.AvailableStatus) : query.OrderBy(e => e.AvailableStatus),
                    _ => filter.SortDesc ?
                        query.OrderByDescending(e => e.EquipmentId) : query.OrderBy(e => e.EquipmentId)
                };

                var totalCount = await query.CountAsync();
                var equipment = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(e => new EquipmentDto
                    {
                        EquipmentId = e.EquipmentId,
                        Name = e.Name,
                        Type = e.Type,
                        CurrentLocation = e.CurrentLocation,
                        AvailableStatus = e.AvailableStatus,
                        Model = e.Model,
                        SerialNumber = e.SerialNumber,
                        Description = e.Description,
                        Quantity = e.Quantity,
                        EstimatedValue = e.EstimatedValue,
                        LastMaintenanceDate = e.LastMaintenanceDate,
                        NextMaintenanceDate = e.NextMaintenanceDate,
                        Condition = e.Condition,
                        NGOId = e.NGOId,
                        NGOName = e.NGO.OrganizationName,
                        AddedDate = e.AddedDate,
                        UpdatedAt = e.UpdatedAt,
                        NeedsMaintenance = e.NextMaintenanceDate.HasValue &&
                                         e.NextMaintenanceDate <= DateTime.UtcNow.AddDays(30),
                        IsCritical = e.Condition == "Poor" || e.AvailableStatus == "Maintenance",
                        DaysUntilMaintenance = e.NextMaintenanceDate.HasValue ?
                            (e.NextMaintenanceDate.Value - DateTime.UtcNow).Days : int.MaxValue
                    })
                    .ToListAsync();

                return new PagedResult<EquipmentDto>
                {
                    Items = equipment,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving equipment with filter");
                throw;
            }
        }

        public async Task<EquipmentDto?> GetEquipmentByIdAsync(int id)
        {
            var equipment = await _context.Equipments
                .Include(e => e.NGO)
                .FirstOrDefaultAsync(e => e.EquipmentId == id);

            if (equipment == null) return null;

            return new EquipmentDto
            {
                EquipmentId = equipment.EquipmentId,
                Name = equipment.Name,
                Type = equipment.Type,
                CurrentLocation = equipment.CurrentLocation,
                AvailableStatus = equipment.AvailableStatus,
                Model = equipment.Model,
                SerialNumber = equipment.SerialNumber,
                Description = equipment.Description,
                Quantity = equipment.Quantity,
                EstimatedValue = equipment.EstimatedValue,
                LastMaintenanceDate = equipment.LastMaintenanceDate,
                NextMaintenanceDate = equipment.NextMaintenanceDate,
                Condition = equipment.Condition,
                NGOId = equipment.NGOId,
                NGOName = equipment.NGO.OrganizationName,
                AddedDate = equipment.AddedDate,
                UpdatedAt = equipment.UpdatedAt,
                NeedsMaintenance = equipment.NextMaintenanceDate.HasValue &&
                                 equipment.NextMaintenanceDate <= DateTime.UtcNow.AddDays(30),
                IsCritical = equipment.Condition == "Poor" || equipment.AvailableStatus == "Maintenance",
                DaysUntilMaintenance = equipment.NextMaintenanceDate.HasValue ?
                    (equipment.NextMaintenanceDate.Value - DateTime.UtcNow).Days : int.MaxValue
            };
        }

        public async Task<EquipmentDto> CreateEquipmentAsync(CreateEquipmentDto createEquipmentDto)
        {
            // Validate NGO exists
            var ngo = await _context.NGOs.FindAsync(createEquipmentDto.NGOId);
            if (ngo == null)
                throw new ArgumentException($"NGO with ID {createEquipmentDto.NGOId} not found");

            // Check for duplicate serial number if provided
            if (!string.IsNullOrEmpty(createEquipmentDto.SerialNumber))
            {
                var existingEquipment = await _context.Equipments
                    .FirstOrDefaultAsync(e => e.SerialNumber == createEquipmentDto.SerialNumber);

                if (existingEquipment != null)
                    throw new ArgumentException($"Equipment with serial number {createEquipmentDto.SerialNumber} already exists");
            }

            var equipment = new Equipment
            {
                Name = createEquipmentDto.Name,
                Type = createEquipmentDto.Type,
                CurrentLocation = createEquipmentDto.CurrentLocation,
                AvailableStatus = createEquipmentDto.AvailableStatus,
                Model = createEquipmentDto.Model,
                SerialNumber = createEquipmentDto.SerialNumber,
                Description = createEquipmentDto.Description,
                Quantity = createEquipmentDto.Quantity,
                EstimatedValue = createEquipmentDto.EstimatedValue,
                LastMaintenanceDate = createEquipmentDto.LastMaintenanceDate,
                NextMaintenanceDate = createEquipmentDto.NextMaintenanceDate,
                Condition = createEquipmentDto.Condition,
                NGOId = createEquipmentDto.NGOId,
                AddedDate = DateTime.UtcNow
            };

            _context.Equipments.Add(equipment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Equipment created: {EquipmentName} with ID {EquipmentId}",
                equipment.Name, equipment.EquipmentId);

            return new EquipmentDto
            {
                EquipmentId = equipment.EquipmentId,
                Name = equipment.Name,
                Type = equipment.Type,
                CurrentLocation = equipment.CurrentLocation,
                AvailableStatus = equipment.AvailableStatus,
                Model = equipment.Model,
                SerialNumber = equipment.SerialNumber,
                Description = equipment.Description,
                Quantity = equipment.Quantity,
                EstimatedValue = equipment.EstimatedValue,
                LastMaintenanceDate = equipment.LastMaintenanceDate,
                NextMaintenanceDate = equipment.NextMaintenanceDate,
                Condition = equipment.Condition,
                NGOId = equipment.NGOId,
                NGOName = ngo.OrganizationName,
                AddedDate = equipment.AddedDate,
                NeedsMaintenance = equipment.NextMaintenanceDate.HasValue &&
                                 equipment.NextMaintenanceDate <= DateTime.UtcNow.AddDays(30),
                IsCritical = equipment.Condition == "Poor" || equipment.AvailableStatus == "Maintenance",
                DaysUntilMaintenance = equipment.NextMaintenanceDate.HasValue ?
                    (equipment.NextMaintenanceDate.Value - DateTime.UtcNow).Days : int.MaxValue
            };
        }

        public async Task<EquipmentDto?> UpdateEquipmentAsync(int id, UpdateEquipmentDto updateEquipmentDto)
        {
            var equipment = await _context.Equipments
                .Include(e => e.NGO)
                .FirstOrDefaultAsync(e => e.EquipmentId == id);

            if (equipment == null) return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateEquipmentDto.Name))
                equipment.Name = updateEquipmentDto.Name;

            if (!string.IsNullOrEmpty(updateEquipmentDto.Type))
                equipment.Type = updateEquipmentDto.Type;

            if (!string.IsNullOrEmpty(updateEquipmentDto.CurrentLocation))
                equipment.CurrentLocation = updateEquipmentDto.CurrentLocation;

            if (!string.IsNullOrEmpty(updateEquipmentDto.AvailableStatus))
                equipment.AvailableStatus = updateEquipmentDto.AvailableStatus;

            if (updateEquipmentDto.Model != null)
                equipment.Model = updateEquipmentDto.Model;

            if (updateEquipmentDto.SerialNumber != null)
            {
                // Check for duplicate serial number
                var existingEquipment = await _context.Equipments
                    .FirstOrDefaultAsync(e => e.SerialNumber == updateEquipmentDto.SerialNumber &&
                                            e.EquipmentId != id);

                if (existingEquipment != null)
                    throw new ArgumentException($"Equipment with serial number {updateEquipmentDto.SerialNumber} already exists");

                equipment.SerialNumber = updateEquipmentDto.SerialNumber;
            }

            if (updateEquipmentDto.Description != null)
                equipment.Description = updateEquipmentDto.Description;

            if (updateEquipmentDto.Quantity.HasValue)
                equipment.Quantity = updateEquipmentDto.Quantity.Value;

            if (updateEquipmentDto.EstimatedValue.HasValue)
                equipment.EstimatedValue = updateEquipmentDto.EstimatedValue.Value;

            if (updateEquipmentDto.LastMaintenanceDate.HasValue)
                equipment.LastMaintenanceDate = updateEquipmentDto.LastMaintenanceDate.Value;

            if (updateEquipmentDto.NextMaintenanceDate.HasValue)
                equipment.NextMaintenanceDate = updateEquipmentDto.NextMaintenanceDate.Value;

            if (!string.IsNullOrEmpty(updateEquipmentDto.Condition))
                equipment.Condition = updateEquipmentDto.Condition;

            if (updateEquipmentDto.NGOId.HasValue)
            {
                var ngo = await _context.NGOs.FindAsync(updateEquipmentDto.NGOId.Value);
                if (ngo == null)
                    throw new ArgumentException($"NGO with ID {updateEquipmentDto.NGOId.Value} not found");

                equipment.NGOId = updateEquipmentDto.NGOId.Value;
            }

            equipment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Equipment updated: {EquipmentId}", id);

            return new EquipmentDto
            {
                EquipmentId = equipment.EquipmentId,
                Name = equipment.Name,
                Type = equipment.Type,
                CurrentLocation = equipment.CurrentLocation,
                AvailableStatus = equipment.AvailableStatus,
                Model = equipment.Model,
                SerialNumber = equipment.SerialNumber,
                Description = equipment.Description,
                Quantity = equipment.Quantity,
                EstimatedValue = equipment.EstimatedValue,
                LastMaintenanceDate = equipment.LastMaintenanceDate,
                NextMaintenanceDate = equipment.NextMaintenanceDate,
                Condition = equipment.Condition,
                NGOId = equipment.NGOId,
                NGOName = equipment.NGO.OrganizationName,
                AddedDate = equipment.AddedDate,
                UpdatedAt = equipment.UpdatedAt,
                NeedsMaintenance = equipment.NextMaintenanceDate.HasValue &&
                                 equipment.NextMaintenanceDate <= DateTime.UtcNow.AddDays(30),
                IsCritical = equipment.Condition == "Poor" || equipment.AvailableStatus == "Maintenance",
                DaysUntilMaintenance = equipment.NextMaintenanceDate.HasValue ?
                    (equipment.NextMaintenanceDate.Value - DateTime.UtcNow).Days : int.MaxValue
            };
        }

        public async Task<EquipmentDto?> ScheduleMaintenanceAsync(int id, MaintenanceScheduleDto maintenanceDto)
        {
            var equipment = await _context.Equipments
                .Include(e => e.NGO)
                .FirstOrDefaultAsync(e => e.EquipmentId == id);

            if (equipment == null) return null;

            equipment.LastMaintenanceDate = DateTime.UtcNow;
            equipment.NextMaintenanceDate = maintenanceDto.NextMaintenanceDate;
            equipment.AvailableStatus = "Maintenance";
            equipment.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(maintenanceDto.Notes))
            {
                equipment.Description = string.IsNullOrEmpty(equipment.Description) ?
                    $"Maintenance scheduled: {maintenanceDto.Notes}" :
                    $"{equipment.Description}\nMaintenance scheduled: {maintenanceDto.Notes}";
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Maintenance scheduled for equipment {EquipmentId}", id);

            return new EquipmentDto
            {
                EquipmentId = equipment.EquipmentId,
                Name = equipment.Name,
                Type = equipment.Type,
                CurrentLocation = equipment.CurrentLocation,
                AvailableStatus = equipment.AvailableStatus,
                Model = equipment.Model,
                SerialNumber = equipment.SerialNumber,
                Description = equipment.Description,
                Quantity = equipment.Quantity,
                EstimatedValue = equipment.EstimatedValue,
                LastMaintenanceDate = equipment.LastMaintenanceDate,
                NextMaintenanceDate = equipment.NextMaintenanceDate,
                Condition = equipment.Condition,
                NGOId = equipment.NGOId,
                NGOName = equipment.NGO.OrganizationName,
                AddedDate = equipment.AddedDate,
                UpdatedAt = equipment.UpdatedAt,
                NeedsMaintenance = false, // Just scheduled maintenance
                IsCritical = equipment.Condition == "Poor" || equipment.AvailableStatus == "Maintenance",
                DaysUntilMaintenance = (equipment.NextMaintenanceDate.Value - DateTime.UtcNow).Days
            };
        }

        public async Task<EquipmentDto?> TransferEquipmentAsync(int id, EquipmentTransferDto transferDto)
        {
            var equipment = await _context.Equipments
                .Include(e => e.NGO)
                .FirstOrDefaultAsync(e => e.EquipmentId == id);

            if (equipment == null) return null;

            var oldLocation = equipment.CurrentLocation;
            equipment.CurrentLocation = transferDto.NewLocation;
            equipment.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(transferDto.Reason))
            {
                equipment.Description = string.IsNullOrEmpty(equipment.Description) ?
                    $"Transferred from {oldLocation} to {transferDto.NewLocation}. Reason: {transferDto.Reason}" :
                    $"{equipment.Description}\nTransferred from {oldLocation} to {transferDto.NewLocation}. Reason: {transferDto.Reason}";
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Equipment {EquipmentId} transferred from {OldLocation} to {NewLocation}",
                id, oldLocation, transferDto.NewLocation);

            return new EquipmentDto
            {
                EquipmentId = equipment.EquipmentId,
                Name = equipment.Name,
                Type = equipment.Type,
                CurrentLocation = equipment.CurrentLocation,
                AvailableStatus = equipment.AvailableStatus,
                Model = equipment.Model,
                SerialNumber = equipment.SerialNumber,
                Description = equipment.Description,
                Quantity = equipment.Quantity,
                EstimatedValue = equipment.EstimatedValue,
                LastMaintenanceDate = equipment.LastMaintenanceDate,
                NextMaintenanceDate = equipment.NextMaintenanceDate,
                Condition = equipment.Condition,
                NGOId = equipment.NGOId,
                NGOName = equipment.NGO.OrganizationName,
                AddedDate = equipment.AddedDate,
                UpdatedAt = equipment.UpdatedAt,
                NeedsMaintenance = equipment.NextMaintenanceDate.HasValue &&
                                 equipment.NextMaintenanceDate <= DateTime.UtcNow.AddDays(30),
                IsCritical = equipment.Condition == "Poor" || equipment.AvailableStatus == "Maintenance",
                DaysUntilMaintenance = equipment.NextMaintenanceDate.HasValue ?
                    (equipment.NextMaintenanceDate.Value - DateTime.UtcNow).Days : int.MaxValue
            };
        }

        public async Task<bool> DeleteEquipmentAsync(int id)
        {
            var equipment = await _context.Equipments.FindAsync(id);
            if (equipment == null) return false;

            _context.Equipments.Remove(equipment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Equipment deleted: {EquipmentId}", id);
            return true;
        }

        public async Task<bool> DeleteEquipmentByNgoAsync(int ngoId)
        {
            var equipment = await _context.Equipments
                .Where(e => e.NGOId == ngoId)
                .ToListAsync();

            if (!equipment.Any()) return false;

            _context.Equipments.RemoveRange(equipment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted {Count} equipment items for NGO {NGOId}", equipment.Count, ngoId);
            return true;
        }

        public async Task<IEnumerable<EquipmentDto>> GetEquipmentByTypeAsync(string type)
        {
            var equipment = await _context.Equipments
                .Include(e => e.NGO)
                .Where(e => e.Type == type)
                .OrderBy(e => e.Name)
                .Select(e => new EquipmentDto
                {
                    EquipmentId = e.EquipmentId,
                    Name = e.Name,
                    Type = e.Type,
                    CurrentLocation = e.CurrentLocation,
                    AvailableStatus = e.AvailableStatus,
                    Model = e.Model,
                    SerialNumber = e.SerialNumber,
                    Description = e.Description,
                    Quantity = e.Quantity,
                    EstimatedValue = e.EstimatedValue,
                    LastMaintenanceDate = e.LastMaintenanceDate,
                    NextMaintenanceDate = e.NextMaintenanceDate,
                    Condition = e.Condition,
                    NGOId = e.NGOId,
                    NGOName = e.NGO.OrganizationName,
                    AddedDate = e.AddedDate,
                    UpdatedAt = e.UpdatedAt,
                    NeedsMaintenance = e.NextMaintenanceDate.HasValue &&
                                     e.NextMaintenanceDate <= DateTime.UtcNow.AddDays(30),
                    IsCritical = e.Condition == "Poor" || e.AvailableStatus == "Maintenance",
                    DaysUntilMaintenance = e.NextMaintenanceDate.HasValue ?
                        (e.NextMaintenanceDate.Value - DateTime.UtcNow).Days : int.MaxValue
                })
                .ToListAsync();

            return equipment;
        }

        public async Task<IEnumerable<EquipmentDto>> GetEquipmentByLocationAsync(string location)
        {
            var equipment = await _context.Equipments
                .Include(e => e.NGO)
                .Where(e => e.CurrentLocation.Contains(location))
                .OrderBy(e => e.Name)
                .Select(e => new EquipmentDto
                {
                    EquipmentId = e.EquipmentId,
                    Name = e.Name,
                    Type = e.Type,
                    CurrentLocation = e.CurrentLocation,
                    AvailableStatus = e.AvailableStatus,
                    Model = e.Model,
                    SerialNumber = e.SerialNumber,
                    Description = e.Description,
                    Quantity = e.Quantity,
                    EstimatedValue = e.EstimatedValue,
                    LastMaintenanceDate = e.LastMaintenanceDate,
                    NextMaintenanceDate = e.NextMaintenanceDate,
                    Condition = e.Condition,
                    NGOId = e.NGOId,
                    NGOName = e.NGO.OrganizationName,
                    AddedDate = e.AddedDate,
                    UpdatedAt = e.UpdatedAt,
                    NeedsMaintenance = e.NextMaintenanceDate.HasValue &&
                                     e.NextMaintenanceDate <= DateTime.UtcNow.AddDays(30),
                    IsCritical = e.Condition == "Poor" || e.AvailableStatus == "Maintenance",
                    DaysUntilMaintenance = e.NextMaintenanceDate.HasValue ?
                        (e.NextMaintenanceDate.Value - DateTime.UtcNow).Days : int.MaxValue
                })
                .ToListAsync();

            return equipment;
        }

        public async Task<IEnumerable<EquipmentDto>> GetCriticalEquipmentAsync()
        {
            var equipment = await _context.Equipments
                .Include(e => e.NGO)
                .Where(e => e.Condition == "Poor" || e.AvailableStatus == "Maintenance")
                .OrderByDescending(e => e.Condition)
                .ThenBy(e => e.Name)
                .Select(e => new EquipmentDto
                {
                    EquipmentId = e.EquipmentId,
                    Name = e.Name,
                    Type = e.Type,
                    CurrentLocation = e.CurrentLocation,
                    AvailableStatus = e.AvailableStatus,
                    Model = e.Model,
                    SerialNumber = e.SerialNumber,
                    Description = e.Description,
                    Quantity = e.Quantity,
                    EstimatedValue = e.EstimatedValue,
                    LastMaintenanceDate = e.LastMaintenanceDate,
                    NextMaintenanceDate = e.NextMaintenanceDate,
                    Condition = e.Condition,
                    NGOId = e.NGOId,
                    NGOName = e.NGO.OrganizationName,
                    AddedDate = e.AddedDate,
                    UpdatedAt = e.UpdatedAt,
                    NeedsMaintenance = e.NextMaintenanceDate.HasValue &&
                                     e.NextMaintenanceDate <= DateTime.UtcNow.AddDays(30),
                    IsCritical = true,
                    DaysUntilMaintenance = e.NextMaintenanceDate.HasValue ?
                        (e.NextMaintenanceDate.Value - DateTime.UtcNow).Days : int.MaxValue
                })
                .ToListAsync();

            return equipment;
        }

        public async Task<IEnumerable<EquipmentDto>> GetEquipmentNeedingMaintenanceAsync()
        {
            var equipment = await _context.Equipments
                .Include(e => e.NGO)
                .Where(e => e.NextMaintenanceDate.HasValue &&
                           e.NextMaintenanceDate <= DateTime.UtcNow.AddDays(30))
                .OrderBy(e => e.NextMaintenanceDate)
                .Select(e => new EquipmentDto
                {
                    EquipmentId = e.EquipmentId,
                    Name = e.Name,
                    Type = e.Type,
                    CurrentLocation = e.CurrentLocation,
                    AvailableStatus = e.AvailableStatus,
                    Model = e.Model,
                    SerialNumber = e.SerialNumber,
                    Description = e.Description,
                    Quantity = e.Quantity,
                    EstimatedValue = e.EstimatedValue,
                    LastMaintenanceDate = e.LastMaintenanceDate,
                    NextMaintenanceDate = e.NextMaintenanceDate,
                    Condition = e.Condition,
                    NGOId = e.NGOId,
                    NGOName = e.NGO.OrganizationName,
                    AddedDate = e.AddedDate,
                    UpdatedAt = e.UpdatedAt,
                    NeedsMaintenance = true,
                    IsCritical = e.Condition == "Poor" || e.AvailableStatus == "Maintenance",
                    DaysUntilMaintenance = (e.NextMaintenanceDate.Value - DateTime.UtcNow).Days
                })
                .ToListAsync();

            return equipment;
        }

        public async Task<EquipmentStatsDto> GetEquipmentStatsAsync()
        {
            var totalEquipment = await _context.Equipments.CountAsync();
            var availableEquipment = await _context.Equipments.CountAsync(e => e.AvailableStatus == "Available");
            var inUseEquipment = await _context.Equipments.CountAsync(e => e.AvailableStatus == "InUse");
            var maintenanceEquipment = await _context.Equipments.CountAsync(e => e.AvailableStatus == "Maintenance");
            var reservedEquipment = await _context.Equipments.CountAsync(e => e.AvailableStatus == "Reserved");
            var criticalEquipment = await _context.Equipments.CountAsync(e => e.Condition == "Poor" || e.AvailableStatus == "Maintenance");
            var needsMaintenanceCount = await _context.Equipments.CountAsync(e =>
                e.NextMaintenanceDate.HasValue && e.NextMaintenanceDate <= DateTime.UtcNow.AddDays(30));

            var totalValue = await _context.Equipments
                .Where(e => e.EstimatedValue.HasValue)
                .SumAsync(e => e.EstimatedValue.Value * e.Quantity);

            var equipmentByType = await _context.Equipments
                .GroupBy(e => e.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);

            var equipmentByCondition = await _context.Equipments
                .GroupBy(e => e.Condition)
                .Select(g => new { Condition = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Condition, x => x.Count);

            var equipmentByLocation = await _context.Equipments
                .GroupBy(e => e.CurrentLocation)
                .Select(g => new { Location = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Location, x => x.Count);

            return new EquipmentStatsDto
            {
                TotalEquipment = totalEquipment,
                AvailableEquipment = availableEquipment,
                InUseEquipment = inUseEquipment,
                MaintenanceEquipment = maintenanceEquipment,
                ReservedEquipment = reservedEquipment,
                CriticalEquipment = criticalEquipment,
                NeedsMaintenanceCount = needsMaintenanceCount,
                TotalValue = totalValue,
                EquipmentByType = equipmentByType,
                EquipmentByCondition = equipmentByCondition,
                EquipmentByLocation = equipmentByLocation
            };
        }

        public async Task<int> GetEquipmentCountByNgoAsync(int ngoId)
        {
            return await _context.Equipments
                .CountAsync(e => e.NGOId == ngoId);
        }

        public async Task<bool> PerformBulkUpdateAsync(BulkEquipmentUpdateDto bulkUpdateDto)
        {
            var equipment = await _context.Equipments
                .Where(e => bulkUpdateDto.EquipmentIds.Contains(e.EquipmentId))
                .ToListAsync();

            if (!equipment.Any()) return false;

            foreach (var item in equipment)
            {
                if (!string.IsNullOrEmpty(bulkUpdateDto.NewLocation))
                    item.CurrentLocation = bulkUpdateDto.NewLocation;

                if (!string.IsNullOrEmpty(bulkUpdateDto.NewStatus))
                    item.AvailableStatus = bulkUpdateDto.NewStatus;

                if (!string.IsNullOrEmpty(bulkUpdateDto.NewCondition))
                    item.Condition = bulkUpdateDto.NewCondition;

                item.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Bulk update performed on {Count} equipment items", equipment.Count);
            return true;
        }

        public async Task<IEnumerable<EquipmentDto>> SearchEquipmentAsync(string searchTerm)
        {
            var equipment = await _context.Equipments
                .Include(e => e.NGO)
                .Where(e => e.Name.Contains(searchTerm) ||
                           e.Description != null && e.Description.Contains(searchTerm) ||
                           e.Model != null && e.Model.Contains(searchTerm) ||
                           e.SerialNumber != null && e.SerialNumber.Contains(searchTerm) ||
                           e.Type.Contains(searchTerm) ||
                           e.CurrentLocation.Contains(searchTerm))
                .OrderBy(e => e.Name)
                .Select(e => new EquipmentDto
                {
                    EquipmentId = e.EquipmentId,
                    Name = e.Name,
                    Type = e.Type,
                    CurrentLocation = e.CurrentLocation,
                    AvailableStatus = e.AvailableStatus,
                    Model = e.Model,
                    SerialNumber = e.SerialNumber,
                    Description = e.Description,
                    Quantity = e.Quantity,
                    EstimatedValue = e.EstimatedValue,
                    LastMaintenanceDate = e.LastMaintenanceDate,
                    NextMaintenanceDate = e.NextMaintenanceDate,
                    Condition = e.Condition,
                    NGOId = e.NGOId,
                    NGOName = e.NGO.OrganizationName,
                    AddedDate = e.AddedDate,
                    UpdatedAt = e.UpdatedAt,
                    NeedsMaintenance = e.NextMaintenanceDate.HasValue &&
                                     e.NextMaintenanceDate <= DateTime.UtcNow.AddDays(30),
                    IsCritical = e.Condition == "Poor" || e.AvailableStatus == "Maintenance",
                    DaysUntilMaintenance = e.NextMaintenanceDate.HasValue ?
                        (e.NextMaintenanceDate.Value - DateTime.UtcNow).Days : int.MaxValue
                })
                .ToListAsync();

            return equipment;
        }
    }
}