using Microsoft.AspNetCore.Mvc;

namespace Transaction_Uploader.Controllers
{
    public class TransactionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
