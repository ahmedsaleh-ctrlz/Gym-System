using System;
using System.Collections.Generic;
using System.Text;

using Gym.Application.Common.Interfaces;
using Gym.Application.Features.PromoCodes.Dtos;
using Gym.Domain.Common.Result;

namespace Gym.Application.Features.PromoCodes.Queries.GetPromoCodesQuery
{
    public sealed record GetPromoCodesQuery(
    bool? IsActive = null,
    string? Search = null
) : ICachedQuery<Result<IEnumerable<PromoCodeResponse>>>
    {
        public string CacheKey =>
            $"PromoCodes:{IsActive}:{Search}";

        public string[] CacheTag => ["PromoCodes"];

        public TimeSpan CacheDuration => TimeSpan.FromMinutes(15);
    }
}
