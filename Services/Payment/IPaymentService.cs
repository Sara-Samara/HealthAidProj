using HealthAidAPI.DTOs.Payments;

namespace HealthAidAPI.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> CreateCheckoutSessionAsync(PaymentRequestDto request);
    }
}