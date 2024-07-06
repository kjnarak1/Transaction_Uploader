using Microsoft.EntityFrameworkCore;
using Transaction_Uploader.Data;
using Transaction_Uploader.DTO;
using Transaction_Uploader.Interfaces;
using Transaction_Uploader.Models;

namespace Transaction_Uploader.Repositories
{
    public class TransactionRepository : ITransaction
    {
        private readonly TransactionContext _context;

        public TransactionRepository(TransactionContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TransactionDto>> GetTransactionsAsync(string? currency, DateTime? start_date, DateTime? end_date, string? status)
        {
            var query = _context.Transaction.AsQueryable();

            if (!string.IsNullOrEmpty(currency))
            {
                query = query.Where(t => t.CurrencyCode == currency);
            }

            if (start_date.HasValue)
            {
                query = query.Where(t => t.TransactionDate >= start_date.Value);
            }

            if (end_date.HasValue)
            {
                query = query.Where(t => t.TransactionDate <= end_date.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status == status);
            }
            var list = await query.Select(t => new TransactionDto
            {
                Id = t.TransactionId,
                Payment = $"{t.Amount:0.00} {t.CurrencyCode}",
                Status = t.Status
            }).ToListAsync();
            return list;
        }
    }
}
