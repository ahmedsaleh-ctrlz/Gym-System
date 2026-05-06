
using Gym.Domain.Common;
using Gym.Domain.Common.Result;
using Gym.Domain.People;

namespace Gym.Domain.Coaches;
public sealed class Coach : AuditableEntity
{
    public DateTime HireDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int PersonId { get; private set; }
    public Person Person { get; private set; } = null!;

    private Coach()
    {
    }

    private Coach(DateTime hireDate, Person person)
    {
        HireDate = hireDate;
        Person = person;
    }

    public static Result<Coach> Create(
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        string phoneNumber,
        string imageUrl,
        DateTime hireDate)
    {
        var error = Validate(hireDate);
        if (error is not null)
        {
            return error;
        }

        var personResult = Person.Create(firstName, lastName, dateOfBirth, phoneNumber, imageUrl);
        if (personResult.IsError)
        {
            return personResult.TopError;
        }

        return new Coach(hireDate,personResult.Value);
    }

    public Result<Updated> UpdateInfo(
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        string phoneNumber,
        DateTime hireDate)
    {
        if (!IsActive)
        {
            return CoachError.CannotUpdateInactiveCoach;
        }

        var error = Validate(hireDate);
        if (error is not null)
        {
            return error;
        }

        var personUpdateResult = Person.UpdateInfo(firstName, lastName, dateOfBirth, phoneNumber);
        if (personUpdateResult.IsError)
        {
            return personUpdateResult.TopError;
        }

        HireDate = hireDate;
        

        return Result.Updated;
    }

    public Result<Updated> UpdateImage(string imageUrl)
    {
        if (!IsActive)
        {
            return CoachError.CannotUpdateInactiveCoach;
        }

        var imageUpdateResult = Person.UpdateImage(imageUrl);
        if (imageUpdateResult.IsError)
        {
            return imageUpdateResult.TopError;
        }

        return Result.Updated;
    }

    public Result<Updated> Activate()
    {
        if (IsActive)
        {
            return CoachError.CoachAlreadyActive;
        }

        IsActive = true;
        return Result.Updated;
    }

    public Result<Updated> Deactivate()
    {
        if (!IsActive)
        {
            return CoachError.CoachAlreadyInactive;
        }

        IsActive = false;
        return Result.Updated;
    }

    private static Error? Validate(DateTime hireDate)
    {
        if (hireDate > DateTime.UtcNow)
        {
            return CoachError.InvalidHireDate;
        }


        return null;
    }
}
