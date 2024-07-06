using Microsoft.AspNetCore.Mvc;
using Transaction_Uploader.Models;

namespace Transaction_Uploader.Controllers
{
    public class TransactionController : Controller
    {
        public IActionResult Transaction()
        {
            ViewModel result = new ViewModel();
            List<Transaction> transactions = new List<Transaction>();
            List<Currency> currency = new List<Currency>();

            result.Transactions = transactions;
            result.Currencies = currency;
            return View(result);
        }
    }
}
