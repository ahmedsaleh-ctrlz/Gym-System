
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
    IAppDbContext context, ILogger<GetMemberByIdQueryHandler> logger) : IRequestHandler<GetMemberByIdQuery, Result<MemberResponse>>
{
    public async Task<Result<MemberResponse>> Handle(GetMemberByIdQuery query, CancellationToken ct)
    {
        var member = await context.Members
            .Where(m => m.Id == query.id)
            .Select(m => m.ToDto())
            .FirstOrDefaultAsync(ct);

        if (member is null)
        {
            logger.LogWarning("Member with id {MemberId} not found.", query.id);
            return ApplicationErrors.MemberNotFound;
        }

        return member;
    }


    
}
