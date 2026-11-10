
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using System.Security.Claims;
using System;

namespace RaceDay.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/events/{eventId}/results
        [HttpGet("events/{eventId:int}/results")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            var eventExists = await _context.Events
                .AnyAsync(e => e.EventId == eventId);

            if (!eventExists)
                return NotFound("Event not found.");

            var results = await _context.Results
                .AsNoTracking()
                .Where(r => r.Enrolment.Category.EventId == eventId)
                .OrderBy(r => r.Position)
                .Select(r => new
                {
                    r.ResultId,
                    r.EnrolmentId,
                    r.Position,
                    r.FinishTime,
                    ParticipantId = r.Enrolment.ParticipantId,
                    ParticipantName = r.Enrolment.Participant.ParticipantName,
                    ParticipantSurname = r.Enrolment.Participant.ParticipantSurname,
                    CategoryId = r.Enrolment.CategoryId
                })
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/users/me/results
        [HttpGet("users/me/results")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            var participantIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(participantIdClaim, out int participantId))
                return Unauthorized("Participant ID is missing or invalid.");

            var results = await _context.Results
                .AsNoTracking()
                .Where(r => r.Enrolment.ParticipantId == participantId)
                .OrderBy(r => r.Position)
                .Select(r => new
                {
                    r.ResultId,
                    r.EnrolmentId,
                    r.Position,
                    r.FinishTime,
                    CategoryId = r.Enrolment.CategoryId,
                    CategoryName = r.Enrolment.Category.CategoryName
                })
                .ToListAsync();

            return Ok(results);
        }

        // POST: api/events/{eventId}/results
        [HttpPost("events/{eventId:int}/results")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateResult(
            int eventId,
            [FromBody] CreateResultRequest request)
        {
            var organiserIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(organiserIdClaim, out int organiserId))
                return Unauthorized("Organiser ID is missing or invalid.");

            var raceEvent = await _context.Events
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (raceEvent == null)
                return NotFound("Event not found.");

            if (raceEvent.OrganiserId != organiserId)
                return Forbid();

            var enrolment = await _context.Enrolments
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e =>
                    e.EnrolmentId == request.EnrolmentId &&
                    e.Category.EventId == eventId);

            if (enrolment == null)
                return BadRequest(
                    "The enrolment does not belong to this event.");

            var resultExists = await _context.Results
                .AnyAsync(r => r.EnrolmentId == request.EnrolmentId);

            if (resultExists)
                return Conflict(
                    "A result already exists for this enrolment.");

            if (request.Position <= 0)
                return BadRequest("Position must be greater than zero.");

            var result = new Result
            {
                EnrolmentId = request.EnrolmentId,
                Position = request.Position,
                FinishTime = request.FinishTime
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return Created(
                $"/api/events/{eventId}/results",
                new
                {
                    result.ResultId,
                    result.EnrolmentId,
                    result.Position,
                    result.FinishTime
                });
        }
    }

    public class CreateResultRequest
    {
        public int EnrolmentId { get; set; }

        public int Position { get; set; }

        public TimeOnly? FinishTime { get; set; }
    }
}
