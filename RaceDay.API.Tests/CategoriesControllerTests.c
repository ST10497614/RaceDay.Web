using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Data;
using RaceDay.API.Controllers;
using Xunit;

namespace RaceDay.API.Tests;

public class CategoriesControllerTests
{
    private RaceDayDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RaceDayDbContext(options);
    }

    [Fact]
    public async Task GetCategories_ReturnsOk()
    {
        using var db = CreateContext();
        var controller = new CategoriesController(db);

        var result = await controller.GetCategories();

        Assert.IsType<OkObjectResult>(result);
    }
}