using Gym.Domain.Common.Result;

namespace Gym.Domain.Classes.ClassBookings;

public static class ClassBookingError
{
    public static Error ClassRequired => Error.Validation("Class_Booking_Class_Required", "ClassBookingClassRequired");
    public static Error MemberRequired => Error.Validation("Class_Booking_Member_Required", "ClassBookingMemberRequired");
    public static Error MemberDeleted => Error.Conflict("Class_Booking_Member_Deleted", "ClassBookingMemberDeleted");
    public static Error CoachInactive => Error.Conflict("Class_Booking_Coach_Inactive", "ClassBookingCoachInactive");
    public static Error InvalidStatus => Error.Validation("Class_Booking_Status_Invalid", "ClassBookingStatusInvalid");
    public static Error InvalidBookedAt => Error.Validation("Class_Booking_Booked_At_Invalid", "ClassBookingBookedAtInvalid");
}
