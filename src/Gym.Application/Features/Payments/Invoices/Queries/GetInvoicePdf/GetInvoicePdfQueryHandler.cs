using Gym.Application.Common.Errors;
using Gym.Application.Common.Interfaces;
using Gym.Domain.Common.Result;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Gym.Application.Features.Invoices.Queries.GetInvoicePdf;

public sealed class GetInvoicePdfQueryHandler(
    IAppDbContext dbContext,
    IInvoicePdfGenerator invoicePdfGenerator,
    ILogger<GetInvoicePdfQueryHandler> logger)
    : IRequestHandler<GetInvoicePdfQuery, Result<byte[]>>
{
    public async Task<Result<byte[]>> Handle(
        GetInvoicePdfQuery request,
        CancellationToken ct)
    {
        var invoice = await dbContext.Invoices
            .FirstOrDefaultAsync(
                x => x.Id == request.PaymentId,
                ct);

        if (invoice is null)
        {
            logger.LogWarning(
                "Invoice with id {InvoiceId} not found.",
                request.PaymentId);

            return ApplicationErrors.InvoiceNotFound;
        }

        var pdf = invoicePdfGenerator.Generate(invoice);

        return pdf;
    }
}