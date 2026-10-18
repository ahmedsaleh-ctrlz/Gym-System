using Gym.Application.Common.Errors;
using Gym.Application.Features.Invoices.Queries.GetInvoiceByPaymentId;
using Gym.Application.SubcutaneousTests.Common;
using Gym.Domain.Payments.Invoices;

namespace Gym.Application.SubcutaneousTests.Features.Invoices;

public class InvoiceFeatureTests
{
    [Fact]
    public async Task GetInvoiceByPaymentIdQuery_ShouldReturnNotFound_WhenInvoiceDoesNotExist()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();

        var result = await context.Mediator.Send(new GetInvoiceByPaymentIdQuery(99999));

        Assert.True(result.IsError);
        Assert.Equal(ApplicationErrors.InvoiceNotFound.Code, result.TopError.Code);
    }

    [Fact]
    public async Task GetInvoiceByPaymentIdQuery_ShouldReturnInvoice_WhenExists()
    {
        await using var context = await SubcutaneousTestContext.CreateAsync();
        var member = await TestDataSeeder.AddMemberAsync(context, "John", "Doe");
        var plan = await TestDataSeeder.AddPlanAsync(context, "Gold Plan", 500m);
        var sub = await TestDataSeeder.AddSubscriptionAsync(context, member, plan);
        var payment = await TestDataSeeder.AddPaymentAsync(context, sub, markAsPaid: true);

        // Explicitly load navigation properties so Invoice.Create succeeds
        await context.DbContext.Entry(sub).Reference(s => s.Member).LoadAsync();
        await context.DbContext.Entry(sub.Member).Reference(m => m.Person).LoadAsync();
        await context.DbContext.Entry(sub).Reference(s => s.Plan).LoadAsync();

        var invoice = Invoice.Create("INV-TEST-001", payment).Value;
        await context.DbContext.Invoices.AddAsync(invoice);
        await context.DbContext.SaveChangesAsync();

        var result = await context.Mediator.Send(new GetInvoiceByPaymentIdQuery(payment.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal("INV-TEST-001", result.Value.InvoiceNumber);
        Assert.Equal(payment.Id, result.Value.PaymentId);
        Assert.Equal("John Doe", result.Value.MemberName);
        Assert.Equal("Gold Plan", result.Value.PlanName);
        Assert.Equal(500m, result.Value.Total);
    }
}
