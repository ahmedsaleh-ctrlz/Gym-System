using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Features.Dashboard.Dtos;

public sealed class AdminDashboardResponse
{
    // Members
    public int TotalMembers { get; init; }

    public int ActiveSubscriptions { get; init; }

    public int FrozenSubscriptions { get; init; }

    public int ScheduledSubscriptions { get; init; }

    public int ExpiredSubscriptions { get; init; }

    // Attendance
    public int TodayAttendanceCount { get; init; }

    public int ThisWeekAttendanceCount { get; init; }

    public int PeakHour { get; init; }

    public int PeakHourAttendanceCount { get; init; }

    // Revenue
    public decimal TodayRevenue { get; init; }

    public decimal ThisMonthRevenue { get; init; }

    public decimal TotalRevenue { get; init; }

    // Plans
    public string MostPopularPlan { get; init; } = string.Empty;

    public int MostPopularPlanSubscriptionsCount { get; init; }

    // Payments
    public int PendingPaymentsCount { get; init; }

    public int PaidPaymentsCount { get; init; }

    
}