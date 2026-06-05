using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Members.Dtos;
using Gym.Domain.Common.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Gym.Application.Features.Members.Queries.GetCurrentMember
{
    public sealed class GetCurrentMemberQueryHandler(IAppDbContext dbContext,IUser currentUser,IIdentityService identityService) : IRequestHandler<GetCurrentMemberQuery, Result<MemberResponse>>
    {
        public async Task<Result<MemberResponse>> Handle(GetCurrentMemberQuery request, CancellationToken cancellationToken)
        {
            var personId = Convert.ToInt32(currentUser.PersonId);

            var member = await dbContext.Members.Include(m=>m.Person).ThenInclude(p=>p.Image).FirstOrDefaultAsync(m => m.PersonId == personId, cancellationToken);
            if (member is null) 
            {
                return ApplicationErrors.MemberNotFound;
            }

            var email = await identityService.GetEmailByPersonIdAsync(personId, cancellationToken);

            return new MemberResponse
            {
                MemberId = member.Id,
                FirstName = member.Person.FirstName,
                LastName = member.Person.LastName,
                Email = email.Value,
                ImageUrl = member.Person.Image?.ImageUrl,
                PhoneNumber = member.Person.PhoneNumber,
                JoinDate = member.JoinDate,
            };

        }
    }
}
