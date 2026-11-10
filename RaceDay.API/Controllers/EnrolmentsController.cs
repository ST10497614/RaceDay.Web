
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/enrolments")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/enrolments
        [HttpGet]
        public async Task<IActionResult> GetEnrolments()
        {
            var enrolments = await _context.Enrolments
                .AsNoTracking()
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/enrolments/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEnrolment(int id)
        {
            var enrolment = await _context.Enrolments
                .FindAsync(id);

            if (enrolment == null)
            {
                return NotFound("Enrolment not found.");
            }

            return Ok(enrolment);
        }

        // POST: api/enrolments

        [HttpPost]
        public async Task<IActionResult> CreateEnrolment(
            [FromBody] CreateEnrolmentRequest request)
        {
            var participantIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(participantIdClaim, out int participantId))
                return Unauthorized("Participant ID is missing or invalid.");

            var participantExists = await _context.Participants
                .AnyAsync(p => p.ParticipantID == participantId);

            if (!participantExists)
                return Forbid();

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId);

            if (category == null)
                return BadRequest("Category does not exist.");

            var alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e =>
                    e.ParticipantId == participantId &&
                    e.CategoryId == request.CategoryId);

            Console.WriteLine($"JWT Participant ID: {participantId}");
            Console.WriteLine($"Requested Category ID: {request.CategoryId}");
            Console.WriteLine($"Already Enrolled: {alreadyEnrolled}");
            if (alreadyEnrolled)
                return Conflict("You are already enrolled in this category.");

            var currentEnrolments = await _context.Enrolments
                .CountAsync(e => e.CategoryId == request.CategoryId);

            if (currentEnrolments >= category.MaxParticipants)
                return Conflict("This category has reached its participant limit.");

            var enrolment = new Enrolment
            {
                ParticipantId = participantId,
                CategoryId = request.CategoryId,
                EnrolmentDate = DateTime.UtcNow,
                Status = "Active"
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Enrolment created successfully.",
                enrolmentId = enrolment.EnrolmentId
            });
        }


        public class CreateEnrolmentRequest
        {
            public int CategoryId { get; set; }
        }


        [HttpGet("my-enrolments")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var participantIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(participantIdClaim, out int participantId))
                return Unauthorized("Participant ID is missing or invalid.");

            var enrolments = await _context.Enrolments
                .AsNoTracking()
                .Where(e => e.ParticipantId == participantId)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.CategoryId,
                    e.EnrolmentDate,
                    e.Status
                })
                .ToListAsync();

            return Ok(enrolments);
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> CancelEnrolment(int id)
        {
            var participantIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(participantIdClaim, out int participantId))
                return Unauthorized("Participant ID is missing or invalid.");

            var enrolment = await _context.Enrolments
                .FirstOrDefaultAsync(e =>
                    e.EnrolmentId == id &&
                    e.ParticipantId == participantId);

            if (enrolment == null)
                return NotFound("Enrolment not found.");

            _context.Enrolments.Remove(enrolment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Enrolment cancelled successfully." });
        }


    }
}
