using Gym.Domain.People;
using Gym.Tests.Common.People;

namespace Gym.Domain.UnitTests.People;

public class PersonTests
{
    [Fact]
    public void Create_ShouldReturnError_WhenFirstNameIsMissing()
    {
        var result = PersonFactory.CreatePerson(firstName: string.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(PersonError.FirstNameRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenLastNameIsMissing()
    {
        var result = PersonFactory.CreatePerson(lastName: string.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(PersonError.LastNameRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenDateOfBirthIsInFuture()
    {
        var result = PersonFactory.CreatePerson(dateOfBirth: DateTime.UtcNow.AddDays(1));

        Assert.False(result.IsSuccess);
        Assert.Equal(PersonError.InvalidDateOfBirth.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenPhoneNumberIsMissing()
    {
        var result = PersonFactory.CreatePerson(phoneNumber: string.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(PersonError.PhoneNumberRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenImageIsInvalid()
    {
        var result = PersonFactory.CreatePerson(imageUrl: string.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(Gym.Domain.People.PersonImages.PersonImageError.PersonImageUrlRequired.Code, result.TopError.Code);
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
    public void UpdateInfo_ShouldReturnError_WhenFirstNameIsMissing()
    {
        var person = PersonFactory.CreatePerson().Value;

        var result = person.UpdateInfo(string.Empty, "Hassan", DateTime.UtcNow.AddYears(-20), "01000000000");

        Assert.False(result.IsSuccess);
        Assert.Equal(PersonError.FirstNameRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateInfo_ShouldReturnError_WhenLastNameIsMissing()
    {
        var person = PersonFactory.CreatePerson().Value;

        var result = person.UpdateInfo("Ali", string.Empty, DateTime.UtcNow.AddYears(-20), "01000000000");

        Assert.False(result.IsSuccess);
        Assert.Equal(PersonError.LastNameRequired.Code, result.TopError.Code);
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
    public void UpdateInfo_ShouldReturnError_WhenPhoneNumberIsMissing()
    {
        var person = PersonFactory.CreatePerson().Value;

        var result = person.UpdateInfo("Ali", "Hassan", DateTime.UtcNow.AddYears(-20), string.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(PersonError.PhoneNumberRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateInfo_ShouldReturnSuccess_WhenDataIsValid()
    {
        var person = PersonFactory.CreatePerson().Value;

        var result = person.UpdateInfo("Ali", "Hassan", DateTime.UtcNow.AddYears(-22), "01111111111");

        Assert.True(result.IsSuccess);
        Assert.Equal("Ali", person.FirstName);
        Assert.Equal("Hassan", person.LastName);
        Assert.Equal("01111111111", person.PhoneNumber);
    }

    [Fact]
    public void UpdateImage_ShouldReturnError_WhenImageUrlIsInvalid()
    {
        var person = PersonFactory.CreatePerson().Value;

        var result = person.UpdateImage(string.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(Gym.Domain.People.PersonImages.PersonImageError.PersonImageUrlRequired.Code, result.TopError.Code);
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