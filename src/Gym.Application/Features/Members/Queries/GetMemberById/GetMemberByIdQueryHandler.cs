using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Members.Dtos;
using Gym.Application.Features.Members.Mappers;
using Gym.Domain.Common.Result;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Members.Queries.GetMemberById;

public class GetMemberByIdQueryHandler(
    IAppDbContext context, ILogger<GetMemberByIdQueryHandler> logger, IIdentityService identityService) : IRequestHandler<GetMemberByIdQuery, Result<MemberResponse>>
{
    public async Task<Result<MemberResponse>> Handle(GetMemberByIdQuery query, CancellationToken ct)
    {
        var member = await context.Members.Include(m => m.Person).ThenInclude(p => p.Image)
            .FirstOrDefaultAsync(m => m.Id == query.Id, ct);

        var email = await identityService.GetEmailByPersonIdAsync(member.PersonId, ct);

        if (member is null)
        {
            logger.LogWarning("Member with id {MemberId} not found.", query.Id);
            return ApplicationErrors.MemberNotFound;
        }

        var response = member.ToDto();
        response.Email = email.Value;
        return response;
    }
}