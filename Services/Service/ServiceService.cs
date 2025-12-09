// Services/Implementations/ServiceService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using HealthAidAPI.Data;
using HealthAidAPI.DTOs;
using HealthAidAPI.Services.Interfaces;
using HealthAidAPI.Models;

namespace HealthAidAPI.Services.Implementations
{
    public class ServiceService : IServiceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ServiceService> _logger;

        public ServiceService(ApplicationDbContext context, IMapper mapper, ILogger<ServiceService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<ServiceDto>> GetServicesAsync(ServiceFilterDto filter)
        {
            try
            {
                var query = _context.Services.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(filter.Search))
                {
                    query = query.Where(s =>
                        s.Name.Contains(filter.Search) ||
                        s.Description.Contains(filter.Search) ||
                        s.Category.Contains(filter.Search));
                }

                if (!string.IsNullOrEmpty(filter.Category))
                    query = query.Where(s => s.Category == filter.Category);

                if (!string.IsNullOrEmpty(filter.Status))
                    query = query.Where(s => s.Status == filter.Status);

                if (!string.IsNullOrEmpty(filter.ProviderType))
                    query = query.Where(s => s.ProviderType == filter.ProviderType);

                if (filter.ProviderId.HasValue)
                    query = query.Where(s => s.ProviderId == filter.ProviderId.Value);

                if (filter.IsFree.HasValue)
                {
                    if (filter.IsFree.Value)
                        query = query.Where(s => !s.Price.HasValue || s.Price == 0);
                    else
                        query = query.Where(s => s.Price.HasValue && s.Price > 0);
                }

                if (filter.MinPrice.HasValue)
                    query = query.Where(s => s.Price >= filter.MinPrice.Value);

                if (filter.MaxPrice.HasValue)
                    query = query.Where(s => s.Price <= filter.MaxPrice.Value);

                if (filter.CreatedAfter.HasValue)
                    query = query.Where(s => s.CreatedAt >= filter.CreatedAfter.Value);

                if (filter.UpdatedAfter.HasValue)
                    query = query.Where(s => s.UpdatedAt >= filter.UpdatedAfter.Value);

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "name" => filter.SortDesc ?
                        query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
                    "price" => filter.SortDesc ?
                        query.OrderByDescending(s => s.Price) : query.OrderBy(s => s.Price),
                    "category" => filter.SortDesc ?
                        query.OrderByDescending(s => s.Category) : query.OrderBy(s => s.Category),
                    "status" => filter.SortDesc ?
                        query.OrderByDescending(s => s.Status) : query.OrderBy(s => s.Status),
                    "createdat" => filter.SortDesc ?
                        query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
                    "updatedat" => filter.SortDesc ?
                        query.OrderByDescending(s => s.UpdatedAt) : query.OrderBy(s => s.UpdatedAt),
                    _ => filter.SortDesc ?
                        query.OrderByDescending(s => s.ServiceId) : query.OrderBy(s => s.ServiceId)
                };

                var totalCount = await query.CountAsync();
                var services = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(s => new ServiceDto
                    {
                        ServiceId = s.ServiceId,
                        Name = s.Name,
                        Description = s.Description,
                        Category = s.Category,
                        Price = s.Price,
                        Status = s.Status,
                        ProviderId = s.ProviderId,
                        ProviderType = s.ProviderType,
                        ProviderName = GetProviderName(s.ProviderId, s.ProviderType),
                        CreatedAt = s.CreatedAt,
                        UpdatedAt = s.UpdatedAt
                    })
                    .ToListAsync();

                return new PagedResult<ServiceDto>
                {
                    Items = services,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving services with filter");
                throw;
            }
        }

        public async Task<ServiceDto?> GetServiceByIdAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return null;

            return new ServiceDto
            {
                ServiceId = service.ServiceId,
                Name = service.Name,
                Description = service.Description,
                Category = service.Category,
                Price = service.Price,
                Status = service.Status,
                ProviderId = service.ProviderId,
                ProviderType = service.ProviderType,
                ProviderName = await GetProviderNameAsync(service.ProviderId, service.ProviderType),
                CreatedAt = service.CreatedAt,
                UpdatedAt = service.UpdatedAt
            };
        }

        public async Task<ServiceDto> CreateServiceAsync(CreateServiceDto createServiceDto)
        {
            // Validate provider if provided
            if (createServiceDto.ProviderId.HasValue && !string.IsNullOrEmpty(createServiceDto.ProviderType))
            {
                var isValidProvider = await ValidateProviderAsync(createServiceDto.ProviderId.Value, createServiceDto.ProviderType);
                if (!isValidProvider)
                    throw new ArgumentException($"Invalid provider: {createServiceDto.ProviderType} with ID {createServiceDto.ProviderId}");
            }

            var service = new Service
            {
                Name = createServiceDto.Name,
                Description = createServiceDto.Description,
                Category = createServiceDto.Category,
                Price = createServiceDto.Price,
                Status = createServiceDto.Status,
                ProviderId = createServiceDto.ProviderId,
                ProviderType = createServiceDto.ProviderType,
                CreatedAt = DateTime.UtcNow
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Service created: {ServiceName}", service.Name);

            return new ServiceDto
            {
                ServiceId = service.ServiceId,
                Name = service.Name,
                Description = service.Description,
                Category = service.Category,
                Price = service.Price,
                Status = service.Status,
                ProviderId = service.ProviderId,
                ProviderType = service.ProviderType,
                ProviderName = await GetProviderNameAsync(service.ProviderId, service.ProviderType),
                CreatedAt = service.CreatedAt
            };
        }

        public async Task<ServiceDto?> UpdateServiceAsync(int id, UpdateServiceDto updateServiceDto)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return null;

            // Validate provider if provided
            if (updateServiceDto.ProviderId.HasValue && !string.IsNullOrEmpty(updateServiceDto.ProviderType))
            {
                var isValidProvider = await ValidateProviderAsync(updateServiceDto.ProviderId.Value, updateServiceDto.ProviderType);
                if (!isValidProvider)
                    throw new ArgumentException($"Invalid provider: {updateServiceDto.ProviderType} with ID {updateServiceDto.ProviderId}");
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateServiceDto.Name))
                service.Name = updateServiceDto.Name;

            if (!string.IsNullOrEmpty(updateServiceDto.Description))
                service.Description = updateServiceDto.Description;

            if (!string.IsNullOrEmpty(updateServiceDto.Category))
                service.Category = updateServiceDto.Category;

            if (updateServiceDto.Price.HasValue)
                service.Price = updateServiceDto.Price.Value;

            if (!string.IsNullOrEmpty(updateServiceDto.Status))
                service.Status = updateServiceDto.Status;

            if (updateServiceDto.ProviderId.HasValue)
                service.ProviderId = updateServiceDto.ProviderId.Value;

            if (!string.IsNullOrEmpty(updateServiceDto.ProviderType))
                service.ProviderType = updateServiceDto.ProviderType;

            service.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Service {ServiceId} updated", id);

            return new ServiceDto
            {
                ServiceId = service.ServiceId,
                Name = service.Name,
                Description = service.Description,
                Category = service.Category,
                Price = service.Price,
                Status = service.Status,
                ProviderId = service.ProviderId,
                ProviderType = service.ProviderType,
                ProviderName = await GetProviderNameAsync(service.ProviderId, service.ProviderType),
                CreatedAt = service.CreatedAt,
                UpdatedAt = service.UpdatedAt
            };
        }

        public async Task<bool> DeleteServiceAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return false;

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Service {ServiceId} deleted", id);
            return true;
        }

        public async Task<ServiceDto?> AssignProviderAsync(int id, AssignServiceProviderDto assignProviderDto)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return null;

            // Validate provider
            var isValidProvider = await ValidateProviderAsync(assignProviderDto.ProviderId, assignProviderDto.ProviderType);
            if (!isValidProvider)
                throw new ArgumentException($"Invalid provider: {assignProviderDto.ProviderType} with ID {assignProviderDto.ProviderId}");

            service.ProviderId = assignProviderDto.ProviderId;
            service.ProviderType = assignProviderDto.ProviderType;
            service.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Service {ServiceId} assigned to provider {ProviderType} {ProviderId}",
                id, assignProviderDto.ProviderType, assignProviderDto.ProviderId);

            return new ServiceDto
            {
                ServiceId = service.ServiceId,
                Name = service.Name,
                Description = service.Description,
                Category = service.Category,
                Price = service.Price,
                Status = service.Status,
                ProviderId = service.ProviderId,
                ProviderType = service.ProviderType,
                ProviderName = await GetProviderNameAsync(service.ProviderId, service.ProviderType),
                CreatedAt = service.CreatedAt,
                UpdatedAt = service.UpdatedAt
            };
        }

        public async Task<ServiceDto?> RemoveProviderAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return null;

            service.ProviderId = null;
            service.ProviderType = null;
            service.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Provider removed from service {ServiceId}", id);

            return new ServiceDto
            {
                ServiceId = service.ServiceId,
                Name = service.Name,
                Description = service.Description,
                Category = service.Category,
                Price = service.Price,
                Status = service.Status,
                ProviderId = service.ProviderId,
                ProviderType = service.ProviderType,
                ProviderName = await GetProviderNameAsync(service.ProviderId, service.ProviderType),
                CreatedAt = service.CreatedAt,
                UpdatedAt = service.UpdatedAt
            };
        }

        public async Task<ServiceStatsDto> GetServiceStatsAsync()
        {
            var services = await _context.Services.ToListAsync();

            var categoryCount = await _context.Services
                .GroupBy(s => s.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Category, x => x.Count);

            var providerTypeCount = await _context.Services
                .Where(s => s.ProviderType != null)
                .GroupBy(s => s.ProviderType!)
                .Select(g => new { ProviderType = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ProviderType, x => x.Count);

            var statusCount = await _context.Services
                .GroupBy(s => s.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            var paidServices = services.Where(s => s.Price.HasValue && s.Price > 0).ToList();
            var averagePrice = paidServices.Any() ? paidServices.Average(s => s.Price!.Value) : 0;

            var recentServices = services.Count(s => s.CreatedAt >= DateTime.UtcNow.AddDays(-30));

            return new ServiceStatsDto
            {
                TotalServices = services.Count,
                ActiveServices = services.Count(s => s.Status == "Active"),
                FreeServices = services.Count(s => !s.Price.HasValue || s.Price == 0),
                PaidServices = paidServices.Count,
                CategoryCount = categoryCount,
                ProviderTypeCount = providerTypeCount,
                StatusCount = statusCount,
                AveragePrice = Math.Round(averagePrice, 2),
                RecentServices = recentServices
            };
        }

        public async Task<IEnumerable<ServiceDto>> GetServicesByProviderAsync(int providerId, string providerType)
        {
            var services = await _context.Services
                .Where(s => s.ProviderId == providerId && s.ProviderType == providerType && s.Status == "Active")
                .OrderBy(s => s.Name)
                .Select(s => new ServiceDto
                {
                    ServiceId = s.ServiceId,
                    Name = s.Name,
                    Description = s.Description,
                    Category = s.Category,
                    Price = s.Price,
                    Status = s.Status,
                    ProviderId = s.ProviderId,
                    ProviderType = s.ProviderType,
                    ProviderName = GetProviderName(s.ProviderId, s.ProviderType),
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                })
                .ToListAsync();

            return services;
        }

        public async Task<IEnumerable<ServiceDto>> GetFreeServicesAsync()
        {
            var services = await _context.Services
                .Where(s => (!s.Price.HasValue || s.Price == 0) && s.Status == "Active")
                .OrderBy(s => s.Category)
                .ThenBy(s => s.Name)
                .Select(s => new ServiceDto
                {
                    ServiceId = s.ServiceId,
                    Name = s.Name,
                    Description = s.Description,
                    Category = s.Category,
                    Price = s.Price,
                    Status = s.Status,
                    ProviderId = s.ProviderId,
                    ProviderType = s.ProviderType,
                    ProviderName = GetProviderName(s.ProviderId, s.ProviderType),
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                })
                .ToListAsync();

            return services;
        }

        public async Task<IEnumerable<ServiceDto>> GetServicesByCategoryAsync(string category)
        {
            var services = await _context.Services
                .Where(s => s.Category == category && s.Status == "Active")
                .OrderBy(s => s.Name)
                .Select(s => new ServiceDto
                {
                    ServiceId = s.ServiceId,
                    Name = s.Name,
                    Description = s.Description,
                    Category = s.Category,
                    Price = s.Price,
                    Status = s.Status,
                    ProviderId = s.ProviderId,
                    ProviderType = s.ProviderType,
                    ProviderName = GetProviderName(s.ProviderId, s.ProviderType),
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                })
                .ToListAsync();

            return services;
        }

        public async Task<bool> ToggleServiceStatusAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return false;

            service.Status = service.Status == "Active" ? "Inactive" : "Active";
            service.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Service {ServiceId} status toggled to {Status}", id, service.Status);
            return true;
        }

        private async Task<bool> ValidateProviderAsync(int providerId, string providerType)
        {
            return providerType.ToLower() switch
            {
                "doctor" => await _context.Doctors.AnyAsync(d => d.DoctorId == providerId),
                "ngo" => await _context.NGOs.AnyAsync(n => n.NGOId == providerId),
                "system" => true, // System services are always valid
                "hospital" => true, // Assuming hospitals are in a separate table
                "clinic" => true, // Assuming clinics are in a separate table
                _ => false
            };
        }

        private string GetProviderName(int? providerId, string? providerType)
        {
            if (!providerId.HasValue || string.IsNullOrEmpty(providerType))
                return "System";

            // This would be implemented based on your actual data structure
            // For now, return a placeholder
            return $"{providerType} #{providerId}";
        }

        private async Task<string> GetProviderNameAsync(int? providerId, string? providerType)
        {
            if (!providerId.HasValue || string.IsNullOrEmpty(providerType))
                return "System";

            try
            {
                return providerType.ToLower() switch
                {
                    "doctor" => await _context.Doctors
                        .Include(d => d.User)
                        .Where(d => d.DoctorId == providerId)
                        .Select(d => $"Dr. {d.User.FirstName} {d.User.LastName}")
                        .FirstOrDefaultAsync() ?? $"Doctor #{providerId}",
                    "ngo" => await _context.NGOs
                        .Where(n => n.NGOId == providerId)
                        .Select(n => n.OrganizationName)
                        .FirstOrDefaultAsync() ?? $"NGO #{providerId}",
                    _ => $"{providerType} #{providerId}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting provider name for {ProviderType} {ProviderId}", providerType, providerId);
                return $"{providerType} #{providerId}";
            }
        }
    }
}