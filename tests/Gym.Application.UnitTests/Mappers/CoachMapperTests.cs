using Gym.Application.Features.Coaches.Mappers;
using Gym.Tests.Common.Coaches;
using Gym.Tests.Common.Reflection;

namespace Gym.Application.UnitTests.Mappers;

public class CoachMapperTests
{
    [Fact]
    public void ToDto_ShouldThrowArgumentNullException_WhenCoachIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => CoachMapper.ToDto(null!));
    }

    [Fact]
    public void ToDto_ShouldMapCoachToResponse()
    {
        var coach = CoachFactory.CreateCoach(firstName: "Omar", lastName: "Samir").Value;
        ReflectionTestHelper.SetProperty(coach, "Id", 9);

        var result = coach.ToDto();

        Assert.Equal(9, result.CoachId);
        Assert.Equal("Omar", result.FirstName);
        Assert.Equal("Samir", result.LastName);
        Assert.Equal(coach.Person.DateOfBirth, result.DateOfBirth);
        Assert.Equal(coach.Person.PhoneNumber, result.PhoneNumber);
        Assert.Equal(coach.Person.Image.ImageUrl, result.ImageUrl);
        Assert.Equal(coach.HireDate, result.HireDate);
    }
}
