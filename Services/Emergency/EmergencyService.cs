using AutoMapper; // يفضل استخدام AutoMapper
using HealthAidAPI.Data;
using HealthAidAPI.DTOs.Emergency;
using HealthAidAPI.Models.Emergency;
using Microsoft.AspNetCore.SignalR;
using HealthAidAPI.Hubs;
using Microsoft.EntityFrameworkCore;

namespace HealthAidAPI.Services.Implementations
{
    public class EmergencyService : IEmergencyService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<HealthAidHub> _hubContext;

        public EmergencyService(ApplicationDbContext context , IHubContext<HealthAidHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext; 
        }


        public async Task<List<EmergencyCaseDto>> GetEmergencyCasesAsync(string? status)
        {
            var query = _context.EmergencyCases.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(ec => ec.Status == status);

            // هنا نقوم بالتحويل من Entity إلى DTO يدوياً (أو استخدم AutoMapper)
            return await query
                .Include(ec => ec.Patient).ThenInclude(p => p.User)
                .Select(ec => new EmergencyCaseDto
                {
                    Id = ec.Id,
                    EmergencyType = ec.EmergencyType,
                    Priority = ec.Priority,
                    Location = ec.Location,
                    Status = ec.Status,
                    PatientName = ec.Patient.User.FirstName + " " + ec.Patient.User.LastName,
                    CreatedAt = ec.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<EmergencyCaseDto> CreateEmergencyAlertAsync(CreateEmergencyCaseDto dto)
        {
            var emergencyCase = new EmergencyCase
            {
                PatientId = dto.PatientId,
                EmergencyType = dto.EmergencyType,
                Priority = dto.Priority,
                Location = dto.Location,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Description = dto.Description,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            _context.EmergencyCases.Add(emergencyCase);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveEmergencyAlert", new
            {
                CaseId = emergencyCase.Id,
                Type = emergencyCase.EmergencyType,
                Location = emergencyCase.Location,
                Time = DateTime.UtcNow
            });

            var log = new EmergencyLog
            {
                EmergencyCaseId = emergencyCase.Id,
                Action = "Emergency_Created",
                PerformedBy = dto.PatientId,
                Notes = $"Alert: {dto.EmergencyType}",
                CreatedAt = DateTime.UtcNow
            };
            _context.EmergencyLogs.Add(log);
            await _context.SaveChangesAsync();

            // إرجاع DTO (تبسيط للكود هنا، يفضل استرجاع البيانات كاملة)
            return new EmergencyCaseDto { Id = emergencyCase.Id, Status = "Active" };
        }

        public async Task<EmergencyCaseDto?> AssignResponderAsync(int caseId, AssignResponderDto dto)
        {
            var emergencyCase = await _context.EmergencyCases.FindAsync(caseId);
            if (emergencyCase == null) return null;

            emergencyCase.ResponderId = dto.ResponderId;
            emergencyCase.Status = "Assigned";

            var log = new EmergencyLog
            {
                EmergencyCaseId = emergencyCase.Id,
                Action = "Responder_Assigned",
                PerformedBy = dto.AssignedByUserId,
                Notes = $"Responder {dto.ResponderId} assigned",
                CreatedAt = DateTime.UtcNow
            };
            _context.EmergencyLogs.Add(log);

            await _context.SaveChangesAsync();

            return new EmergencyCaseDto { Id = emergencyCase.Id, Status = "Assigned" };
        }

        // ... تنفيذ باقي الدوال (GetById, GetNearby, CreateResponder)
        public Task<EmergencyCaseDto?> GetEmergencyCaseByIdAsync(int id) => throw new NotImplementedException();
        public Task<List<EmergencyResponder>> GetNearbyRespondersAsync(decimal lat, decimal lon, decimal radius) => throw new NotImplementedException();
        public Task<EmergencyResponder> CreateResponderAsync(EmergencyResponder responder) => throw new NotImplementedException();
    }
}