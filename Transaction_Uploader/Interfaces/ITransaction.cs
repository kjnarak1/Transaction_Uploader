﻿using Transaction_Uploader.DTO;
using Transaction_Uploader.Models;

namespace Transaction_Uploader.Interfaces
{
    public interface ITransaction
    {
        Task<IEnumerable<TransactionDto>> GetTransactionsAsync(string? currency, DateTime? start_date, DateTime? end_date, string? status);
    }
}