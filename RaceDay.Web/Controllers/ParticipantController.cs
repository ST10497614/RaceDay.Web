
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

        // Check whether the logged-in user is a participant.
        private bool IsParticipant()
        {
            return string.Equals(
                       HttpContext.Session.GetString("Role"),
                       "Participant",
                       StringComparison.OrdinalIgnoreCase)
                   && !string.IsNullOrEmpty(
                       HttpContext.Session.GetString("Token"));
        }

        // PARTICIPANT DASHBOARD
        // Login is required.
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            if (!IsParticipant())
            {
                return RedirectToAction("Login", "Account");
            }

            string token = HttpContext.Session.GetString("Token")!;

            var events = await _eventService.GetEventsAsync(token);

            var upcomingEvents = events
                .Where(e => e.Date.Date >= DateTime.Today)
                .OrderBy(e => e.Date)
                .ToList();

            return View(upcomingEvents);
        }

        // EXPLORE EVENTS
        // Public access: login is not required.
        // Both URLs are supported.
        [HttpGet("/Events")]
        [HttpGet("/Participant/Events")]
        public async Task<IActionResult> Events()
        {
            var events = await _eventService.GetEventsAsync();

            // Display ALL events, including past events.
            var allEvents = events
                .OrderBy(e => e.Date)
                .ToList();

            return View(allEvents);
        }

        // EVENT DETAILS
        // Public access: login is not required.
        [HttpGet]
        public async Task<IActionResult> EventDetails(int id)
        {
            var model = await _eventService.GetEventAsync(id);

            if (model == null)
            {
                return NotFound("Event not found.");
            }

            return View(model);
        }
    }
}
