using HealthAidAPI.DTOs;
using HealthAidAPI.Models;
using HealthAidAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HealthAidAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(ITransactionService transactionService, ILogger<TransactionsController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }

        /// <summary>
        /// Get all transactions with filtering and pagination
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Finance")]
        [ProducesResponseType(typeof(PagedResult<TransactionDto>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PagedResult<TransactionDto>>> GetTransactions([FromQuery] TransactionFilterDto filter)
        {
            try
            {
                var result = await _transactionService.GetTransactionsAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions");
                return BadRequest(new { message = "An error occurred while retrieving transactions" });
            }
        }

        /// <summary>
        /// Get transaction by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Manager,Finance,User")]
        [ProducesResponseType(typeof(TransactionDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<TransactionDto>> GetTransaction(int id)
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(id);
            if (transaction == null)
                return NotFound(new { message = "Transaction not found" });

            return Ok(transaction);
        }

        /// <summary>
        /// Create a new transaction
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Finance")]
        [ProducesResponseType(typeof(TransactionDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<TransactionDto>> CreateTransaction(CreateTransactionDto transactionDto)
        {
            try
            {
                var transaction = await _transactionService.CreateTransactionAsync(transactionDto);
                return CreatedAtAction(nameof(GetTransaction), new { id = transaction.TransactionId }, transaction);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transaction");
                return BadRequest(new { message = "An error occurred while creating transaction" });
            }
        }

        /// <summary>
        /// Update transaction
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager,Finance")]
        [ProducesResponseType(typeof(TransactionDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<TransactionDto>> UpdateTransaction(int id, UpdateTransactionDto transactionDto)
        {
            try
            {
                var transaction = await _transactionService.UpdateTransactionAsync(id, transactionDto);
                if (transaction == null)
                    return NotFound(new { message = "Transaction not found" });

                return Ok(transaction);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transaction {TransactionId}", id);
                return BadRequest(new { message = "An error occurred while updating transaction" });
            }
        }

        /// <summary>
        /// Delete transaction
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var result = await _transactionService.DeleteTransactionAsync(id);
            if (!result)
                return NotFound(new { message = "Transaction not found" });

            return NoContent();
        }

        /// <summary>
        /// Update transaction status
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Manager,Finance")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateTransactionStatus(int id, [FromBody] UpdateTransactionStatusDto statusDto)
        {
            var result = await _transactionService.UpdateTransactionStatusAsync(id, statusDto.Status);
            if (!result)
                return NotFound(new { message = "Transaction not found" });

            return Ok(new { message = "Transaction status updated successfully" });
        }

        /// <summary>
        /// Get transactions by user
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Manager,Finance,User")]
        [ProducesResponseType(typeof(IEnumerable<TransactionDto>), 200)]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactionsByUser(int userId)
        {
            var transactions = await _transactionService.GetTransactionsByUserAsync(userId);
            return Ok(transactions);
        }

        /// <summary>
        /// Get transactions by consultation
        /// </summary>
        [HttpGet("consultation/{consultationId}")]
        [Authorize(Roles = "Admin,Manager,Finance")]
        [ProducesResponseType(typeof(IEnumerable<TransactionDto>), 200)]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactionsByConsultation(int consultationId)
        {
            var transactions = await _transactionService.GetTransactionsByConsultationAsync(consultationId);
            return Ok(transactions);
        }

        /// <summary>
        /// Get transactions by date range
        /// </summary>
        [HttpGet("date-range")]
        [Authorize(Roles = "Admin,Manager,Finance")]
        [ProducesResponseType(typeof(IEnumerable<TransactionDto>), 200)]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactionsByDateRange(
            [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var transactions = await _transactionService.GetTransactionsByDateRangeAsync(startDate, endDate);
            return Ok(transactions);
        }

        /// <summary>
        /// Get transaction statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin,Manager,Finance")]
        [ProducesResponseType(typeof(TransactionStatsDto), 200)]
        public async Task<ActionResult<TransactionStatsDto>> GetTransactionStats()
        {
            var stats = await _transactionService.GetTransactionStatsAsync();
            return Ok(stats);
        }

        /// <summary>
        /// Get financial summary
        /// </summary>
        [HttpGet("financial-summary")]
        [Authorize(Roles = "Admin,Manager,Finance")]
        [ProducesResponseType(typeof(FinancialSummaryDto), 200)]
        public async Task<ActionResult<FinancialSummaryDto>> GetFinancialSummary()
        {
            var summary = await _transactionService.GetFinancialSummaryAsync();
            return Ok(summary);
        }

        /// <summary>
        /// Get total revenue
        /// </summary>
        [HttpGet("total-revenue")]
        [Authorize(Roles = "Admin,Manager,Finance")]
        [ProducesResponseType(typeof(decimal), 200)]
        public async Task<ActionResult<decimal>> GetTotalRevenue(
            [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var revenue = await _transactionService.GetTotalRevenueAsync(startDate, endDate);
            return Ok(revenue);
        }

        /// <summary>
        /// Get pending transactions
        /// </summary>
        [HttpGet("pending")]
        [Authorize(Roles = "Admin,Manager,Finance")]
        [ProducesResponseType(typeof(IEnumerable<TransactionDto>), 200)]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetPendingTransactions()
        {
            var transactions = await _transactionService.GetPendingTransactionsAsync();
            return Ok(transactions);
        }
    }
}