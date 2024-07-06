using Microsoft.AspNetCore.Mvc;
using Transaction_Uploader.Models;
using Newtonsoft.Json;
using Transaction_Uploader.Services;

namespace Transaction_Uploader.Controllers
{
    public class TransactionController : Controller
    {
        public async Task<IActionResult> Transaction()
        {
            ViewModel result = new ViewModel();
            List<Transaction> transactions = new List<Transaction>();
            List<Currency> currency = await GetCurrencyCode.LoadCurrenciesAsync("Common-Currency.json");

            result.Transactions = transactions;
            result.Currencies = currency;
            return View(result);
        }
    }
}
