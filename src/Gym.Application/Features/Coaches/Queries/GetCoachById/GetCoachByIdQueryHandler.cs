using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Coaches.Dtos;
using Gym.Application.Features.Coaches.Mappers;
using Gym.Domain.Common.Result;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Coaches.Queries.GetCoachById;

public class GetCoachByIdQueryHandler(
    IAppDbContext context, ILogger<GetCoachByIdQueryHandler> logger) : IRequestHandler<GetCoachByIdQuery, Result<CoachResponse>>
{
    public async Task<Result<CoachResponse>> Handle(GetCoachByIdQuery query, CancellationToken ct)
    {
        var coach = await context.Coaches.Include(c => c.Person).ThenInclude(p => p.Image).FirstOrDefaultAsync(c => c.Id == query.Id);

        if (coach is null)
        {
            logger.LogWarning("Coach with id {CoachId} not found.", query.Id);
            return ApplicationErrors.CoachNotFound;
        }

        return coach.ToDto();
    }
}