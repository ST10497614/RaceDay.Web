using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

[Route("event-banners")]
public class EventBannersController : Controller
{
    private readonly EventApiService _events;
    public EventBannersController(EventApiService events) => _events = events;

    [HttpGet("{eventId:int}")]
    public async Task<IActionResult> Get(int eventId)
    {
        var banner = await _events.GetBannerAsync(eventId);
        return banner is null ? NotFound() : File(banner.Value.Content, banner.Value.ContentType);
    }
}
