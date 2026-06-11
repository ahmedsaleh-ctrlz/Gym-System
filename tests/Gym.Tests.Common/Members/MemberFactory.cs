using Gym.Domain.Common.Result;
using Gym.Domain.Members;

namespace Gym.Tests.Common.Members;

public static class MemberFactory
{
    public static Result<Member> CreateMember(
        string? firstName = "Sara",
        string? lastName = "Hassan",
        DateTime? dateOfBirth = null,
        string? phoneNumber = "01000000001",
        string? imageUrl = "/images/member.jpg",
        DateTime? joinDate = null,
        string? notes = "Notes")
    {
        return Member.Create(
            firstName ?? "Sara",
            lastName ?? "Hassan",
            dateOfBirth ?? DateTime.UtcNow.AddYears(-25),
            phoneNumber ?? "01000000001",
            imageUrl ?? "/images/member.jpg",
            joinDate ?? DateTime.UtcNow.AddDays(-5),
            notes);
    }
}
