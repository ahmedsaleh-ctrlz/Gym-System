using Gym.Domain.People.PersonImages;
using Gym.Tests.Common.People.PersonImages;

namespace Gym.Domain.UnitTests.People;

public class PersonImageTests
{
    [Fact]
    public void Create_ShouldReturnError_WhenImageUrlIsEmpty()
    {
        var result = PersonImageFactory.CreatePersonImage(string.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(PersonImageError.PersonImageUrlRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenImageUrlIsValid()
    {
        var result = PersonImageFactory.CreatePersonImage("/images/person-image.jpg");

        Assert.True(result.IsSuccess);
        Assert.Equal("/images/person-image.jpg", result.Value.ImageUrl);
    }

    [Fact]
    public void Update_ShouldReturnError_WhenImageUrlIsEmpty()
    {
        var image = PersonImageFactory.CreatePersonImage().Value;

        var result = image.Update(string.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(PersonImageError.PersonImageUrlRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void Update_ShouldReturnSuccess_WhenImageUrlIsValid()
    {
        var image = PersonImageFactory.CreatePersonImage().Value;

        var result = image.Update("/images/updated.jpg");

        Assert.True(result.IsSuccess);
        Assert.Equal("/images/updated.jpg", image.ImageUrl);
    }
}