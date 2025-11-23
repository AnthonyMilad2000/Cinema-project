using Microsoft.AspNetCore.Mvc;

namespace Cinema.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class CheckoutController : Controller
    {
        public IActionResult Success()
        {
            return View();
        }
    }
}
