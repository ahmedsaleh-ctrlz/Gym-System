using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Application.Features.Invoices.Dtos;
using Gym.Application.Features.Invoices.Mappers;
using Gym.Domain.Common.Result;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Invoices.Queries.GetInvoiceByPaymentId;

public sealed record GetInvoiceByPaymentIdQuery(
    int PaymentId)
    : IRequest<Result<InvoiceResponse>>;

public sealed class GetInvoiceByPaymentIdQueryHandler(
    IAppDbContext dbContext,
    IUser user,
    ILogger<GetInvoiceByPaymentIdQueryHandler> logger)
    : IRequestHandler<
        GetInvoiceByPaymentIdQuery,
        Result<InvoiceResponse>>
{
    public async Task<Result<InvoiceResponse>> Handle(
        GetInvoiceByPaymentIdQuery request,
        CancellationToken ct)
    {
        var invoice = await dbContext.Invoices
            .Include(x => x.Payment)
                .ThenInclude(p => p.Subscription)
                    .ThenInclude(s => s.Member)
            .FirstOrDefaultAsync(
                x => x.PaymentId == request.PaymentId,
                ct);

        if (invoice is null)
        {
            logger.LogWarning(
                "Invoice with id {InvoiceId} not found.",
                request.PaymentId);

            return ApplicationErrors.InvoiceNotFound;
        }

        if (!string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(user.PersonId) ||
                !int.TryParse(user.PersonId, out var callerPersonId) ||
                invoice.Payment?.Subscription?.Member?.PersonId != callerPersonId)
            {
                logger.LogWarning(
                    "Forbidden access to invoice details for PaymentId {PaymentId} by PersonId {PersonId}.",
                    request.PaymentId,
                    user.PersonId);

                return ApplicationErrors.InvalidPaymentOwner;
            }
        }

        return invoice.ToResponse();
    }
}