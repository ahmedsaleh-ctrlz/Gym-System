using Gym.Domain.Payments.Enums;

namespace Gym.Application.Features.Invoices.Dtos;

public sealed record InvoiceResponse(
    int Id,
    string InvoiceNumber,
    int PaymentId,
    string MemberName,
    string PlanName,
    DateOnly SubscriptionStartDate,
    DateOnly SubscriptionEndDate,
    decimal SubTotal,
    decimal Discount,
    decimal Tax,
    decimal Total,
    string? PromoCode,
    PaymentMethod? PaymentMethod,
    string? PaymentReference,
    DateTime IssuedAt);