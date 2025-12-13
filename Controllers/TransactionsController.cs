using HealthAidAPI.DTOs.Transactions;
using HealthAidAPI.Helpers;
using HealthAidAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(
            ITransactionService transactionService,
            ILogger<TransactionsController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        // ============================
        // 🔐 Auth Helpers
        // ============================
        private int GetCurrentUserId()
        {
            var claim =
                User.FindFirst("id") ??
                User.FindFirst(ClaimTypes.NameIdentifier) ??
                User.FindFirst("nameid");

            if (claim == null || !int.TryParse(claim.Value, out int userId))
                throw new UnauthorizedAccessException("User not logged in");

            return userId;
        }

        private bool IsAdmin =>
            User.IsInRole("Admin") ||
            User.IsInRole("Manager") ||
            User.IsInRole("Finance");

        private IActionResult UnauthorizedResponse(string msg = "You must be logged in.")
            => Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = msg
            });


        [HttpGet]
        public async Task<IActionResult> GetTransactions([FromQuery] TransactionFilterDto filter)
        {
            try
            {
                int currentUserId = GetCurrentUserId();

                // ✅ دايمًا اربط بالمستخدم المسجل
                filter.UserId = currentUserId;

                var result = await _transactionService.GetTransactionsAsync(filter);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Transactions retrieved successfully",
                    Data = result
                });
            }
            catch (UnauthorizedAccessException)
            {
                return UnauthorizedResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransaction(int id)
        {
            try
            {
                int currentUserId = GetCurrentUserId();

                var transaction = await _transactionService.GetTransactionByIdAsync(id);
                if (transaction == null)
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Transaction not found"
                    });

                if (!IsAdmin && transaction.UserId != currentUserId)
                    return Forbid();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Transaction retrieved successfully",
                    Data = transaction
                });
            }
            catch (UnauthorizedAccessException)
            {
                return UnauthorizedResponse();
            }
        }

        
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> CreateTransaction(CreateTransactionDto dto)
        {
            try
            {
                int currentUserId = GetCurrentUserId();
                dto.UserId = currentUserId;

      
                int refs =
                    (dto.ConsultationId != null ? 1 : 0) +
                    (dto.DonationId != null ? 1 : 0) +
                    (dto.MedicineRequestId != null ? 1 : 0);

                if (refs != 1)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Transaction must be linked to exactly one of Consultation, Donation, or MedicineRequest."
                    });
                }

                var transaction = await _transactionService.CreateTransactionAsync(dto);

                return CreatedAtAction(nameof(GetTransaction),
                    new { id = transaction.TransactionId },
                    new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Transaction created successfully",
                        Data = transaction
                    });
            }
            catch (UnauthorizedAccessException)
            {
                return UnauthorizedResponse();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transaction");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to create transaction"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, UpdateTransactionDto dto)
        {
            try
            {
                int currentUserId = GetCurrentUserId();

                var transaction = await _transactionService.GetTransactionByIdAsync(id);
                if (transaction == null)
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Transaction not found"
                    });

                if (!IsAdmin && transaction.UserId != currentUserId)
                    return Forbid();

                var updated = await _transactionService.UpdateTransactionAsync(id, dto, currentUserId);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Transaction updated successfully",
                    Data = updated
                });
            }
            catch (UnauthorizedAccessException)
            {
                return UnauthorizedResponse();
            }
        }

        // ============================
        // DELETE Transaction
        // ============================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            try
            {
                int currentUserId = GetCurrentUserId();

                var transaction = await _transactionService.GetTransactionByIdAsync(id);
                if (transaction == null)
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Transaction not found"
                    });

                if (!IsAdmin && transaction.UserId != currentUserId)
                    return Forbid();

                await _transactionService.DeleteTransactionAsync(id, currentUserId);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Transaction deleted successfully"
                });
            }
            catch (UnauthorizedAccessException)
            {
                return UnauthorizedResponse();
            }
        }

        // ============================
        // MY Transactions
        // ============================
        [HttpGet("my")]
        public async Task<IActionResult> GetMyTransactions()
        {
            try
            {
                int currentUserId = GetCurrentUserId();

                var transactions = await _transactionService.GetTransactionsByUserAsync(currentUserId);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Your transactions retrieved successfully",
                    Data = transactions
                });
            }
            catch (UnauthorizedAccessException)
            {
                return UnauthorizedResponse();
            }
        }
    }
}
