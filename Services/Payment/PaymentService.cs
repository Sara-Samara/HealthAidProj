using HealthAidAPI.DTOs.Payments;
using HealthAidAPI.Services.Interfaces;
using Stripe;
using Stripe.Checkout;

namespace HealthAidAPI.Services.Implementations
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;

        public PaymentService(IConfiguration configuration)
        {
            _configuration = configuration;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        public async Task<PaymentResponseDto> CreateCheckoutSessionAsync(PaymentRequestDto dto)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(dto.Amount * 100), // Stripe يأخذ المبلغ بالسنت
                            Currency = dto.Currency,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Donation for Case #{dto.DonationId}",
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = "https://localhost:7068/api/Payment/success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = "https://localhost:7068/api/Payment/cancel",
                Metadata = new Dictionary<string, string>
                {
                    { "DonationId", dto.DonationId.ToString() }
                }
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            return new PaymentResponseDto
            {
                PaymentUrl = session.Url,
                SessionId = session.Id
            };
        }
    }
}