
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.API.Data;
using RaceDay.API.Models;
using RaceDay.API.Services;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private const long MaxBannerBytes = 5 * 1024 * 1024;

    private readonly RaceDayDbContext _context;
    private readonly IEventBannerStorageService _bannerStorage;

    [ActivatorUtilitiesConstructor]
    public EventsController(
        RaceDayDbContext context,
        IEventBannerStorageService bannerStorage)
    {
        _context = context;
        _bannerStorage = bannerStorage;
    }

    // Retained for existing unit tests.
    public EventsController(RaceDayDbContext context)
        : this(context, new UnavailableBannerStorageService())
    {
    }

    private sealed class UnavailableBannerStorageService
        : IEventBannerStorageService
    {
        public Task UploadAsync(
            int eventId,
            Stream content,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                "Blob storage is not configured.");
        }

        public Task<(Stream Content, string ContentType)?> DownloadAsync(
            int eventId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<(Stream Content, string ContentType)?>(null);
        }

        public Task<bool> ExistsAsync(
            int eventId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task DeleteAsync(
            int eventId,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    // GET: api/events
    // Public event listing.
    [HttpGet]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _context.Events
            .Select(e => new
            {
                eventId = e.EventId,
                name = e.Title,
                date = e.EventDate,
                location = e.Location,
                description = e.Description,
                organiserId = e.OrganiserId,
                bannerImageUrl = $"/api/events/{e.EventId}/banner"
            })
            .ToListAsync();

        return Ok(events);
    }

    // GET: api/events/5
    // Public event details.
    [HttpGet("{eventId:int}")]
    public async Task<IActionResult> GetEvent(int eventId)
    {
        var eventItem = await _context.Events
            .Where(e => e.EventId == eventId)
            .Select(e => new
            {
                eventId = e.EventId,
                name = e.Title,
                date = e.EventDate,
                location = e.Location,
                description = e.Description,
                organiserId = e.OrganiserId,
                bannerImageUrl = $"/api/events/{e.EventId}/banner"
            })
            .FirstOrDefaultAsync();

        if (eventItem == null)
            return NotFound("Event not found.");

        return Ok(eventItem);
    }

    // POST: api/events
    [Authorize(Roles = "Organiser")]
    [HttpPost]
    public async Task<IActionResult> CreateEvent(
        [FromBody] CreateEventRequest request)
    {
        var organiserId = GetOrganiserId();

        if (organiserId == null ||
            request.OrganiserId != organiserId.Value)
        {
            return Forbid();
        }

        var organiserExists = await _context.Organisers
            .AnyAsync(o => o.OrganiserID == organiserId.Value);

        if (!organiserExists)
            return BadRequest("Organiser not found.");

        var newEvent = new Event
        {
            OrganiserId = organiserId.Value,
            Title = request.Name,
            EventDate = request.Date,
            Location = request.Location,
            Description = request.Description
        };

        _context.Events.Add(newEvent);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetEvent),
            new { eventId = newEvent.EventId },
            new { eventId = newEvent.EventId });
    }

    // PUT: api/events/5
    [Authorize(Roles = "Organiser")]
    [HttpPut("{eventId:int}")]
    public async Task<IActionResult> UpdateEvent(
        int eventId,
        [FromBody] CreateEventRequest request)
    {
        var eventItem = await _context.Events.FindAsync(eventId);

        if (eventItem == null)
            return NotFound("Event not found.");

        if (!OwnsEvent(eventItem))
            return Forbid();

        eventItem.Title = request.Name;
        eventItem.EventDate = request.Date;
        eventItem.Location = request.Location;
        eventItem.Description = request.Description;

        await _context.SaveChangesAsync();

        return Ok(new { eventId = eventItem.EventId });
    }

    // DELETE: api/events/5
    [Authorize(Roles = "Organiser")]
    [HttpDelete("{eventId:int}")]
    public async Task<IActionResult> DeleteEvent(int eventId)
    {
        var eventItem = await _context.Events.FindAsync(eventId);

        if (eventItem == null)
            return NotFound("Event not found.");

        if (!OwnsEvent(eventItem))
            return Forbid();

        // Attempt banner deletion, but do not let unavailable
        // storage prevent deletion of the database event.
        try
        {
            await _bannerStorage.DeleteAsync(eventId);
        }
        catch (Exception)
        {
            // Storage is unavailable. Continue with event deletion.
        }

        _context.Events.Remove(eventItem);

        await _context.SaveChangesAsync();

        return Ok("Event deleted successfully.");
    }

    // POST: api/events/5/banner
    [Authorize(Roles = "Organiser")]
    [HttpPost("{eventId:int}/banner")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxBannerBytes + 64 * 1024)]
    public async Task<IActionResult> UploadBanner(
        int eventId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Please select an image.");

        if (file.Length > MaxBannerBytes)
            return BadRequest("The image must not exceed 5 MB.");

        var eventItem = await _context.Events.FindAsync(
            new object[] { eventId },
            cancellationToken);

        if (eventItem == null)
            return NotFound("Event not found.");

        if (!OwnsEvent(eventItem))
            return Forbid();

        var extension = Path.GetExtension(file.FileName)
            .ToLowerInvariant();

        var allowed = new Dictionary<string, string>
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };

        if (!allowed.TryGetValue(extension, out var expectedType) ||
            !string.Equals(
                file.ContentType,
                expectedType,
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(
                "Only valid JPG, PNG and WEBP image files are allowed.");
        }

        await using var stream = file.OpenReadStream();

        var signature = new byte[12];

        var bytesRead = await stream.ReadAsync(
            signature.AsMemory(0, signature.Length),
            cancellationToken);

        if (!HasValidImageSignature(
            extension,
            signature,
            bytesRead))
        {
            return BadRequest(
                "The uploaded file content is not a valid image of the selected type.");
        }

        stream.Position = 0;

        try
        {
            await _bannerStorage.UploadAsync(
                eventId,
                stream,
                expectedType,
                cancellationToken);

            return Ok(new
            {
                message = "Banner uploaded successfully."
            });
        }
        catch (Exception)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                "Banner storage is unavailable. Please start Azurite and try again.");
        }
    }

    // HEAD: api/events/5/banner
    [HttpHead("{eventId:int}/banner")]
    public async Task<IActionResult> HeadBanner(
        int eventId,
        CancellationToken cancellationToken)
    {
        var eventExists = await _context.Events
            .AnyAsync(e => e.EventId == eventId, cancellationToken);

        if (!eventExists)
            return NotFound();

        try
        {
            var exists = await _bannerStorage.ExistsAsync(
                eventId,
                cancellationToken);

            return exists ? NoContent() : NotFound();
        }
        catch (Exception)
        {
            // Banner storage is unavailable.
            // The event itself can still be displayed.
            return NotFound();
        }
    }

    // GET: api/events/5/banner
    [HttpGet("{eventId:int}/banner")]
    public async Task<IActionResult> GetBanner(
        int eventId,
        CancellationToken cancellationToken)
    {
        var eventExists = await _context.Events
            .AnyAsync(e => e.EventId == eventId, cancellationToken);

        if (!eventExists)
            return NotFound("Event not found.");

        try
        {
            var banner = await _bannerStorage.DownloadAsync(
                eventId,
                cancellationToken);

            if (banner == null)
                return NotFound();

            return File(
                banner.Value.Content,
                banner.Value.ContentType);
        }
        catch (Exception)
        {
            return NotFound(
                "Event banner is currently unavailable.");
        }
    }

    private static bool HasValidImageSignature(
        string extension,
        byte[] bytes,
        int length)
    {
        if (extension is ".jpg" or ".jpeg")
        {
            return length >= 3 &&
                   bytes[0] == 0xFF &&
                   bytes[1] == 0xD8 &&
                   bytes[2] == 0xFF;
        }

        if (extension == ".png")
        {
            return length >= 8 &&
                   bytes.Take(8).SequenceEqual(
                       new byte[]
                       {
                           0x89, 0x50, 0x4E, 0x47,
                           0x0D, 0x0A, 0x1A, 0x0A
                       });
        }

        if (extension == ".webp")
        {
            return length >= 12 &&
                   System.Text.Encoding.ASCII.GetString(
                       bytes, 0, 4) == "RIFF" &&
                   System.Text.Encoding.ASCII.GetString(
                       bytes, 8, 4) == "WEBP";
        }

        return false;
    }

    private int? GetOrganiserId()
    {
        var value = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.TryParse(value, out var id)
            ? id
            : null;
    }

    private bool OwnsEvent(Event eventItem)
    {
        return GetOrganiserId() == eventItem.OrganiserId;
    }
}
