using Gym.Domain.Payments;
using Gym.Domain.Payments.Enums;
using Gym.Domain.Payments.Invoices;
using Gym.Domain.Plans;
using Gym.Domain.Subscriptions;
using Gym.Tests.Common.Members;
using Gym.Tests.Common.Payments.Invoices;
using Gym.Tests.Common.Plans;
using Gym.Tests.Common.PromoCodes;
using Gym.Tests.Common.Reflection;

namespace Gym.Domain.UnitTests.Payments.Invoices;

public class InvoiceTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_ShouldReturnError_WhenInvoiceNumberIsInvalid(string? invalidInvoiceNumber)
    {
        var payment = InvoiceFactory.CreatePaymentWithFullGraph();

        var result = Invoice.Create(invalidInvoiceNumber!, payment);

        Assert.True(result.IsError);
        Assert.Equal(InvoiceErrors.InvoiceNumberIsRequired.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenPaymentIsNull()
    {
        var result = Invoice.Create("INV-001", null!);

        Assert.True(result.IsError);
        Assert.Equal(InvoiceErrors.PaymentIsInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenSubscriptionPlanIsNull()
    {
        var payment = InvoiceFactory.CreatePaymentWithFullGraph();
        ReflectionTestHelper.SetProperty(payment.Subscription, nameof(Subscription.Plan), (Plan?)null);

        var result = Invoice.Create("INV-001", payment);

        Assert.True(result.IsError);
        Assert.Equal(InvoiceErrors.PlanIsInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenMemberPersonIsNull()
    {
        var payment = InvoiceFactory.CreatePaymentWithFullGraph();
        ReflectionTestHelper.SetProperty<Gym.Domain.Members.Member, Gym.Domain.People.Person?>(payment.Subscription.Member, nameof(payment.Subscription.Member.Person), null);

        var result = Invoice.Create("INV-001", payment);

        Assert.True(result.IsError);
        Assert.Equal(InvoiceErrors.MemberIsInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnSuccess_AndSnapshotAllFieldsAccurately()
    {
        var payment = InvoiceFactory.CreatePaymentWithFullGraph(planCost: 600m, paymentMethod: PaymentMethod.Visa);
        var before = DateTime.UtcNow;

        var result = Invoice.Create("INV-2026-9999", payment);

        Assert.True(result.IsSuccess);
        var invoice = result.Value;
        Assert.Equal("INV-2026-9999", invoice.InvoiceNumber);
        Assert.Equal(payment.Id, invoice.PaymentId);
        Assert.Equal($"{payment.Subscription.Member.Person.FirstName} {payment.Subscription.Member.Person.LastName}", invoice.MemberName);
        Assert.Equal(payment.Subscription.Plan!.Title, invoice.PlanName);
        Assert.Equal(payment.Subscription.StartDate, invoice.SubscriptionStartDate);
        Assert.Equal(payment.Subscription.EndDate, invoice.SubscriptionEndDate);
        Assert.Equal(payment.SubTotal, invoice.SubTotal);
        Assert.Equal(payment.Discount, invoice.Discount);
        Assert.Equal(payment.Tax, invoice.Tax);
        Assert.Equal(payment.Amount, invoice.Total);
        Assert.Equal(payment.PaymentMethod, invoice.PaymentMethod);
        Assert.Equal(payment.PaymentReference, invoice.PaymentReference);
        Assert.True(invoice.IssuedAt >= before.AddSeconds(-1));
    }
}
