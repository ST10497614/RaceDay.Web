using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Data;
using RaceDay.API.Controllers;
using Xunit;

namespace RaceDay.API.Tests;

public class EventsControllerTests
{
    private RaceDayDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RaceDayDbContext(options);
    }

    [Fact]
    public async Task GetEvents_ReturnsOk()
    {
        using var db = CreateContext();
        var controller = new EventsController(db);

        var result = await controller.GetEvents();

        Assert.IsType<OkObjectResult>(result);
    }
}