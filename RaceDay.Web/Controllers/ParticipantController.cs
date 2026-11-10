using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly EventApiService _eventService;

        public ParticipantController(EventApiService eventService)
        {
            _eventService = eventService;
        }

        private bool IsParticipant()
        {
            return HttpContext.Session.GetString("Role") == "Participant"
                && !string.IsNullOrEmpty(
                    HttpContext.Session.GetString("Token"));
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Account");

            string token = HttpContext.Session.GetString("Token")!;

            var events = await _eventService.GetEventsAsync(token);

            var upcoming = events
                .Where(e => e.Date.Date >= DateTime.Today)
                .OrderBy(e => e.Date)
                .ToList();

            return View(upcoming);
        }

        public async Task<IActionResult> Events()
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Account");

            string token = HttpContext.Session.GetString("Token")!;

            var events = await _eventService.GetEventsAsync(token);

            return View(events
                .Where(e => e.Date.Date >= DateTime.Today)
                .OrderBy(e => e.Date)
                .ToList());
        }

        public async Task<IActionResult> EventDetails(int id)
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Account");

            string token = HttpContext.Session.GetString("Token")!;

            var model = await _eventService.GetEventAsync(id, token);

            if (model == null)
                return NotFound();

            return View(model);
        }
    }
}