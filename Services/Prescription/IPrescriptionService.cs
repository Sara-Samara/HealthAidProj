// Services/Interfaces/IPrescriptionService.cs
using HealthAidAPI.DTOs;
using HealthAidAPI.Models;

namespace HealthAidAPI.Services.Interfaces
{
    public interface IPrescriptionService
    {
        Task<PagedResult<PrescriptionDto>> GetPrescriptionsAsync(PrescriptionFilterDto filter);
        Task<PrescriptionDto?> GetPrescriptionByIdAsync(int id);
        Task<PrescriptionDto> CreatePrescriptionAsync(CreatePrescriptionDto prescriptionDto);
        Task<PrescriptionDto?> UpdatePrescriptionAsync(int id, UpdatePrescriptionDto prescriptionDto);
        Task<bool> DeletePrescriptionAsync(int id);
        Task<bool> CompletePrescriptionAsync(int id);
        Task<bool> RequestRefillAsync(RefillRequestDto refillRequest);
        Task<bool> ApproveRefillAsync(int prescriptionId, int quantity);
        Task<PrescriptionStatsDto> GetPrescriptionStatsAsync();
        Task<IEnumerable<PrescriptionDto>> GetPatientPrescriptionsAsync(int patientId);
        Task<IEnumerable<PrescriptionDto>> GetDoctorPrescriptionsAsync(int doctorId);
        Task<PatientPrescriptionSummaryDto> GetPatientPrescriptionSummaryAsync(int patientId);
        Task<IEnumerable<PrescriptionDto>> GetExpiringPrescriptionsAsync(int days = 7);
        Task<bool> ValidatePrescriptionAsync(int prescriptionId);
    }
}