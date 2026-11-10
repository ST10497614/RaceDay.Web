using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Controllers;
using RaceDay.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Xunit;

namespace RaceDay.API.Tests;

public class ResultsControllerTests
{
    private RaceDayDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RaceDayDbContext(options);
    }

    [Fact]
    public async Task GetEventResults_ReturnsNotFound_WhenEventDoesNotExist()
    {
        using var db = CreateContext();
        var controller = new ResultsController(db);

        var result = await controller.GetEventResults(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetMyResults_ReturnsUnauthorized_WhenUserHasNoValidId()
    {
        using var db = CreateContext();
        var controller = new ResultsController(db);

        var result = await controller.GetMyResults();

        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}