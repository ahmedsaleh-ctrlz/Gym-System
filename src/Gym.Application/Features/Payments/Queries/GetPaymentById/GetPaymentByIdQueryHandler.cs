using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Payments.Dtos;
using Gym.Application.Features.Payments.Mappers;
using Gym.Domain.Common.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Payments.Queries.GetPaymentById;

public sealed class GetPaymentByIdQueryHandler(
    IAppDbContext context,
    ILogger<GetPaymentByIdQueryHandler> logger) : IRequestHandler<GetPaymentByIdQuery, Result<PaymentResponse>>
{
    public async Task<Result<PaymentResponse>> Handle(GetPaymentByIdQuery query, CancellationToken ct)
    {
        var payment = await context.Payments
            .AsNoTracking()
            .Include(p => p.Subscription)
                .ThenInclude(s => s.Member)
                    .ThenInclude(m => m.Person)
            .Include(p => p.Subscription)
                .ThenInclude(s => s.Plan)
            .FirstOrDefaultAsync(p => p.Id == query.Id, ct);

        if (payment is null)
        {
            logger.LogWarning("Payment with ID {PaymentId} not found.", query.Id);
            return ApplicationErrors.PaymentNotFound;
        }

        return payment.ToDto();
    }
}
