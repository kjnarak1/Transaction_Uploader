using Microsoft.AspNetCore.Mvc;
using Transaction_Uploader.Repositories;
using Transaction_Uploader.Interfaces;
using Transaction_Uploader.Services;

namespace Transaction_Uploader.Controllers
{
    [ApiController]
    [Route("api")]
    public class APIController : ControllerBase
    {
        private readonly ITransaction _transaction;
        public APIController(ITransaction itransaction)
        {
            _transaction = itransaction;
        }

        [HttpGet]
        [Route("Transactions")]
        public async Task<IActionResult> GetTransactions([FromQuery] string? currency, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? status)
        {
            var transactions = await _transaction.GetTransactionsAsync(currency, startDate, endDate, status);
            if (transactions == null || !transactions.Any())
            {
                return NotFound("No transactions found for the given criteria.");
            }
            return Ok(transactions);
        }
    }
}
