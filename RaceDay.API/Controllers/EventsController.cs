using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    public EventsController(RaceDayDbContext context, IEventBannerStorageService bannerStorage)
    {
        _context = context;
        _bannerStorage = bannerStorage;
    }

    // Keeps simple controller construction available to existing unit tests.
    public EventsController(RaceDayDbContext context) : this(context, new UnavailableBannerStorageService()) { }

    private sealed class UnavailableBannerStorageService : IEventBannerStorageService
    {
        public Task UploadAsync(int eventId, Stream content, string contentType, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Blob storage is not configured for this controller instance.");
        public Task<(Stream Content, string ContentType)?> DownloadAsync(int eventId, CancellationToken cancellationToken = default)
            => Task.FromResult<(Stream Content, string ContentType)?>(null);
        public Task<bool> ExistsAsync(int eventId, CancellationToken cancellationToken = default)
            => Task.FromResult(false);
        public Task DeleteAsync(int eventId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    [HttpGet]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _context.Events.Select(e => new
        {
            eventId = e.EventId,
            name = e.Title,
            date = e.EventDate,
            location = e.Location,
            description = e.Description,
            organiserId = e.OrganiserId,
            bannerImageUrl = $"/api/events/{e.EventId}/banner"
        }).ToListAsync();
        return Ok(events);
    }

    [HttpGet("{eventId:int}")]
    public async Task<IActionResult> GetEvent(int eventId)
    {
        var eventItem = await _context.Events.Where(e => e.EventId == eventId)
            .Select(e => new
            {
                eventId = e.EventId,
                name = e.Title,
                date = e.EventDate,
                location = e.Location,
                description = e.Description,
                organiserId = e.OrganiserId,
                bannerImageUrl = $"/api/events/{e.EventId}/banner"
            }).FirstOrDefaultAsync();
        return eventItem is null ? NotFound("Event not found.") : Ok(eventItem);
    }

    [Authorize(Roles = "Organiser")]
    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request)
    {
        var organiserId = GetOrganiserId();
        if (organiserId is null || request.OrganiserId != organiserId.Value) return Forbid();
        var organiserExists = await _context.Organisers.AnyAsync(o => o.OrganiserID == organiserId.Value);
        if (!organiserExists) return BadRequest("Organiser not found.");

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
        return CreatedAtAction(nameof(GetEvent), new { eventId = newEvent.EventId }, new { eventId = newEvent.EventId });
    }

    [Authorize(Roles = "Organiser")]
    [HttpPut("{eventId:int}")]
    public async Task<IActionResult> UpdateEvent(int eventId, [FromBody] CreateEventRequest request)
    {
        var eventItem = await _context.Events.FindAsync(eventId);
        if (eventItem is null) return NotFound("Event not found.");
        if (!OwnsEvent(eventItem)) return Forbid();
        eventItem.Title = request.Name;
        eventItem.EventDate = request.Date;
        eventItem.Location = request.Location;
        eventItem.Description = request.Description;
        await _context.SaveChangesAsync();
        return Ok(new { eventId = eventItem.EventId });
    }

    [Authorize(Roles = "Organiser")]
    [HttpDelete("{eventId:int}")]
    public async Task<IActionResult> DeleteEvent(int eventId)
    {
        var eventItem = await _context.Events.FindAsync(eventId);
        if (eventItem is null) return NotFound("Event not found.");
        if (!OwnsEvent(eventItem)) return Forbid();
        await _bannerStorage.DeleteAsync(eventId);
        _context.Events.Remove(eventItem);
        await _context.SaveChangesAsync();
        return Ok("Event deleted successfully.");
    }

    [Authorize(Roles = "Organiser")]
    [HttpPost("{eventId:int}/banner")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxBannerBytes + 64 * 1024)]
    public async Task<IActionResult> UploadBanner(int eventId, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0) return BadRequest("Please select an image.");
        if (file.Length > MaxBannerBytes) return BadRequest("The image must not exceed 5 MB.");

        var eventItem = await _context.Events.FindAsync(new object[] { eventId }, cancellationToken);
        if (eventItem is null) return NotFound("Event not found.");
        if (!OwnsEvent(eventItem)) return Forbid();

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = new Dictionary<string, string>
        {
            [".jpg"] = "image/jpeg", [".jpeg"] = "image/jpeg",
            [".png"] = "image/png", [".webp"] = "image/webp"
        };
        if (!allowed.TryGetValue(extension, out var expectedType) ||
            !string.Equals(file.ContentType, expectedType, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only valid JPG, PNG and WEBP image files are allowed.");

        await using var stream = file.OpenReadStream();
        var signature = new byte[12];
        var bytesRead = await stream.ReadAsync(signature.AsMemory(0, signature.Length), cancellationToken);
        if (!HasValidImageSignature(extension, signature, bytesRead))
            return BadRequest("The uploaded file content is not a valid image of the selected type.");
        stream.Position = 0;
        await _bannerStorage.UploadAsync(eventId, stream, expectedType, cancellationToken);
        return Ok(new { message = "Banner uploaded successfully." });
    }

    [HttpHead("{eventId:int}/banner")]
    public async Task<IActionResult> HeadBanner(int eventId, CancellationToken cancellationToken)
    {
        if (!await _context.Events.AnyAsync(e => e.EventId == eventId, cancellationToken)) return NotFound();
        return await _bannerStorage.ExistsAsync(eventId, cancellationToken) ? NoContent() : NotFound();
    }

    [HttpGet("{eventId:int}/banner")]
    public async Task<IActionResult> GetBanner(int eventId, CancellationToken cancellationToken)
    {
        if (!await _context.Events.AnyAsync(e => e.EventId == eventId, cancellationToken))
            return NotFound("Event not found.");
        var banner = await _bannerStorage.DownloadAsync(eventId, cancellationToken);
        return banner is null ? NotFound() : File(banner.Value.Content, banner.Value.ContentType);
    }

    private static bool HasValidImageSignature(string extension, byte[] bytes, int length)
    {
        if (extension is ".jpg" or ".jpeg")
            return length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;
        if (extension == ".png")
            return length >= 8 && bytes.Take(8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        if (extension == ".webp")
            return length >= 12 && System.Text.Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF"
                && System.Text.Encoding.ASCII.GetString(bytes, 8, 4) == "WEBP";
        return false;
    }

    private int? GetOrganiserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }

    private bool OwnsEvent(Event eventItem) => GetOrganiserId() == eventItem.OrganiserId;
}
