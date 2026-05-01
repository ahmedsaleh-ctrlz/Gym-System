
using Gym.Domain.Common;
using Gym.Domain.Common.Result;
using Gym.Domain.People;

namespace Gym.Domain.Employees;
public sealed class Employee : AuditableEntity
{
    public DateTime HireDate { get; private set; }
    public bool IsActive { get; private set; } = true;

    public Person Person { get; private set; } = null!;

    private Employee()
    {
    }

    private Employee(DateTime hireDate, Person person)
    {
        HireDate = hireDate;
        Person = person;
    }

    public static Result<Employee> Create(
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        string phoneNumber,
        string imageUrl,
        DateTime hireDate,int userId)
    {
        var error = Validate(hireDate);
        if (error is not null)
        {
            return error;
        }

        var personResult = Person.Create(firstName, lastName, dateOfBirth, phoneNumber, imageUrl,userId);
        if (personResult.IsError)
        {
            return personResult.TopError;
        }

        return new Employee(hireDate,personResult.Value);
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
            return EmployeeError.CannotUpdateInactiveEmployee;
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
            return EmployeeError.CannotUpdateInactiveEmployee;
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
            return EmployeeError.EmployeeAlreadyActive;
        }

        IsActive = true;
        return Result.Updated;
    }

    public Result<Updated> Deactivate()
    {
        if (!IsActive)
        {
            return EmployeeError.EmployeeAlreadyInactive;
        }

        IsActive = false;
        return Result.Updated;
    }

    private static Error? Validate(DateTime hireDate)
    {
        if (hireDate > DateTime.UtcNow)
        {
            return EmployeeError.InvalidHireDate;
        }


        return null;
    }
}
