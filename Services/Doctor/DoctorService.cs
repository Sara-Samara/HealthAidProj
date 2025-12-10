// Services/Implementations/DoctorService.cs
using AutoMapper;
using HealthAidAPI.Data;
using HealthAidAPI.DTOs.Users;
using HealthAidAPI.Models;
using HealthAidAPI.Helpers;
using HealthAidAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using HealthAidAPI.DTOs.Doctors;

namespace HealthAidAPI.Services.Implementations
{
    public class DoctorService : IDoctorService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<DoctorService> _logger;

        public DoctorService(ApplicationDbContext context, IMapper mapper, ILogger<DoctorService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<DoctorDto>> GetDoctorsAsync(DoctorFilterDto filter)
        {
            try
            {
                var query = _context.Doctors
                    .Include(d => d.User)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(filter.Specialization))
                    query = query.Where(d => d.Specialization.Contains(filter.Specialization));

                if (!string.IsNullOrEmpty(filter.Search))
                {
                    query = query.Where(d =>
                        d.User.FirstName.Contains(filter.Search) ||
                        d.User.LastName.Contains(filter.Search) ||
                        d.Specialization.Contains(filter.Search) ||
                        d.Bio != null && d.Bio.Contains(filter.Search));
                }

                if (filter.MinYearsExperience.HasValue)
                    query = query.Where(d => d.YearsExperience >= filter.MinYearsExperience.Value);

                if (filter.MaxYearsExperience.HasValue)
                    query = query.Where(d => d.YearsExperience <= filter.MaxYearsExperience.Value);

                if (filter.IsAvailable.HasValue)
                {
                    // Assuming we have an IsAvailable field or we can determine based on consultations
                    query = query.Where(d => true); // Adjust based on actual availability logic
                }

                if (!string.IsNullOrEmpty(filter.City))
                    query = query.Where(d => d.User.City == filter.City);

                if (!string.IsNullOrEmpty(filter.Country))
                    query = query.Where(d => d.User.Country == filter.Country);

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "experience" => filter.SortDesc ?
                        query.OrderByDescending(d => d.YearsExperience) :
                        query.OrderBy(d => d.YearsExperience),
                    "specialization" => filter.SortDesc ?
                        query.OrderByDescending(d => d.Specialization) :
                        query.OrderBy(d => d.Specialization),
                    "name" => filter.SortDesc ?
                        query.OrderByDescending(d => d.User.LastName).ThenByDescending(d => d.User.FirstName) :
                        query.OrderBy(d => d.User.LastName).ThenBy(d => d.User.FirstName),
                    _ => filter.SortDesc ?
                        query.OrderByDescending(d => d.DoctorId) :
                        query.OrderBy(d => d.DoctorId)
                };

                var totalCount = await query.CountAsync();

                var doctors = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(d => new DoctorDto
                    {
                        DoctorId = d.DoctorId,
                        Specialization = d.Specialization,
                        YearsExperience = d.YearsExperience,
                        Bio = d.Bio,
                        LicenseNumber = d.LicenseNumber,
                        AvailableHours = d.AvailableHours,
                        UserId = d.UserId,
                        User = _mapper.Map<UserDto>(d.User),
                        TotalConsultations = d.Consultations.Count,
                        IsAvailable = true // Implement actual availability logic
                    })
                    .ToListAsync();

                return new PagedResult<DoctorDto>(doctors, totalCount, filter.Page, filter.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving doctors with filter {@Filter}", filter);
                throw;
            }
        }

        public async Task<DoctorDto?> GetDoctorByIdAsync(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DoctorId == id);

            if (doctor == null) return null;

            var doctorDto = _mapper.Map<DoctorDto>(doctor);
            // Add additional data like ratings, consultations count, etc.
            doctorDto.TotalConsultations = await _context.Consultations
                .CountAsync(c => c.DoctorId == id);

            return doctorDto;
        }

        public async Task<DoctorDto?> GetDoctorByUserIdAsync(int userId)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            return doctor == null ? null : _mapper.Map<DoctorDto>(doctor);
        }

        public async Task<DoctorDto> CreateDoctorAsync(CreateDoctorDto doctorDto)
        {
            // Check if license number already exists
            if (await LicenseNumberExistsAsync(doctorDto.LicenseNumber))
                throw new ArgumentException("License number already exists");

            // Check if user exists and has doctor role
            var user = await _context.Users.FindAsync(doctorDto.UserId);
            if (user == null)
                throw new ArgumentException("User not found");

            // Update user role to Doctor if not already
            if (user.Role != "Doctor")
            {
                user.Role = "Doctor";
                _context.Users.Update(user);
            }

            var doctor = new Doctor
            {
                Specialization = doctorDto.Specialization,
                YearsExperience = doctorDto.YearsExperience,
                Bio = doctorDto.Bio,
                LicenseNumber = doctorDto.LicenseNumber,
                AvailableHours = doctorDto.AvailableHours,
                UserId = doctorDto.UserId
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Doctor {DoctorId} created successfully for user {UserId}",
                doctor.DoctorId, doctorDto.UserId);

            return await GetDoctorByIdAsync(doctor.DoctorId);
        }

        public async Task<DoctorDto?> UpdateDoctorAsync(int id, UpdateDoctorDto doctorDto)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DoctorId == id);

            if (doctor == null) return null;

            // Update only provided fields
            if (!string.IsNullOrEmpty(doctorDto.Specialization))
                doctor.Specialization = doctorDto.Specialization;

            if (doctorDto.YearsExperience.HasValue)
                doctor.YearsExperience = doctorDto.YearsExperience.Value;

            if (doctorDto.Bio != null)
                doctor.Bio = doctorDto.Bio;

            if (!string.IsNullOrEmpty(doctorDto.LicenseNumber))
            {
                // Check if new license number already exists (excluding current doctor)
                if (await _context.Doctors.AnyAsync(d =>
                    d.LicenseNumber == doctorDto.LicenseNumber && d.DoctorId != id))
                    throw new ArgumentException("License number already exists");

                doctor.LicenseNumber = doctorDto.LicenseNumber;
            }

            if (doctorDto.AvailableHours != null)
                doctor.AvailableHours = doctorDto.AvailableHours;

            await _context.SaveChangesAsync();
            return _mapper.Map<DoctorDto>(doctor);
        }

        public async Task<bool> DeleteDoctorAsync(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return false;

            // Update user role if this is their only doctor profile
            var user = await _context.Users.FindAsync(doctor.UserId);
            if (user != null && user.Role == "Doctor")
            {
                // Check if user has other doctor profiles
                var otherDoctorProfiles = await _context.Doctors
                    .CountAsync(d => d.UserId == doctor.UserId && d.DoctorId != id);

                if (otherDoctorProfiles == 0)
                {
                    user.Role = "Patient"; // Or whatever default role
                    _context.Users.Update(user);
                }
            }

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Doctor {DoctorId} deleted successfully", id);
            return true;
        }

        public async Task<bool> ToggleAvailabilityAsync(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return false;

            // Implement availability toggle logic
            // This could involve updating a field or determining based on schedule
            return true;
        }

        public async Task<IEnumerable<string>> GetSpecializationsAsync()
        {
            return await _context.Doctors
                .Select(d => d.Specialization)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();
        }

        public async Task<DoctorStatsDto> GetDoctorStatsAsync()
        {
            var totalDoctors = await _context.Doctors.CountAsync();
            var availableDoctors = await _context.Doctors.CountAsync(); // Adjust based on availability logic

            var specializationsCount = await _context.Doctors
                .GroupBy(d => d.Specialization)
                .Select(g => new { Specialization = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Specialization, x => x.Count);

            var averageExperience = await _context.Doctors
                .AverageAsync(d => (double)d.YearsExperience);

            var newDoctorsThisMonth = await _context.Doctors
                .Where(d => d.User.CreatedAt >= DateTime.UtcNow.AddMonths(-1))
                .CountAsync();

            return new DoctorStatsDto
            {
                TotalDoctors = totalDoctors,
                AvailableDoctors = availableDoctors,
                SpecializationsCount = specializationsCount,
                AverageExperience = Math.Round(averageExperience, 1),
                NewDoctorsThisMonth = newDoctorsThisMonth
            };
        }

        public async Task<IEnumerable<DoctorDto>> GetAvailableDoctorsAsync()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Where(d => true) // Add actual availability filter
                .Select(d => _mapper.Map<DoctorDto>(d))
                .ToListAsync();

            return doctors;
        }

        public async Task<IEnumerable<DoctorDto>> GetDoctorsBySpecializationAsync(string specialization)
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Where(d => d.Specialization == specialization)
                .Select(d => _mapper.Map<DoctorDto>(d))
                .ToListAsync();

            return doctors;
        }

        public async Task<bool> LicenseNumberExistsAsync(string licenseNumber)
        {
            return await _context.Doctors
                .AnyAsync(d => d.LicenseNumber == licenseNumber);
        }
    }
}