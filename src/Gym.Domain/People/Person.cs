using Gym.Domain.Common;
using Gym.Domain.Common.Result;
using Gym.Domain.People.PersonImages;

namespace Gym.Domain.People;

public class Person : AuditableEntity
{
    private Person() 
    { }

    private Person(string firstName, string lastName, DateTime dateOfBirth, string phoneNumber, PersonImage image)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        PhoneNumber = phoneNumber;
        Image = image;
        
    }

    public string FirstName { get;private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateTime DateOfBirth { get; private set; }
    public string PhoneNumber { get; private set; } = string.Empty;
    public PersonImage Image { get; private set; } = null!;


    public static Result<Person> Create(string firstName, string lastName, DateTime dateOfBirth, string phoneNumber, string imageUrl)
    {

        var error = Validate(firstName, lastName, dateOfBirth, phoneNumber);
        if (error is not null)
            return error;
        var image = PersonImage.Create(imageUrl);
        if (image.IsError)
            return image.TopError;

        return new Person(firstName, lastName, dateOfBirth, phoneNumber, image.Value);
    }
    public Result<Updated> UpdateInfo(string firstName, string lastName, DateTime dateOfBirth, string phoneNumber)
    {
        var error = Validate(firstName, lastName, dateOfBirth, phoneNumber);
        if (error is not null) 
            return error;

        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        PhoneNumber = phoneNumber;
        
        return Result.Updated;
    }

    public Result<Updated> UpdateImage(string imageUrl)
    {
        var imageResult = Image.Update(imageUrl);
        if (imageResult.IsError)
            return imageResult.TopError;
        return Result.Updated;
    }

    private static Error? Validate(string firstName, string lastName, DateTime dateOfBirth, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return PersonError.FirstNameRequired;
        if (string.IsNullOrWhiteSpace(lastName))
            return PersonError.LastNameRequired;
        if (dateOfBirth > DateTime.UtcNow)
            return PersonError.InvalidDateOfBirth;
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return PersonError.PhoneNumberRequired;
        

        return null;
    }

}   
