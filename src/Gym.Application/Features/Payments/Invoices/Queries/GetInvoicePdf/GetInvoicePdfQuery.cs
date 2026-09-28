using Gym.Domain.Common.Result;

using MediatR;

namespace Gym.Application.Features.Invoices.Queries.GetInvoicePdf;

public sealed record GetInvoicePdfQuery(
    int PaymentId)
    : IRequest<Result<byte[]>>;
