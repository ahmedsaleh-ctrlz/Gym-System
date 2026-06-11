using Gym.Domain.Common.Result;
using Gym.Domain.People;

namespace Gym.Tests.Common.People;

public static class PersonFactory
{
    public static Result<Person> CreatePerson(
        string? firstName = "Omar",
        string? lastName = "Khaled",
        DateTime? dateOfBirth = null,
        string? phoneNumber = "01000000002",
        string? imageUrl = "/images/person.jpg")
    {
        return Person.Create(
            firstName ?? "Omar",
            lastName ?? "Khaled",
            dateOfBirth ?? DateTime.UtcNow.AddYears(-20),
            phoneNumber ?? "01000000002",
            imageUrl ?? "/images/person.jpg");
    }
}
