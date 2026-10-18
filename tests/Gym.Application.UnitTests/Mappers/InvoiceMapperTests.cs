using Gym.Application.Features.Invoices.Mappers;
using Gym.Domain.Payments.Enums;
using Gym.Domain.Payments.Invoices;
using Gym.Tests.Common.Payments.Invoices;
using Gym.Tests.Common.Reflection;

namespace Gym.Application.UnitTests.Mappers;

public class InvoiceMapperTests
{
    [Fact]
    public void ToResponse_ShouldMapAllInvoiceFieldsToInvoiceResponse()
    {
        var payment = InvoiceFactory.CreatePaymentWithFullGraph(planCost: 750m, paymentMethod: PaymentMethod.Visa);
        var invoice = Invoice.Create("INV-2026-0042", payment).Value;
        ReflectionTestHelper.SetProperty(invoice, nameof(Invoice.Id), 42);

        var response = invoice.ToResponse();

        Assert.Equal(42, response.Id);
        Assert.Equal("INV-2026-0042", response.InvoiceNumber);
        Assert.Equal(payment.Id, response.PaymentId);
        Assert.Equal(invoice.MemberName, response.MemberName);
        Assert.Equal(invoice.PlanName, response.PlanName);
        Assert.Equal(invoice.SubscriptionStartDate, response.SubscriptionStartDate);
        Assert.Equal(invoice.SubscriptionEndDate, response.SubscriptionEndDate);
        Assert.Equal(750m, response.SubTotal);
        Assert.Equal(0m, response.Discount);
        Assert.Equal(0m, response.Tax);
        Assert.Equal(750m, response.Total);
        Assert.Equal(PaymentMethod.Visa, response.PaymentMethod);
        Assert.Equal(invoice.IssuedAt, response.IssuedAt);
    }
}
