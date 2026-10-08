using System;

namespace Gym.Application.Features.Dashboard.Dtos;

public sealed record AdminMembersSummaryResponse(
    int TotalMembers,
    int NewMembersThisMonth,
    int NewMembersPreviousMonth
);
