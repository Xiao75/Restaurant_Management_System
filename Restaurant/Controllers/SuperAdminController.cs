using Microsoft.AspNetCore.Mvc;

namespace Restaurant.Controllers
{
    public class SuperAdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
