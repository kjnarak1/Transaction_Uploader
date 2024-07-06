using Microsoft.AspNetCore.Mvc;
using Transaction_Uploader.Models;
using Transaction_Uploader.Services;

namespace Transaction_Uploader.Controllers
{
    public class TransactionController : Controller
    {
        public async Task<IActionResult> Transaction()
        {
            List<Currency> currency = await GetCurrencyCode.LoadCurrenciesAsync("Common-Currency.json");
            return View(currency);
        }
    }
}
