using Gym.Domain.Common.Result;
using Gym.Domain.People.PersonImages;

namespace Gym.Tests.Common.People.PersonImages;

public static class PersonImageFactory
{
    public static Result<PersonImage> CreatePersonImage(string? imageUrl = "/images/default.jpg")
    {
        return PersonImage.Create(imageUrl ?? "/images/default.jpg");
    }
}