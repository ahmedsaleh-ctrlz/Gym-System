using Gym.Domain.Classes;
using Gym.Domain.Classes.ClassBookings.Enums;
using Gym.Domain.Common;
using Gym.Domain.Common.Result;
using Gym.Domain.Members;

namespace Gym.Domain.Classes.ClassBookings;

public sealed class ClassBooking : AuditableEntity
{
    public GymClass Class { get; private set; } = null!;
    public Member Member { get; private set; } = null!;
    public ClassBookingStatus Status { get; private set; }
    public DateTime BookedAt { get; private set; }

    private ClassBooking()
    {
    }

    private ClassBooking(GymClass gymClass, Member member, ClassBookingStatus status, DateTime bookedAt)
    {
        Class = gymClass;
        Member = member;
        Status = status;
        BookedAt = bookedAt;
    }

    public static Result<ClassBooking> Create(GymClass gymClass, Member member, ClassBookingStatus status, DateTime bookedAt)
    {
        var error = Validate(gymClass, member, status, bookedAt);
        if (error is not null)
        {
            return error;
        }

        return new ClassBooking(gymClass, member, status, bookedAt);
    }

    public Result<Updated> UpdateStatus(ClassBookingStatus status)
    {
        if (status is ClassBookingStatus.Unknown)
        {
            return ClassBookingError.InvalidStatus;
        }

        Status = status;
        return Result.Updated;
    }

    private static Error? Validate(GymClass gymClass, Member member, ClassBookingStatus status, DateTime bookedAt)
    {
        if (gymClass is null)
        {
            return ClassBookingError.ClassRequired;
        }

        if (member is null)
        {
            return ClassBookingError.MemberRequired;
        }

        if (member.IsDeleted)
        {
            return ClassBookingError.MemberDeleted;
        }

        if (!gymClass.Coach.IsActive)
        {
            return ClassBookingError.CoachInactive;
        }

        if (status is ClassBookingStatus.Unknown)
        {
            return ClassBookingError.InvalidStatus;
        }

        if (bookedAt > DateTime.UtcNow)
        {
            return ClassBookingError.InvalidBookedAt;
        }

        return null;
    }
}
