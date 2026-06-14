using Gym.Application.Common.Errors;
using Gym.Application.Features.Coaches.Commands.CreateCoach;
using Gym.Application.Features.Coaches.Commands.DeleteCoach;
using Gym.Application.Features.Coaches.Commands.UpdateCoach;
using Gym.Application.Features.Coaches.Queries.GetCoachById;
using Gym.Application.Features.Coaches.Queries.GetCoaches;
using Gym.Application.SubcutaneousTests.Common;
using Microsoft.EntityFrameworkCore;

namespace Gym.Application.SubcutaneousTests.Features.Coaches;

public class CoachFeatureTests
{
    [Fact]
    public async Task CreateCoachCommand_ShouldCreateCoach()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();

        var result = await context.Mediator.Send(new CreateCoachCommand("Coach", "New", DateTime.UtcNow.AddYears(-28), "01020000000", "/images/coach-new.jpg", DateTime.UtcNow.AddDays(-3), "coach@gym.com", "123456"));

        Assert.True(result.IsSuccess);
        Assert.Single(context.DbContext.Coaches);
    }

    [Fact]
    public async Task UpdateCoachCommand_ShouldUpdateCoach()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var coach = await TestDataSeeder.AddCoachAsync(context);

        var result = await context.Mediator.Send(new UpdateCoachCommand(coach.Id, "Updated", "Coach", DateTime.UtcNow.AddYears(-29), "01122222222", DateTime.UtcNow.AddDays(-10)));

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated", context.DbContext.Coaches.First().Person.FirstName);
    }

    [Fact]
    public async Task UpdateCoachImageCommand_ShouldUpdateCoachImage()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var coach = await TestDataSeeder.AddCoachAsync(context);

        var result = await context.Mediator.Send(new UpdateCoachImageCommand(coach.Id, "/images/coach-updated.jpg"));

        Assert.True(result.IsSuccess);
        Assert.Equal("/images/coach-updated.jpg", context.DbContext.Coaches.First().Person.Image.ImageUrl);
    }

    [Fact]
    public async Task DeleteCoachCommand_ShouldDeactivateCoach()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var coach = await TestDataSeeder.AddCoachAsync(context);

        var result = await context.Mediator.Send(new DeleteCoachCommand(coach.Id));

        Assert.True(result.IsSuccess);
        Assert.False(context.DbContext.Coaches.IgnoreQueryFilters().First().IsActive);
    }

    [Fact]
    public async Task GetCoachesQuery_ShouldReturnFilteredCoaches()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        await TestDataSeeder.AddCoachAsync(context, "Alpha", "One");
        await TestDataSeeder.AddCoachAsync(context, "Beta", "Two");

        var result = await context.Mediator.Send(new GetCoachesQuery(SearchTerm: "beta"));

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Items!);
        Assert.Equal("Beta", result.Value.Items!.First().FirstName);
    }

    [Fact]
    public async Task GetCoachByIdQuery_ShouldReturnCoach()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var coach = await TestDataSeeder.AddCoachAsync(context, "Coach", "Lookup");

        var result = await context.Mediator.Send(new GetCoachByIdQuery(coach.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal("Lookup", result.Value.LastName);
    }
}
