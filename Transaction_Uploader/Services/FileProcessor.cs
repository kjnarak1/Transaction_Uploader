using System.Globalization;
using System.Xml.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Transaction_Uploader.Data;
using Transaction_Uploader.Interfaces;
using Transaction_Uploader.Models;

namespace Transaction_Uploader.Services
{
    public class FileProcessor : IFileProcessor
    {
        private static readonly HashSet<string> ValidStatusesCSV = new HashSet<string> { "Approved", "Failed", "Finished" };
        private static readonly HashSet<string> ValidStatusesXML = new HashSet<string> { "Approved", "Rejected", "Done" };
        private static readonly Dictionary<string, string> dicStatuses = new Dictionary<string, string>() { { "Approved", "A" }, { "Failed", "R" }, { "Rejected", "R" }, { "Finished", "D" }, { "Done", "D" } };
        private static HashSet<string> ValidStatuses;
        private static HashSet<string> ValidCurrencyCodes;
        private readonly TransactionContext _context;
        private readonly ILogger<FileProcessor> _logger;
        private const long MaxFileSize = 1 * 1024 * 1024;

        public static async Task<HashSet<string>> LoadCurrenciesAsync(string filePath)
        {
            using (var stream = new StreamReader(filePath))
            {
                var json = await stream.ReadToEndAsync();
                var currencies = JsonConvert.DeserializeObject<List<Currency>>(json);
                var currencyCodes = new HashSet<string>();
                foreach (var currency in currencies)
                {
                    currencyCodes.Add(currency.cc);
                }
                return currencyCodes;
            }
        }

        public FileProcessor(TransactionContext context, ILogger<FileProcessor> logger)
        {
            _context = context;
            _logger = logger;
            LoadCurrencyCodesAsync("Common-Currency.json").GetAwaiter().GetResult();
        }

        private async Task LoadCurrencyCodesAsync(string filePath)
        {
            ValidCurrencyCodes = await LoadCurrenciesAsync(filePath);
        }

        public async Task<ValidationResult> ProcessFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogError("Invalid file.");
                return new ValidationResult { ErrorMessage = "Invalid file." };
            }
            if (file.Length > MaxFileSize)
            {
                _logger.LogError("File size exceeds the maximum limit of 1MB.");
                return new ValidationResult { ErrorMessage = "File size exceeds the maximum limit of 1MB." };
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (fileExtension != ".csv" && fileExtension != ".xml")
            {
                _logger.LogError("Unknown format.");
                return new ValidationResult { ErrorMessage = "Unknown format" };
            }

            using (var stream = file.OpenReadStream())
            {
                try
                {
                    if (fileExtension == ".csv")
                    {
                        ValidStatuses = ValidStatusesCSV;
                        return await ProcessCsvFileAsync(stream);
                    }
                    else if (fileExtension == ".xml")
                    {
                        ValidStatuses = ValidStatusesXML;
                        return await ProcessXmlFileAsync(stream);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "File processing failed.");
                    return new ValidationResult { ErrorMessage = $"File processing failed: {ex.Message}" };
                }
            }
            _logger.LogError("Unknown error occurred during file processing.");
            return new ValidationResult { ErrorMessage = "Unknown error occurred during file processing." };
        }

        private async Task<ValidationResult> ProcessCsvFileAsync(Stream stream)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
            };

            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, config))
            {
                var transactions = new List<Transaction>();
                var rowIndex = 0;
                try
                {
                    csv.Read();
                    csv.ReadHeader();
                    while (await csv.ReadAsync())
                    {
                        rowIndex++;
                        try
                        {
                            var tt = csv.GetField("Transaction Date");
                            DateTime date = DateTime.ParseExact(csv.GetField("Transaction Date"), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Invalid Transaction Date.");
                            return new ValidationResult { ErrorMessage = "Invalid Transaction Date." };
                        };
                        var record = new Transaction
                        {
                            TransactionId = csv.GetField("Transaction Identificator"),
                            Amount = csv.GetField<decimal>("Amount"),
                            CurrencyCode = csv.GetField("Currency Code"),
                            TransactionDate = DateTime.ParseExact(csv.GetField("Transaction Date"), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                            Status = csv.GetField("Status")
                        };
                        var validationError = ValidateTransaction(record);
                        if (!string.IsNullOrEmpty(validationError))
                        {
                            _logger.LogError($"Row {rowIndex}: Validation error - {validationError}");
                            return new ValidationResult { ErrorMessage = validationError };
                        }
                        record.Status = dicStatuses[record.Status];
                        transactions.Add(record);
                    }
                    await SaveTransactionsToDatabaseAsync(transactions);
                }
                catch (CsvHelperException ex)
                {
                    _logger.LogError(ex, $"Row {rowIndex}: CSV parsing error.");
                    return new ValidationResult { ErrorMessage = $"CSV parsing error: {ex.Message}" };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Row {rowIndex}: An error occurred.");
                    return new ValidationResult { ErrorMessage = $"An error occurred: {ex.Message}" };
                }
            }

            return new ValidationResult { IsValid = true, ErrorMessage = "CSV file processed successfully." };
        }

        private async Task<ValidationResult> ProcessXmlFileAsync(Stream stream)
        {
            XDocument xmlDoc;
            using (var reader = new StreamReader(stream))
            {
                var xmlString = await reader.ReadToEndAsync();
                xmlDoc = XDocument.Parse(xmlString);
            }
            var transactions = new List<Transaction>();
            var rowIndex = 0;
            try
            {
                foreach (var element in xmlDoc.Descendants("Transaction"))
                {
                    rowIndex++;
                    var record = new Transaction
                    {
                        TransactionId = element.Attribute("id")?.Value,
                        Amount = decimal.Parse(element.Element("PaymentDetails")?.Element("Amount")?.Value),
                        CurrencyCode = element.Element("PaymentDetails")?.Element("CurrencyCode")?.Value,
                        TransactionDate = DateTime.Parse(element.Element("TransactionDate")?.Value, null, DateTimeStyles.RoundtripKind),
                        Status = element.Element("Status")?.Value
                    };

                    var validationError = ValidateTransaction(record);
                    if (!string.IsNullOrEmpty(validationError))
                    {
                        _logger.LogError($"Row {rowIndex}: Validation error - {validationError}");
                        return new ValidationResult { ErrorMessage = validationError };
                    }
                    record.Status = dicStatuses[record.Status];
                    transactions.Add(record);
                }
                await SaveTransactionsToDatabaseAsync(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Row {rowIndex}: An error occurred while saving transactions.");
                return new ValidationResult { ErrorMessage = $"An error occurred: {ex.Message}" };
            }

            return new ValidationResult { IsValid = true, ErrorMessage = "XML file processed successfully." };
        }

        private string ValidateTransaction(Transaction transaction)
        {
            if (string.IsNullOrEmpty(transaction.TransactionId) || transaction.TransactionId.Length > 50)
            {
                return "Invalid Transaction Id.";
            }

            if (transaction.Amount <= 0)
            {
                return "Invalid Amount.";
            }

            if (string.IsNullOrEmpty(transaction.CurrencyCode) || !ValidCurrencyCodes.Contains(transaction.CurrencyCode))
            {
                return "Invalid Currency Code.";
            }

            if (transaction.TransactionDate == default)
            {
                return "Invalid Transaction Date.";
            }

            if (!ValidStatuses.Contains(transaction.Status))
            {
                return "Invalid Status.";
            }

            return null;
        }

        private async Task SaveTransactionsToDatabaseAsync(IEnumerable<Transaction> transactions)
        {
            foreach (var transaction in transactions)
            {
                var existingTransaction = await _context.Transaction
                    .FirstOrDefaultAsync(t => t.TransactionId == transaction.TransactionId);

                if (existingTransaction != null)
                {
                    existingTransaction.Amount = transaction.Amount;
                    existingTransaction.CurrencyCode = transaction.CurrencyCode;
                    existingTransaction.TransactionDate = transaction.TransactionDate;
                    existingTransaction.Status = transaction.Status;
                }
                else
                {
                    await _context.Transaction.AddAsync(transaction);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
