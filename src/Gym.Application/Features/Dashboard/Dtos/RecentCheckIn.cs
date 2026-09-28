using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Features.Dashboard.Dtos
{
    public record class RecentCheckIn
    {
        public int MemberId { get; init; }
        public string MemberName { get; init; } = string.Empty;
        public string? ImageUrl { get; init; }
        public DateTime CheckInAtUtc { get; init; }
    }
}
