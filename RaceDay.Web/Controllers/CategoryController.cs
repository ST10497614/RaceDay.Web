
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly CategoryEnrolmentApiService _service;

        public CategoryController(
            CategoryEnrolmentApiService service)
        {
            _service = service;
        }

        private bool IsOrganiser()
        {
            return HttpContext.Session.GetString("Role") == "Organiser"
                && !string.IsNullOrWhiteSpace(
                    HttpContext.Session.GetString("Token"));
        }

        public async Task<IActionResult> Index()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var categories = await _service.GetCategoriesAsync();

            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            return View(new CategoryViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            var token = HttpContext.Session.GetString("Token")!;

            var success = await _service.CreateCategoryAsync(
                model, token);

            if (!success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The API could not create the category. Check the event ID and your organiser login.");

                return View(model);
            }

            TempData["Success"] = "Category created successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
