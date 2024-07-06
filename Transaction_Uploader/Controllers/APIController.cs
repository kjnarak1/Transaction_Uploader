using Microsoft.AspNetCore.Mvc;
using Transaction_Uploader.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Transaction_Uploader.DTO;
using Transaction_Uploader.Services;

namespace Transaction_Uploader.Controllers
{
    [ApiController]
    [Route("api")]
    public class APIController : ControllerBase
    {
        private readonly ITransaction _transaction;
        private readonly IMemoryCache _cache;
        private readonly IFileProcessor _fileProcessor;
        private readonly CacheKeyManager _cacheKeyManager;
        public APIController(ITransaction itransaction, IMemoryCache cache, CacheKeyManager cacheKeyManager, IFileProcessor fileProcessor)
        {
            _transaction = itransaction;
            _cache = cache;
            _cacheKeyManager = cacheKeyManager;
            _fileProcessor = fileProcessor; 
        }

        [HttpGet]
        [Route("Transactions")]
        public async Task<IActionResult> GetTransactions([FromQuery] string? currency, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? status)
        {
            string cacheKey = $"{currency}-{startDate}-{endDate}-{status}";
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<TransactionDto> transactions))
            {
                transactions = await _transaction.GetTransactionsAsync(currency, startDate, endDate, status);
                if (transactions == null || !transactions.Any())
                {
                    return NotFound("No transactions found for the given criteria.");
                }
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(5));

                _cache.Set(cacheKey, transactions, cacheEntryOptions);
                _cacheKeyManager.AddKey(cacheKey);
            }
            return Ok(transactions);
        }

        [HttpPost]
        [Route("Upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            var result = await _fileProcessor.ProcessFileAsync(file);
            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                return BadRequest(result);
            }
            foreach (var key in _cacheKeyManager.GetAllKeys())
            {
                _cache.Remove(key);
            }
            _cacheKeyManager.ClearAllKeys();
            return Ok();
        }
    }
}