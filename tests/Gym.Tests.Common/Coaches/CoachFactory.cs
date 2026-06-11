using Gym.Domain.Coaches;
using Gym.Domain.Common.Result;

namespace Gym.Tests.Common.Coaches;

public static class CoachFactory
{
    public static Result<Coach> CreateCoach(
        string? firstName = "Ahmed",
        string? lastName = "Ali",
        DateTime? dateOfBirth = null,
        string? phoneNumber = "01000000000",
        string? imageUrl = "/images/coach.jpg",
        DateTime? hireDate = null)
    {
        return Coach.Create(
            firstName ?? "Ahmed",
            lastName ?? "Ali",
            dateOfBirth ?? DateTime.UtcNow.AddYears(-30),
            phoneNumber ?? "01000000000",
            imageUrl ?? "/images/coach.jpg",
            hireDate ?? DateTime.UtcNow.AddDays(-10));
    }
}
