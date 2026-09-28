using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Features.Dashboard.Dtos
{
    public sealed class RevenuePoint
    {
        public DateOnly Date { get; init; }
        public decimal Amount { get; init; }
    }
}
