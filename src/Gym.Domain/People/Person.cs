using Gym.Domain.Common;
using Gym.Domain.Common.Result;
using Gym.Domain.People.PersonImages;

namespace Gym.Domain.People;

public class Person : AuditableEntity
{
    private Person() { }

    public string FirstName { get;private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateTime DateOfBirth { get; private set; }
    public string PhoneNumber { get; private set; } = string.Empty;
    public PersonImage Image { get; private set; } = null!;

    public static Result<Person> Create(string firstName, string lastName, DateTime dateOfBirth, string phoneNumber, PersonImage image)
    {
      
        if (string.IsNullOrWhiteSpace(firstName))
            return PersonError.FirstNameRequired;
        if(string.IsNullOrWhiteSpace(lastName))
            return PersonError.LastNameRequired;
        if(dateOfBirth == default)
            return PersonError.DateOfBirthRequired;
        if(string.IsNullOrWhiteSpace(phoneNumber))
            return PersonError.PhoneNumberRequired  ;
        if(image is null)
            return PersonError.ImageRequired;
            

        var person = new Person
        {
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            PhoneNumber = phoneNumber,
            Image = image
        };

        return person;
    }

    public Result<Updated> Update(string firstName, string lastName, DateTime dateOfBirth, string phoneNumber, PersonImage image)
    {
        if(string.IsNullOrWhiteSpace(firstName))
            return PersonError.FirstNameRequired;
        if(string.IsNullOrWhiteSpace(lastName))
            return PersonError.LastNameRequired;
        if(dateOfBirth == default)
            return PersonError.DateOfBirthRequired; 
        if(string.IsNullOrWhiteSpace(phoneNumber))
            return PersonError.PhoneNumberRequired;
        if(image is null)
            return PersonError.ImageRequired;   

        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        PhoneNumber = phoneNumber;
        Image = image;

        return Result.Updated;
    }

}
