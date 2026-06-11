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
    public void Create_ShouldReturnSuccess_WhenDataIsValid()
    {
        var result = MemberFactory.CreateMember();

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.IsDeleted);
        Assert.NotNull(result.Value.Person);
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
    public void UpdateInfo_ShouldReturnError_WhenMemberIsDeleted()
    {
        var member = MemberFactory.CreateMember().Value;
        member.Delete();

        var result = member.UpdateInfo("A", "B", DateTime.UtcNow.AddYears(-22), "010", DateTime.UtcNow.AddDays(-1), "Updated");

        Assert.False(result.IsSuccess);
        Assert.Equal(MemberError.CannotUpdateDeletedMember.Code, result.TopError.Code);
    }
}
