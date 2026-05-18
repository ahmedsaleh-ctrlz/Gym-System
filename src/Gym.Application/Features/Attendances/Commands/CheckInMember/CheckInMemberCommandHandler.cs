using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Attendances.Dtos;
using Gym.Domain.Attendance;
using Gym.Domain.Common.Result;
using Gym.Domain.Subscriptions.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Attendances.Commands.CheckInMember;

public sealed class CheckInMemberCommandHandler(
    IAppDbContext context,
    ILogger<CheckInMemberCommandHandler> logger,
    HybridCache cache) : IRequestHandler<CheckInMemberCommand, Result<AttendanceResponse>>
{
    public async Task<Result<AttendanceResponse>> Handle(CheckInMemberCommand command, CancellationToken ct)
    {
        logger.LogTrace("Handling {CommandName} for Member ID {MemberId}.", nameof(CheckInMemberCommand), command.MemberId);

        var member = await context.Members
            .Include(m => m.Person)
            .FirstOrDefaultAsync(m => m.Id == command.MemberId, ct);

        if (member is null)
        {
            logger.LogWarning("Member with ID {MemberId} not found for attendance check-in.", command.MemberId);
            return ApplicationErrors.MemberNotFound;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if(await context.Attendances.AnyAsync(a=> a.MemberId == command.MemberId && DateOnly.FromDateTime(a.CheckInAtUtc) == today, ct))
        {
            logger.LogWarning("Member with ID {MemberId} attempted multiple check-ins on {Date}.", command.MemberId, today);
            return ApplicationErrors.InvalidCheckInTime;
        }

        var hasActiveSubscription = await context.Subscriptions
            .AsNoTracking()
            .AnyAsync(s =>
                s.MemberId == command.MemberId &&
                s.Status == SubscriptionStatus.Active &&
                s.StartDate <= today &&
                s.EndDate >= today,
                ct);

        if (!hasActiveSubscription)
        {
            logger.LogWarning("Member with ID {MemberId} attempted check-in without an active subscription.", command.MemberId);
            return ApplicationErrors.MemberCannotCheckInWithoutActiveSubscription;
        }

        var attendanceResult = Attendance.Create(command.MemberId, DateTime.UtcNow);
        if (attendanceResult.IsError)
        {
            logger.LogWarning("Failed to create attendance for Member ID {MemberId}. Errors: {Errors}", command.MemberId, attendanceResult.Errors);
            return attendanceResult.Errors;
        }

        var attendance = attendanceResult.Value;

        await context.Attendances.AddAsync(attendance, ct);
        await context.SaveChangesAsync(ct);

        await cache.RemoveByTagAsync("Attendance", ct);

        logger.LogInformation("Attendance with ID {AttendanceId} created for Member ID {MemberId}.", attendance.Id, command.MemberId);

        return new AttendanceResponse
        {
            AttendanceId = attendance.Id,
            MemberId = attendance.MemberId,
            MemberName = $"{member.Person.FirstName} {member.Person.LastName}",
            CheckInAtUtc = attendance.CheckInAtUtc
        };
    }
}
