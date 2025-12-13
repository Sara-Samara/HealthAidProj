using HealthAidAPI.Data;
using HealthAidAPI.DTOs.Payments;
using HealthAidAPI.Helpers;
using HealthAidAPI.Models;
using HealthAidAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ApplicationDbContext _context; // لتحديث حالة التبرع

        public PaymentController(IPaymentService paymentService, ApplicationDbContext context)
        {
            _paymentService = paymentService;
            _context = context;
        }

        [HttpPost("checkout")]
        public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> CreateCheckout(PaymentRequestDto request)
        {
            try
            {
                var result = await _paymentService.CreateCheckoutSessionAsync(request);
                return Ok(new ApiResponse<PaymentResponseDto> { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<string> { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("success")]
        public async Task<IActionResult> PaymentSuccess(string session_id)
        {

            return Ok("Payment Successful! Thank you for your donation.");
        }
    }
}