using Gym.Domain.People;
using Gym.Tests.Common.People;

namespace Gym.Domain.UnitTests.People;

public class PersonTests
{
    [Fact]
    public void Create_ShouldReturnError_WhenFirstNameIsMissing()
    {
        var result = PersonFactory.CreatePerson(firstName: "");

        Assert.False(result.IsSuccess);
        Assert.Equal(PersonError.FirstNameRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenDataIsValid()
    {
        var result = PersonFactory.CreatePerson();

        Assert.True(result.IsSuccess);
        Assert.Equal("Omar", result.Value.FirstName);
        Assert.Equal("/images/person.jpg", result.Value.Image.ImageUrl);
    }

    [Fact]
    public void UpdateInfo_ShouldReturnError_WhenDateOfBirthIsInFuture()
    {
        var person = PersonFactory.CreatePerson().Value;

        var result = person.UpdateInfo("Ali", "Hassan", DateTime.UtcNow.AddDays(1), "01000000000");

        Assert.False(result.IsSuccess);
        Assert.Equal(PersonError.InvalidDateOfBirth.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateImage_ShouldReturnSuccess_WhenImageUrlIsValid()
    {
        var person = PersonFactory.CreatePerson().Value;

        var result = person.UpdateImage("/images/person-updated.jpg");

        Assert.True(result.IsSuccess);
        Assert.Equal("/images/person-updated.jpg", person.Image.ImageUrl);
    }
}
