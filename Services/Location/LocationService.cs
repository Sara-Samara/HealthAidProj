using Microsoft.EntityFrameworkCore;
using HealthAidAPI.Data;
using HealthAidAPI.Models.Location;
using HealthAidAPI.Services;

// DTOs Namespaces
using HealthAidAPI.DTOs.Locations;
using HealthAidAPI.DTOs.MedicalFacilities; 
using HealthAidAPI.DTOs.Emergency;         

namespace HealthAidAPI.Services
{
    public class LocationService : ILocationService
    {
        private readonly ApplicationDbContext _context;

        public LocationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserLocationDto> UpdateUserLocationAsync(UpdateUserLocationDto dto)
        {
            var location = new UserLocation
            {
                UserId = dto.UserId,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Address = dto.Address,
                City = dto.City,
                Region = dto.Region,
                Accuracy = dto.Accuracy,
                LocationType = dto.LocationType,
                IsPrimary = dto.IsPrimary,
                CreatedAt = DateTime.UtcNow
            };

            if (dto.IsPrimary)
            {
                var existingPrimaries = await _context.UserLocations
                    .Where(ul => ul.UserId == dto.UserId && ul.IsPrimary)
                    .ToListAsync();

                foreach (var loc in existingPrimaries)
                {
                    loc.IsPrimary = false;
                }
            }

            _context.UserLocations.Add(location);
            await _context.SaveChangesAsync();

            return MapToUserLocationDto(location);
        }

        public async Task<List<UserLocationDto>> GetUserLocationsAsync(int userId)
        {
            var locations = await _context.UserLocations
                .Where(ul => ul.UserId == userId)
                .OrderByDescending(ul => ul.CreatedAt)
                .ToListAsync();

            return locations.Select(MapToUserLocationDto).ToList();
        }

        public async Task<EmergencyServicesResponseDto> GetEmergencyServicesAsync(decimal latitude, decimal longitude, decimal radius)
        {
            
            var hospitals = await _context.MedicalFacilities
                .Where(f => f.Type == "Hospital" && f.IsActive && f.Verified)
                .Take(10) 
                .Select(h => new MedicalFacilityDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    Type = h.Type,
                    Address = h.Address,
                    Latitude = h.Latitude,
                    Longitude = h.Longitude,
                    ContactNumber = h.ContactNumber,
                    IsActive = h.IsActive
                })
                .ToListAsync();

            var responders = await _context.EmergencyResponders
                .Where(r => r.IsAvailable)
                .Include(r => r.User)
                .Take(10)
                .Select(r => new EmergencyResponderDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = $"{r.User.FirstName} {r.User.LastName}",
                    Type = r.Type,
                    Location = r.Location,
                    Latitude = r.Latitude,
                    Longitude = r.Longitude,
                    ContactNumber = r.ContactNumber,
                    IsAvailable = r.IsAvailable
                })
                .ToListAsync();

            return new EmergencyServicesResponseDto
            {
                Hospitals = hospitals,
                EmergencyResponders = responders
            };
        }

        public async Task<List<ServiceAreaDto>> GetServiceAreasAsync()
        {
            var areas = await _context.ServiceAreas
                .Where(sa => sa.IsActive)
                .ToListAsync();

            return areas.Select(MapToServiceAreaDto).ToList();
        }

        public async Task<ServiceAreaDto> CreateServiceAreaAsync(CreateServiceAreaDto dto)
        {
            var area = new ServiceArea
            {
                AreaName = dto.AreaName,
                City = dto.City,
                Region = dto.Region,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Radius = dto.Radius,
                Description = dto.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.ServiceAreas.Add(area);
            await _context.SaveChangesAsync();

            return MapToServiceAreaDto(area);
        }

        // ==========================================================
        // Helper Methods (Mappers) - لتحويل الـ Models إلى DTOs
        // ==========================================================

        private static UserLocationDto MapToUserLocationDto(UserLocation entity)
        {
            return new UserLocationDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                Address = entity.Address,
                City = entity.City,
                Region = entity.Region,
                LocationType = entity.LocationType,
                IsPrimary = entity.IsPrimary,
                CreatedAt = entity.CreatedAt
            };
        }

        private static ServiceAreaDto MapToServiceAreaDto(ServiceArea entity)
        {
            return new ServiceAreaDto
            {
                Id = entity.Id,
                AreaName = entity.AreaName,
                City = entity.City,
                Region = entity.Region,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                Radius = entity.Radius,
                Description = entity.Description,
                IsActive = entity.IsActive
            };
        }
    }
}