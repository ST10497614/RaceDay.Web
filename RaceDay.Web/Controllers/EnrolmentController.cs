
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class EnrolmentController : Controller
    {
        private readonly CategoryEnrolmentApiService _service;

        public EnrolmentController(
            CategoryEnrolmentApiService service)
        {
            _service = service;
        }

        private bool IsParticipant()
        {
            return HttpContext.Session.GetString("Role") == "Participant"
                && !string.IsNullOrWhiteSpace(
                    HttpContext.Session.GetString("Token"));
        }

        public async Task<IActionResult> MyEnrolments()
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Account");

            var token = HttpContext.Session.GetString("Token")!;

            var enrolments = await _service.GetMyEnrolmentsAsync(token);

            return View(enrolments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(int categoryId)
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Account");

            if (categoryId <= 0)
            {
                TempData["Error"] = "Please select a valid category.";
                return RedirectToAction("Events", "Participant");
            }

            var token = HttpContext.Session.GetString("Token")!;

            var success = await _service.EnrolAsync(
                categoryId, token);

            if (success)
            {
                TempData["Success"] =
                    "Your enrolment was submitted successfully.";
            }
            else
            {
                TempData["Error"] =
                    "Enrolment could not be completed. You may already be enrolled, the category may be full, or the API rejected the request.";
            }

            return RedirectToAction(nameof(MyEnrolments));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int enrolmentId)
        {
            if (!IsParticipant())
                return RedirectToAction("Login", "Account");

            var token = HttpContext.Session.GetString("Token")!;

            var success = await _service.CancelEnrolmentAsync(
                enrolmentId, token);

            TempData[success ? "Success" : "Error"] = success
                ? "Enrolment cancelled."
                : "The API could not cancel this enrolment.";

            return RedirectToAction(nameof(MyEnrolments));
        }
    }
}
