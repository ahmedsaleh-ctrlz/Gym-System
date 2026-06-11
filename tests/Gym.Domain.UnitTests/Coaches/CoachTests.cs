using Gym.Domain.Coaches;
using Gym.Tests.Common.Coaches;

namespace Gym.Domain.UnitTests.Coaches;

public class CoachTests
{
    [Fact]
    public void Create_ShouldReturnError_WhenHireDateIsInFuture()
    {
        var result = CoachFactory.CreateCoach(hireDate: DateTime.UtcNow.AddDays(1));

        Assert.False(result.IsSuccess);
        Assert.Equal(CoachError.InvalidHireDate.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenPersonDataIsInvalid()
    {
        var result = CoachFactory.CreateCoach(firstName: "");

        Assert.False(result.IsSuccess);
        Assert.Equal(Gym.Domain.People.PersonError.FirstNameRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenDataIsValid()
    {
        var result = CoachFactory.CreateCoach();

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsActive);
        Assert.NotNull(result.Value.Person);
    }

    [Fact]
    public void UpdateInfo_ShouldReturnError_WhenCoachIsInactive()
    {
        var coach = CoachFactory.CreateCoach().Value;
        coach.Deactivate();

        var result = coach.UpdateInfo("A", "B", DateTime.UtcNow.AddYears(-20), "010", DateTime.UtcNow.AddDays(-3));

        Assert.False(result.IsSuccess);
        Assert.Equal(CoachError.CannotUpdateInactiveCoach.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateInfo_ShouldReturnError_WhenHireDateIsInFuture()
    {
        var coach = CoachFactory.CreateCoach().Value;

        var result = coach.UpdateInfo("A", "B", DateTime.UtcNow.AddYears(-20), "010", DateTime.UtcNow.AddDays(1));

        Assert.False(result.IsSuccess);
        Assert.Equal(CoachError.InvalidHireDate.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateInfo_ShouldReturnError_WhenPersonDataIsInvalid()
    {
        var coach = CoachFactory.CreateCoach().Value;

        var result = coach.UpdateInfo("", "B", DateTime.UtcNow.AddYears(-20), "010", DateTime.UtcNow.AddDays(-3));

        Assert.False(result.IsSuccess);
        Assert.Equal(Gym.Domain.People.PersonError.FirstNameRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateInfo_ShouldReturnSuccess_WhenDataIsValid()
    {
        var coach = CoachFactory.CreateCoach().Value;

        var result = coach.UpdateInfo("Youssef", "Adel", DateTime.UtcNow.AddYears(-28), "01111111111", DateTime.UtcNow.AddDays(-4));

        Assert.True(result.IsSuccess);
        Assert.Equal("Youssef", coach.Person.FirstName);
        Assert.Equal("Adel", coach.Person.LastName);
        Assert.Equal("01111111111", coach.Person.PhoneNumber);
    }

    [Fact]
    public void UpdateImage_ShouldReturnError_WhenCoachIsInactive()
    {
        var coach = CoachFactory.CreateCoach().Value;
        coach.Deactivate();

        var result = coach.UpdateImage("/images/inactive-coach.jpg");

        Assert.False(result.IsSuccess);
        Assert.Equal(CoachError.CannotUpdateInactiveCoach.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateImage_ShouldReturnError_WhenImageUrlIsInvalid()
    {
        var coach = CoachFactory.CreateCoach().Value;

        var result = coach.UpdateImage("");

        Assert.False(result.IsSuccess);
        Assert.Equal(Gym.Domain.People.PersonImages.PersonImageError.PersonImageUrlRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateImage_ShouldReturnSuccess_WhenImageUrlIsValid()
    {
        var coach = CoachFactory.CreateCoach().Value;

        var result = coach.UpdateImage("/images/coach-updated.jpg");

        Assert.True(result.IsSuccess);
        Assert.Equal("/images/coach-updated.jpg", coach.Person.Image.ImageUrl);
    }

    [Fact]
    public void Activate_ShouldReturnError_WhenCoachAlreadyActive()
    {
        var coach = CoachFactory.CreateCoach().Value;

        var result = coach.Activate();

        Assert.False(result.IsSuccess);
        Assert.Equal(CoachError.CoachAlreadyActive.Code, result.TopError.Code);
    }

    [Fact]
    public void Deactivate_ShouldReturnError_WhenCoachAlreadyInactive()
    {
        var coach = CoachFactory.CreateCoach().Value;
        coach.Deactivate();

        var result = coach.Deactivate();

        Assert.False(result.IsSuccess);
        Assert.Equal(CoachError.CoachAlreadyInactive.Code, result.TopError.Code);
    }

    [Fact]
    public void Deactivate_ThenActivate_ShouldUpdateCoachState()
    {
        var coach = CoachFactory.CreateCoach().Value;

        var deactivateResult = coach.Deactivate();
        var activateResult = coach.Activate();

        Assert.True(deactivateResult.IsSuccess);
        Assert.True(activateResult.IsSuccess);
        Assert.True(coach.IsActive);
    }
}
