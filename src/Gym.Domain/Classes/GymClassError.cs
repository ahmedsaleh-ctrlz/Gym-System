using Gym.Domain.Common.Result;

namespace Gym.Domain.Classes;

public static class GymClassError
{
    public static Error CoachRequired => Error.Validation("Class_Coach_Required", "ClassCoachRequired");
    public static Error CoachInactive => Error.Conflict("Class_Coach_Inactive", "ClassCoachInactive");
    public static Error TitleRequired => Error.Validation("Class_Title_Required", "ClassTitleRequired");
    public static Error InvalidDuration => Error.Validation("Class_Duration_Invalid", "ClassDurationInvalid");
}
