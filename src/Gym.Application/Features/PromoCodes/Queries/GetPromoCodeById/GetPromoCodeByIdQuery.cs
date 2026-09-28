using System;
using System.Collections.Generic;
using System.Text;

using Gym.Application.Common.Interfaces;
using Gym.Application.Features.PromoCodes.Dtos;
using Gym.Domain.Common.Result;
using Gym.Domain.PromoCodes;

namespace Gym.Application.Features.PromoCodes.Queries.GetPromoCodeByIdQuery
{
    public sealed record GetPromoCodeByIdQuery(
    int Id)
    : ICachedQuery<Result<PromoCodeResponse>>
    {
        public string CacheKey => $"PromoCode:{Id}";

        public string[] CacheTag => [$"PromoCode:{Id}", "PromoCodes"];

        public TimeSpan CacheDuration => TimeSpan.FromMinutes(5);
    }
}
