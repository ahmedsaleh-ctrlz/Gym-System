using Gym.Domain.Common;
using Gym.Domain.Common.Result;
using Gym.Domain.People;
using Gym.Domain.People.PersonImages;

namespace Gym.Domain.Members;
public sealed class Member : AuditableEntity
{
    public DateTime JoinDate { get; private set; }
    public string? Notes { get; private set; }
    private bool IsDeleted { get; set; } = false;
    private DateTime DeletedAt { get; set; }
    public Person Person { get; private set; } = null!;


    private Member() { }

    private Member(DateTime joinDate, string? notes,Person personInfo)
    {
        Person = personInfo;
        JoinDate = joinDate;
        Notes = notes;
    }

    public static Result<Member> Create(string firstName,
        string lastName,
        DateTime dateOfBirth,
        string phoneNumber,
        string imageUrl,
        DateTime joinDate,
        string? notes)
    {

        var error = Validate(joinDate);
        if (error is not null) 
        {
            return error;
        }

        var personResult = Person.Create(firstName, lastName, dateOfBirth, phoneNumber, imageUrl);
        if (personResult.IsError)
            return personResult.TopError;
        
        return new Member(joinDate, notes, personResult.Value);
    }

    public Result<Updated> UpdateInfo(string firstName,
        string lastName,
        DateTime dateOfBirth,
        string phoneNumber,
        DateTime joinDate,
        string? notes)
    {
        if (IsDeleted)
            return MemberError.CannotUpdateDeletedMember;

        var error = Validate(joinDate);
        if (error is not null)
            return error;
        var personUpdateResult = Person.UpdateInfo(firstName, lastName, dateOfBirth, phoneNumber);
        if (personUpdateResult.IsError)
            return personUpdateResult.TopError;

        JoinDate = joinDate;
        Notes = notes;
        
        return Result.Updated;
    }

    public Result<Updated> UpdateImage(string imageUrl)
    {
        if (IsDeleted)
            return MemberError.CannotUpdateDeletedMember;

        var imageUpdateResult = Person.UpdateImage(imageUrl);
        if (imageUpdateResult.IsError)
            return imageUpdateResult.TopError;
        return Result.Updated;
    }

    public Result<Deleted> Delete()
    {
        if (IsDeleted)
            return MemberError.MemberAlreadyDeleted;
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        return Result.Deleted;
    }   

    private static Error? Validate(DateTime JoinDate)
    {
        if(JoinDate > DateTime.UtcNow)
        {
            return MemberError.JoinDataInvalid;
        }

        return null;
    }
}
