using Gym.Application.Common.Interfaces;
using Gym.Application.Common.Models;
using Gym.Application.Features.Members.Dtos;
using Gym.Application.Features.Members.Mappers;
using Gym.Domain.Common.Result;
using Gym.Domain.Subscriptions.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gym.Application.Features.Members.Queries.GetMembersWithActiveSubscription
{
    public sealed record GetMembersWithActiveSubscriptionQuery(
    ) : ICachedQuery<Result<List<ActiveMemberResponse>>>
    {
        public string cacheKey => "ActiveMembers";

        public string[] cacheTag => ["Member"];

        public TimeSpan cacheDuration => TimeSpan.FromMinutes(10);
    }


    public sealed record GetMembersWithActiveSubscriptionQueryHandler(IAppDbContext dbContext) : IRequestHandler<GetMembersWithActiveSubscriptionQuery,Result<List<ActiveMemberResponse>>>
    {
        public async  Task<Result<List<ActiveMemberResponse>>> Handle(GetMembersWithActiveSubscriptionQuery request, CancellationToken cancellationToken)
        {
            var members = await dbContext.Members.Include(m => m.Person).ThenInclude(p=>p.Image)
                .Where(m =>
                 dbContext.Subscriptions.Any(s =>
                s.MemberId == m.Id
                && s.Status == SubscriptionStatus.Active))
                .ToListAsync();

            return members.ToActiveMemberDtos();

        }
            
    }

}