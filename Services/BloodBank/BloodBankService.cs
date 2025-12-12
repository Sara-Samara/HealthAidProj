using HealthAidAPI.Data;
using HealthAidAPI.Models.Extras;
using HealthAidAPI.DTOs.Extras;
using HealthAidAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthAidAPI.Services.Implementations
{
    public class BloodBankService : IBloodBankService
    {
        private readonly ApplicationDbContext _context;

        public BloodBankService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BloodRequestDto> CreateRequestAsync(CreateBloodRequestDto dto, int userId)
        {
            var req = new BloodRequest
            {
                BloodType = dto.BloodType,
                HospitalName = dto.HospitalName,
                ContactNumber = dto.ContactNumber,
                UrgencyLevel = dto.UrgencyLevel,
                RequesterId = userId
            };

            _context.BloodRequests.Add(req);
            await _context.SaveChangesAsync();

            return new BloodRequestDto
            {
                Id = req.Id,
                BloodType = req.BloodType,
                HospitalName = req.HospitalName,
                ContactNumber = req.ContactNumber,
                UrgencyLevel = req.UrgencyLevel,
                CreatedAt = req.CreatedAt
            };
        }

        public async Task<List<BloodRequestDto>> GetActiveRequestsAsync()
        {
            return await _context.BloodRequests
                .Where(b => !b.IsFulfilled)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new BloodRequestDto
                {
                    Id = b.Id,
                    BloodType = b.BloodType,
                    HospitalName = b.HospitalName,
                    ContactNumber = b.ContactNumber,
                    UrgencyLevel = b.UrgencyLevel,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();
        }
    }
}