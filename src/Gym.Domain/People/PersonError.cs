using Gym.Domain.Common.Result;

namespace Gym.Domain.People;
public class PersonError
{
    public static Error FirstNameRequired => Error.Validation("First_Name_Required" , "FirstNameRequired");
    public static Error LastNameRequired => Error.Validation("Last_Name_Required", "LastNameRequired");
    public static Error InvalidDateOfBirth => Error.Validation("Invalid_Date_Of_Birth", "InvalidDateOfBirth");
    public static Error PhoneNumberRequired => Error.Validation("Phone_Number_Required", "PhoneNumberRequired");
    public static Error ImageRequired => Error.Validation("Image_Required", "ImageRequired");
    public static Error UserIdRequired => Error.Validation("User_Id_Required", "UserIdRequired");
      
}
