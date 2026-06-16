using Gym.Application.Features.Members.Mappers;
using Gym.Tests.Common.Members;
using Gym.Tests.Common.Reflection;

namespace Gym.Application.UnitTests.Mappers;

public class MemberMapperTests
{
    [Fact]
    public void ToDto_ShouldThrowArgumentNullException_WhenMemberIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => MemberMapper.ToDto(null!));
    }

    [Fact]
    public void ToDto_ShouldMapMemberToResponse()
    {
        var member = MemberFactory.CreateMember(firstName: "Mona", lastName: "Nabil", notes: "VIP").Value;
        ReflectionTestHelper.SetProperty(member, "Id", 3);

        var result = member.ToDto();

        Assert.Equal(3, result.MemberId);
        Assert.Equal("Mona", result.FirstName);
        Assert.Equal("Nabil", result.LastName);
        Assert.Equal(member.Person.DateOfBirth, result.DateOfBirth);
        Assert.Equal(member.Person.PhoneNumber, result.PhoneNumber);
        Assert.Equal(member.Person.Image.ImageUrl, result.ImageUrl);
        Assert.Equal(member.JoinDate, result.JoinDate);
        Assert.Equal("VIP", result.Notes);
    }

    [Fact]
    public void ToActiveMemberDtos_ShouldThrowArgumentNullException_WhenMembersAreNull()
    {
        IEnumerable<Gym.Domain.Members.Member>? members = null;

        Assert.Throws<ArgumentNullException>(() => members!.ToActiveMemberDtos());
    }

    [Fact]
    public void ToActiveMemberDtos_ShouldMapMembersToResponses()
    {
        var firstMember = MemberFactory.CreateMember(firstName: "A", lastName: "One").Value;
        var secondMember = MemberFactory.CreateMember(firstName: "B", lastName: "Two").Value;
        ReflectionTestHelper.SetProperty(firstMember, "Id", 1);
        ReflectionTestHelper.SetProperty(secondMember, "Id", 2);

        var result = new[] { firstMember, secondMember }.ToActiveMemberDtos();

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].MemberId);
        Assert.Equal("A", result[0].FirstName);
        Assert.Equal("Two", result[1].LastName);
    }
}