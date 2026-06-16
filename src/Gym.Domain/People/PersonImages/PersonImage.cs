using Gym.Domain.Common;
using Gym.Domain.Common.Result;

namespace Gym.Domain.People.PersonImages;

public sealed class PersonImage : AuditableEntity
{
    public string ImageUrl { get; private set; } = string.Empty;

    private PersonImage() { }

    private PersonImage(string imageUrl)
    {
        ImageUrl = imageUrl;
    }

    public static Result<PersonImage> Create(string imageUrl)
    {
        var error = Validate(imageUrl);
        if (error is not null)
        {
            return error;
        }

        return new PersonImage(imageUrl);
    }

    public Result<Updated> Update(string imageUrl)
    {
        var error = Validate(imageUrl);
        if (error is not null)
        {
            return error;
        }

        ImageUrl = imageUrl;
        return Result.Updated;
    }

    private static Error? Validate(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return PersonImageError.PersonImageUrlRequired;
        }

        return null;
    }
}