// Services/Interfaces/IAppointmentService.cs
using HealthAidAPI.DTOs;
using HealthAidAPI.Models;

namespace HealthAidAPI.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<PagedResult<AppointmentDto>> GetAppointmentsAsync(AppointmentFilterDto filter);
        Task<AppointmentDto?> GetAppointmentByIdAsync(int id);
        Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto createAppointmentDto);
        Task<AppointmentDto?> UpdateAppointmentAsync(int id, UpdateAppointmentDto updateAppointmentDto);
        Task<AppointmentDto?> RescheduleAppointmentAsync(int id, RescheduleAppointmentDto rescheduleDto);
        Task<bool> CancelAppointmentAsync(int id, string? cancellationReason = null);
        Task<bool> ConfirmAppointmentAsync(int id);
        Task<bool> CompleteAppointmentAsync(int id);
        Task<bool> DeleteAppointmentAsync(int id);
        Task<AppointmentStatsDto> GetAppointmentStatsAsync();
        Task<IEnumerable<AppointmentDto>> GetUpcomingAppointmentsAsync(int days = 7);
        Task<IEnumerable<AppointmentDto>> GetDoctorAppointmentsAsync(int doctorId, DateTime? date = null);
        Task<IEnumerable<AppointmentDto>> GetPatientAppointmentsAsync(int patientId);
        Task<bool> IsTimeSlotAvailableAsync(int doctorId, DateTime dateTime);
    }
}