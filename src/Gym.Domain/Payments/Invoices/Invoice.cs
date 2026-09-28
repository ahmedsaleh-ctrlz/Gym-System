using Gym.Domain.Common;
using Gym.Domain.Common.Result;
using Gym.Domain.Payments.Enums;

namespace Gym.Domain.Payments.Invoices;

public sealed class Invoice : Entity
{
    public string InvoiceNumber { get; private set; } = null!;
    public int PaymentId { get; private set; }

    public string MemberName { get; private set; } = null!;

    public string PlanName { get; private set; } = null!;
    public DateOnly SubscriptionStartDate { get; private set; }
    public DateOnly SubscriptionEndDate { get; private set; }

    public decimal SubTotal { get; private set; }
    public decimal Discount { get; private set; }
    public decimal Tax { get; private set; }
    public decimal Total { get; private set; }

    public string? PromoCode { get; private set; }

    public PaymentMethod? PaymentMethod { get; private set; }
    public string? PaymentReference { get; private set; }

    public DateTime IssuedAt { get; private set; }

    public Payment Payment { get; private set; } = null!;

    private Invoice() { }

    public static Result<Invoice> Create(
    string invoiceNumber,
    Payment payment)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
        {
            return InvoiceErrors.InvoiceNumberIsRequired;
        }

        if (payment is null)
        {
            return InvoiceErrors.PaymentIsInvalid;
        }

        if (payment.Subscription.Plan is null)
        {
            return InvoiceErrors.PlanIsInvalid;
        }

        if (payment.Subscription.Member.Person is null)
        {
            return InvoiceErrors.MemberIsInvalid;
        }

        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            PaymentId = payment.Id,

            MemberName =
                $"{payment.Subscription.Member.Person.FirstName} " +
                $"{payment.Subscription.Member.Person.LastName}",

            PlanName = payment.Subscription.Plan.Title,

            SubscriptionStartDate =
                payment.Subscription.StartDate,

            SubscriptionEndDate =
                payment.Subscription.EndDate,

            SubTotal = payment.SubTotal,
            Discount = payment.Discount,
            Tax = payment.Tax,
            Total = payment.Amount,

            PromoCode = payment.PromoCode?.Code,

            PaymentMethod = payment.PaymentMethod,
            PaymentReference = payment.PaymentReference,

            IssuedAt = DateTime.UtcNow
        };

        return invoice;
    }
}