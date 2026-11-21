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
    IUser user,
    ILogger<GetInvoicePdfQueryHandler> logger)
    : IRequestHandler<GetInvoicePdfQuery, Result<byte[]>>
{
    public async Task<Result<byte[]>> Handle(
        GetInvoicePdfQuery request,
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
                    "Forbidden access to invoice for PaymentId {PaymentId} by PersonId {PersonId}.",
                    request.PaymentId,
                    user.PersonId);

                return ApplicationErrors.InvalidPaymentOwner;
            }
        }

        var pdf = invoicePdfGenerator.Generate(invoice);

        return pdf;
    }
}