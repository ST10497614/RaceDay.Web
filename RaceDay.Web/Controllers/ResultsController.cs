
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class ResultsController : Controller
    {
        private readonly ResultApiService _resultService;

        public ResultsController(ResultApiService resultService)
        {
            _resultService = resultService;
        }

        private bool IsLoggedInAs(string role)
        {
            return HttpContext.Session.GetString("Role") == role
                && !string.IsNullOrWhiteSpace(
                    HttpContext.Session.GetString("Token"));
        }

        // ORGANISER: View results for an event.
        [HttpGet]
        public async Task<IActionResult> EventResults(int eventId)
        {
            if (!IsLoggedInAs("Organiser"))
                return RedirectToAction("Login", "Account");

            if (eventId <= 0)
                return BadRequest();

            var token = HttpContext.Session.GetString("Token")!;

            var results = await _resultService.GetEventResultsAsync(
                eventId, token);

            ViewBag.EventId = eventId;

            return View(results);
        }

        // ORGANISER: Display the result entry form.
        [HttpGet]
        public IActionResult Enter(int eventId)
        {
            if (!IsLoggedInAs("Organiser"))
                return RedirectToAction("Login", "Account");

            if (eventId <= 0)
                return BadRequest();

            ViewBag.EventId = eventId;

            return View(new ResultEntryViewModel());
        }

        // ORGANISER: Submit a participant's result.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enter(
            int eventId,
            ResultEntryViewModel model)
        {
            if (!IsLoggedInAs("Organiser"))
                return RedirectToAction("Login", "Account");

            if (eventId <= 0)
                return BadRequest();

            ViewBag.EventId = eventId;

            if (!ModelState.IsValid)
                return View(model);

            var token = HttpContext.Session.GetString("Token")!;

            var success = await _resultService.SubmitResultAsync(
                eventId, model, token);

            if (!success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The API could not save the result. Check the event, enrolment and result details.");

                return View(model);
            }

            TempData["SuccessMessage"] =
                "Race result published successfully.";

            return RedirectToAction(
                nameof(EventResults),
                new { eventId });
        }

        // PARTICIPANT: View personal results.
        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
            if (!IsLoggedInAs("Participant"))
                return RedirectToAction("Login", "Account");

            var token = HttpContext.Session.GetString("Token")!;

            var results = await _resultService.GetMyResultsAsync(token);

            return View(results);
        }
    }
}
