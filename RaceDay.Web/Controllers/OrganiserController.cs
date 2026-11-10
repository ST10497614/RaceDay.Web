
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly EventApiService _eventService;

        public OrganiserController(EventApiService eventService)
        {
            _eventService = eventService;
        }

        private bool IsOrganiser()
        {
            return HttpContext.Session.GetString("Role") == "Organiser"
                && !string.IsNullOrEmpty(
                    HttpContext.Session.GetString("Token"));
        }

        private IActionResult LoginRequired()
        {
            return RedirectToAction("Login", "Account");
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!IsOrganiser())
                return LoginRequired();

            string token = HttpContext.Session.GetString("Token")!;

            var events = await _eventService.GetEventsAsync(token);
                                                        
            string userId = HttpContext.Session.GetString("UserId")!;

            var myEvents = events
                .Where(e => e.OrganiserId.ToString() == userId)
                .ToList();

            ViewBag.TotalEvents = myEvents.Count;
            ViewBag.UpcomingEvents = myEvents.Count(
                e => e.Date.Date >= DateTime.Today);

            return View(myEvents);
        }

        public async Task<IActionResult> Events()
        {
            if (!IsOrganiser())
                return LoginRequired();

            string token = HttpContext.Session.GetString("Token")!;

            var events = await _eventService.GetEventsAsync(token);

            string userId = HttpContext.Session.GetString("UserId")!;

            var myEvents = events
                .Where(e => e.OrganiserId.ToString() == userId)
                .ToList();

            return View(myEvents);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsOrganiser())
                return LoginRequired();

            return View(new EventViewModel
            {
                Date = DateTime.Today.AddDays(7)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel model)
        {
            if (!IsOrganiser())
                return LoginRequired();

            if (!ModelState.IsValid)
                return View(model);

            string token = HttpContext.Session.GetString("Token")!;

            model.OrganiserId = int.Parse(
                HttpContext.Session.GetString("UserId")!);

            var createdEventId = await _eventService.CreateEventAsync(model, token);

            if (createdEventId is null)
            {
                ModelState.AddModelError("",
                    "Unable to create event. Check your event details and API response.");
                return View(model);
            }

            if (model.BannerFile is not null && model.BannerFile.Length > 0)
            {
                var uploaded = await _eventService.UploadBannerAsync(createdEventId.Value, model.BannerFile, token);
                if (!uploaded)
                {
                    TempData["Error"] = "Event was created, but its banner could not be uploaded. Edit the event to try again.";
                    return RedirectToAction("Events");
                }
            }

            TempData["Success"] = "Event created successfully.";
            return RedirectToAction("Events");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsOrganiser())
                return LoginRequired();

            string token = HttpContext.Session.GetString("Token")!;

            var model = await _eventService.GetEventAsync(id, token);

            if (model == null ||
                model.OrganiserId.ToString() !=
                HttpContext.Session.GetString("UserId"))
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, EventViewModel model)
        {
            if (!IsOrganiser())
                return LoginRequired();

            if (!ModelState.IsValid)
                return View(model);

            string token = HttpContext.Session.GetString("Token")!;

            var existing = await _eventService.GetEventAsync(id, token);

            if (existing == null ||
                existing.OrganiserId.ToString() !=
                HttpContext.Session.GetString("UserId"))
            {
                return NotFound();
            }

            model.EventId = id;
            model.OrganiserId = existing.OrganiserId;

            bool success = await _eventService.UpdateEventAsync(model, token);

            if (!success)
            {
                ModelState.AddModelError("",
                    "Unable to update event.");
                return View(model);
            }

            if (model.BannerFile is not null && model.BannerFile.Length > 0)
            {
                var uploaded = await _eventService.UploadBannerAsync(id, model.BannerFile, token);
                if (!uploaded)
                {
                    ModelState.AddModelError("", "Event details were saved, but the banner upload failed.");
                    return View(model);
                }
            }

            TempData["Success"] = "Event updated successfully.";
            return RedirectToAction("Events");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsOrganiser())
                return LoginRequired();

            string token = HttpContext.Session.GetString("Token")!;

            var existing = await _eventService.GetEventAsync(id, token);

            if (existing == null ||
                existing.OrganiserId.ToString() !=
                HttpContext.Session.GetString("UserId"))
            {
                return NotFound();
            }

            bool success = await _eventService.DeleteEventAsync(
                id, token);

            TempData[success ? "Success" : "Error"] =
                success ? "Event deleted successfully."
                        : "Unable to delete event.";

            return RedirectToAction("Events");
        }
    }
}

