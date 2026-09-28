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

        return invoice.ToResponse();
    }
}