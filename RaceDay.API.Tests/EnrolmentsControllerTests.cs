using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Data;
using RaceDay.API.Controllers;
using Xunit;

namespace RaceDay.API.Tests;

public class EnrolmentsControllerTests
{
    private RaceDayDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RaceDayDbContext(options);
    }

    [Fact]
    public async Task GetMyEnrolments_ReturnsUnauthorized_WhenUserHasNoValidId()
    {
        using var db = CreateContext();
        var controller = new EnrolmentsController(db);

        var result = await controller.GetMyEnrolments();

        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}