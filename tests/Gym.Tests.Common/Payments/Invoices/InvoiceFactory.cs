using Gym.Domain.Common.Result;
using Gym.Domain.Members;
using Gym.Domain.Payments;
using Gym.Domain.Payments.Enums;
using Gym.Domain.Payments.Invoices;
using Gym.Domain.Plans;
using Gym.Domain.Subscriptions;
using Gym.Tests.Common.Members;
using Gym.Tests.Common.Payments;
using Gym.Tests.Common.Plans;
using Gym.Tests.Common.Reflection;
using Gym.Tests.Common.Subscriptions;

namespace Gym.Tests.Common.Payments.Invoices;

public static class InvoiceFactory
{
    public static Payment CreatePaymentWithFullGraph(
        decimal planCost = 500m,
        PaymentMethod? paymentMethod = PaymentMethod.Cash,
        bool markAsPaid = true)
    {
        var member = MemberFactory.CreateMember().Value;
        ReflectionTestHelper.SetProperty(member, nameof(Member.Id), 1);
        ReflectionTestHelper.SetProperty(member.Person, "Id", 1);

        var plan = PlanFactory.CreatePlan(cost: planCost).Value;
        ReflectionTestHelper.SetProperty(plan, nameof(Plan.Id), 1);

        var subscription = Subscription.Create(member.Id, plan, DateOnly.FromDateTime(DateTime.UtcNow)).Value;
        ReflectionTestHelper.SetProperty(subscription, nameof(Subscription.Id), 1);
        ReflectionTestHelper.SetProperty(subscription, nameof(Subscription.Member), member);

        var payment = Payment.Create(subscription).Value;
        ReflectionTestHelper.SetProperty(payment, nameof(Payment.Id), 1);

        if (markAsPaid)
        {
            payment.Pay(paymentMethod ?? PaymentMethod.Cash);
        }

        return payment;
    }

    public static Result<Invoice> CreateInvoice(
        string invoiceNumber = "INV-2026-0001",
        Payment? payment = null)
    {
        var effectivePayment = payment ?? CreatePaymentWithFullGraph();
        return Invoice.Create(invoiceNumber, effectivePayment);
    }
}
