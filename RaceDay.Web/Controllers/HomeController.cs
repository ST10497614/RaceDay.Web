
using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
