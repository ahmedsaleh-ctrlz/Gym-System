using Gym.Application.Features.Invoices.Dtos;
using Gym.Domain.Payments.Invoices;

namespace Gym.Application.Features.Invoices.Mappers;

public static class InvoiceMapper
{
    public static InvoiceResponse ToResponse(
        this Invoice invoice)
    {
        return new InvoiceResponse(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.PaymentId,
            invoice.MemberName,
            invoice.PlanName,
            invoice.SubscriptionStartDate,
            invoice.SubscriptionEndDate,
            invoice.SubTotal,
            invoice.Discount,
            invoice.Tax,
            invoice.Total,
            invoice.PromoCode,
            invoice.PaymentMethod,
            invoice.PaymentReference,
            invoice.IssuedAt);
    }
}