using Gym.Domain.Members;
using Gym.Tests.Common.Members;

namespace Gym.Domain.UnitTests.Members;

public class MemberTests
{
    [Fact]
    public void Create_ShouldReturnError_WhenJoinDateIsInFuture()
    {
        var result = MemberFactory.CreateMember(joinDate: DateTime.UtcNow.AddDays(1));

        Assert.False(result.IsSuccess);
        Assert.Equal(MemberError.JoinDataInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenPersonDataIsInvalid()
    {
        var result = MemberFactory.CreateMember(firstName: string.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(Gym.Domain.People.PersonError.FirstNameRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_WhenDataIsValid()
    {
        var result = MemberFactory.CreateMember();

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsDeleted);
        Assert.NotNull(result.Value.Person);
    }

    [Fact]
    public void UpdateInfo_ShouldReturnError_WhenJoinDateIsInFuture()
    {
        var member = MemberFactory.CreateMember().Value;

        var result = member.UpdateInfo("A", "B", DateTime.UtcNow.AddYears(-22), "010", DateTime.UtcNow.AddDays(1), "Updated");

        Assert.False(result.IsSuccess);
        Assert.Equal(MemberError.JoinDataInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void Delete_ShouldMarkMemberAsDeleted()
    {
        var member = MemberFactory.CreateMember().Value;

        var result = member.Delete();

        Assert.True(result.IsSuccess);
        Assert.True(member.IsDeleted);
        Assert.NotNull(member.DeletedAt);
    }

    [Fact]
    public void Delete_ShouldReturnError_WhenMemberAlreadyDeleted()
    {
        var member = MemberFactory.CreateMember().Value;
        member.Delete();

        var result = member.Delete();

        Assert.False(result.IsSuccess);
        Assert.Equal(MemberError.MemberAlreadyDeleted.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateInfo_ShouldReturnError_WhenMemberIsDeleted()
    {
        var member = MemberFactory.CreateMember().Value;
        member.Delete();

        var result = member.UpdateInfo("A", "B", DateTime.UtcNow.AddYears(-22), "010", DateTime.UtcNow.AddDays(-1), "Updated");

        Assert.False(result.IsSuccess);
        Assert.Equal(MemberError.CannotUpdateDeletedMember.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateInfo_ShouldReturnError_WhenPersonDataIsInvalid()
    {
        var member = MemberFactory.CreateMember().Value;

        var result = member.UpdateInfo(string.Empty, "B", DateTime.UtcNow.AddYears(-22), "010", DateTime.UtcNow.AddDays(-1), "Updated");

        Assert.False(result.IsSuccess);
        Assert.Equal(Gym.Domain.People.PersonError.FirstNameRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateInfo_ShouldReturnSuccess_WhenDataIsValid()
    {
        var member = MemberFactory.CreateMember().Value;

        var result = member.UpdateInfo("Mona", "Sameh", DateTime.UtcNow.AddYears(-23), "01111111111", DateTime.UtcNow.AddDays(-2), "Updated");

        Assert.True(result.IsSuccess);
        Assert.Equal("Mona", member.Person.FirstName);
        Assert.Equal("Sameh", member.Person.LastName);
        Assert.Equal("01111111111", member.Person.PhoneNumber);
        Assert.Equal("Updated", member.Notes);
    }

    [Fact]
    public void UpdateImage_ShouldReturnError_WhenMemberIsDeleted()
    {
        var member = MemberFactory.CreateMember().Value;
        member.Delete();

        var result = member.UpdateImage("/images/new-member.jpg");

        Assert.False(result.IsSuccess);
        Assert.Equal(MemberError.CannotUpdateDeletedMember.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateImage_ShouldReturnError_WhenImageUrlIsInvalid()
    {
        var member = MemberFactory.CreateMember().Value;

        var result = member.UpdateImage(string.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(Gym.Domain.People.PersonImages.PersonImageError.PersonImageUrlRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateImage_ShouldReturnSuccess_WhenImageUrlIsValid()
    {
        var member = MemberFactory.CreateMember().Value;

        var result = member.UpdateImage("/images/new-member.jpg");

        Assert.True(result.IsSuccess);
        Assert.Equal("/images/new-member.jpg", member.Person.Image.ImageUrl);
    }
}