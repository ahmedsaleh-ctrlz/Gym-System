using Gym.Application.Common.Interfaces;
using Gym.Domain.Payments.Invoices;

using System.Globalization;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Gym.Infrastructure.Files;

public sealed class InvoicePdfGenerator : IInvoicePdfGenerator
{
    private const string Primary = "#172554";
    private const string PrimaryLight = "#EFF6FF";
    private const string Accent = "#2563EB";
    private const string AccentLight = "#DBEAFE";

    private const string TextPrimary = "#111827";
    private const string TextSecondary = "#6B7280";
    private const string Border = "#E5E7EB";
    private const string Background = "#F8FAFC";

    private const string Success = "#15803D";
    private const string SuccessLight = "#DCFCE7";

    public byte[] Generate(Invoice invoice)
    {
        return Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(42);
                page.MarginVertical(36);

                page.PageColor(Colors.White);

                page.DefaultTextStyle(
                    TextStyle.Default
                        .FontFamily("Lato")
                        .FontSize(10)
                        .FontColor(TextPrimary));

                page.Header()
                    .Element(container =>
                        ComposeHeader(container, invoice));

                page.Content()
                    .PaddingTop(24)
                    .Element(container =>
                        ComposeContent(container, invoice));

                page.Footer()
                    .PaddingTop(16)
                    .BorderTop(1)
                    .BorderColor(Border)
                    .Element(ComposeFooter);
            });
        }).GeneratePdf();
    }

    private void ComposeHeader(
        IContainer container,
        Invoice invoice)
    {
        container
            .Background(Primary)
            .CornerRadius(12)
            .PaddingHorizontal(22)
            .PaddingVertical(18)
            .Row(row =>
            {
                row.RelativeItem()
                    .Column(column =>
                    {
                        column.Item()
                            .Text("GYM")
                            .FontSize(26)
                            .Bold()
                            .FontColor(Colors.White);

                        column.Item()
                            .PaddingTop(2)
                            .Text("MEMBERSHIP MANAGEMENT")
                            .FontSize(8)
                            .SemiBold()
                            .LetterSpacing(1.2f)
                            .FontColor("#BFDBFE");
                    });

                row.ConstantItem(190)
                    .AlignRight()
                    .Column(column =>
                    {
                        column.Item()
                            .AlignRight()
                            .Text("INVOICE")
                            .FontSize(22)
                            .Bold()
                            .FontColor(Colors.White);

                        column.Item()
                            .PaddingTop(4)
                            .AlignRight()
                            .Text(invoice.InvoiceNumber)
                            .FontSize(10)
                            .SemiBold()
                            .FontColor("#DBEAFE");

                        column.Item()
                            .PaddingTop(8)
                            .AlignRight()
                            .Background(SuccessLight)
                            .CornerRadius(20)
                            .PaddingHorizontal(11)
                            .PaddingVertical(5)
                            .Text("PAID")
                            .FontSize(8)
                            .Bold()
                            .FontColor(Success);
                    });
            });
    }

    private void ComposeContent(
        IContainer container,
        Invoice invoice)
    {
        container.Column(column =>
        {
            column.Spacing(18);

            column.Item()
                .Element(inner =>
                    ComposeInvoiceMeta(inner, invoice));

            column.Item()
                .Row(row =>
                {
                    row.RelativeItem()
                        .Element(inner =>
                        {
                            ComposeCard(
                                inner,
                                "BILL TO",
                                cardColumn =>
                                {
                                    cardColumn.Item()
                                        .Text(invoice.MemberName)
                                        .FontSize(14)
                                        .Bold()
                                        .FontColor(TextPrimary);

                                    cardColumn.Item()
                                        .PaddingTop(7)
                                        .Text("Gym Member")
                                        .FontSize(9)
                                        .FontColor(TextSecondary);
                                });
                        });

                    row.ConstantItem(14);

                    row.RelativeItem()
                        .Element(inner =>
                        {
                            ComposeCard(
                                inner,
                                "SUBSCRIPTION",
                                cardColumn =>
                                {
                                    cardColumn.Item()
                                        .Text(invoice.PlanName)
                                        .FontSize(14)
                                        .Bold()
                                        .FontColor(TextPrimary);

                                    cardColumn.Item()
                                        .PaddingTop(7)
                                        .Text(
                                            $"{invoice.SubscriptionStartDate.ToString(
                                                "dd MMM yyyy",
                                                CultureInfo.InvariantCulture)}  →  " +
                                            $"{invoice.SubscriptionEndDate.ToString(
                                                "dd MMM yyyy",
                                                CultureInfo.InvariantCulture)}")
                                        .FontSize(9)
                                        .FontColor(TextSecondary);
                                });
                        });
                });

            column.Item()
                .Element(inner =>
                    ComposePricing(inner, invoice));

            column.Item()
                .Element(inner =>
                    ComposePaymentInfo(inner, invoice));

            column.Item()
                .Element(ComposeThankYou);
        });
    }

    private void ComposeInvoiceMeta(
        IContainer container,
        Invoice invoice)
    {
        container.Row(row =>
        {
            row.RelativeItem()
                .Column(column =>
                {
                    column.Item()
                        .Text("ISSUED DATE")
                        .FontSize(8)
                        .SemiBold()
                        .FontColor(TextSecondary);

                    column.Item()
                        .PaddingTop(3)
                        .Text(
                        invoice.IssuedAt.ToString(
                            "dd MMM yyyy, HH:mm",
                            CultureInfo.InvariantCulture))
                        .FontSize(10)
                        .SemiBold();
                });

            row.RelativeItem()
                .AlignRight()
                .Column(column =>
                {
                    column.Item()
                        .AlignRight()
                        .Text("PAYMENT STATUS")
                        .FontSize(8)
                        .SemiBold()
                        .FontColor(TextSecondary);

                    column.Item()
                        .PaddingTop(3)
                        .AlignRight()
                        .Text("Completed")
                        .FontSize(10)
                        .SemiBold()
                        .FontColor(Success);
                });
        });
    }

    private void ComposePricing(
        IContainer container,
        Invoice invoice)
    {
        container
            .Border(1)
            .BorderColor(Border)
            .CornerRadius(10)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.ConstantColumn(130);
                });

                table.Header(header =>
                {
                    header.Cell()
                        .Background(Primary)
                        .PaddingHorizontal(12)
                        .PaddingVertical(9)
                        .Text("DESCRIPTION")
                        .FontSize(8)
                        .Bold()
                        .FontColor(Colors.White);

                    header.Cell()
                        .Background(Primary)
                        .PaddingHorizontal(12)
                        .PaddingVertical(9)
                        .AlignRight()
                        .Text("AMOUNT")
                        .FontSize(8)
                        .Bold()
                        .FontColor(Colors.White);
                });

                table.Cell()
                    .BorderBottom(1)
                    .BorderColor(Border)
                    .PaddingHorizontal(12)
                    .PaddingVertical(10)
                    .Text($"Membership Plan — {invoice.PlanName}")
                    .FontSize(9);

                table.Cell()
                    .BorderBottom(1)
                    .BorderColor(Border)
                    .PaddingHorizontal(12)
                    .PaddingVertical(10)
                    .AlignRight()
                    .Text(FormatAmount(invoice.SubTotal))
                    .FontSize(9);

                if (invoice.Discount > 0)
                {
                    table.Cell()
                        .BorderBottom(1)
                        .BorderColor(Border)
                        .PaddingHorizontal(12)
                        .PaddingVertical(10)
                        .Text(
                            string.IsNullOrWhiteSpace(invoice.PromoCode)
                                ? "Discount"
                                : $"Discount ({invoice.PromoCode})")
                        .FontSize(9)
                        .FontColor(Success);

                    table.Cell()
                        .BorderBottom(1)
                        .BorderColor(Border)
                        .PaddingHorizontal(12)
                        .PaddingVertical(10)
                        .AlignRight()
                        .Text($"-{FormatAmount(invoice.Discount)}")
                        .FontSize(9)
                        .FontColor(Success);
                }

                table.Cell()
                    .BorderBottom(1)
                    .BorderColor(Border)
                    .PaddingHorizontal(12)
                    .PaddingVertical(10)
                    .Text("Tax")
                    .FontSize(9);

                table.Cell()
                    .BorderBottom(1)
                    .BorderColor(Border)
                    .PaddingHorizontal(12)
                    .PaddingVertical(10)
                    .AlignRight()
                    .Text(FormatAmount(invoice.Tax))
                    .FontSize(9);

                table.Cell()
                    .Background(PrimaryLight)
                    .PaddingHorizontal(12)
                    .PaddingVertical(12)
                    .Text("TOTAL")
                    .FontSize(11)
                    .Bold()
                    .FontColor(Primary);

                table.Cell()
                    .Background(AccentLight)
                    .PaddingHorizontal(12)
                    .PaddingVertical(12)
                    .AlignRight()
                    .Text(FormatAmount(invoice.Total))
                    .FontSize(13)
                    .Bold()
                    .FontColor(Primary);
            });
    }

    private void ComposePaymentInfo(
        IContainer container,
        Invoice invoice)
    {
        container
            .Background(Background)
            .CornerRadius(10)
            .Padding(16)
            .Row(row =>
            {
                row.RelativeItem()
                    .Column(column =>
                    {
                        column.Item()
                            .Text("PAYMENT METHOD")
                            .FontSize(8)
                            .Bold()
                            .FontColor(TextSecondary);

                        column.Item()
                            .PaddingTop(5)
                            .Text(
                                invoice.PaymentMethod?.ToString()
                                ?? "Free Payment")
                            .FontSize(11)
                            .SemiBold();

                        if (!string.IsNullOrWhiteSpace(
                                invoice.PaymentReference))
                        {
                            column.Item()
                                .PaddingTop(3)
                                .Text(
                                    $"Reference: {invoice.PaymentReference}")
                                .FontSize(8)
                                .FontColor(TextSecondary);
                        }
                    });

                row.ConstantItem(170)
                    .AlignRight()
                    .Column(column =>
                    {
                        column.Item()
                            .AlignRight()
                            .Text("PAYMENT ID")
                            .FontSize(8)
                            .Bold()
                            .FontColor(TextSecondary);

                        column.Item()
                            .PaddingTop(5)
                            .AlignRight()
                            .Text(invoice.PaymentId.ToString())
                            .FontSize(11)
                            .SemiBold()
                            .FontColor(Primary);
                    });
            });
    }

    private void ComposeThankYou(IContainer container)
    {
        container
            .BorderLeft(3)
            .BorderColor(Accent)
            .Background(PrimaryLight)
            .PaddingHorizontal(14)
            .PaddingVertical(11)
            .Column(column =>
            {
                column.Item()
                    .Text("Thank you for choosing our gym.")
                    .FontSize(10)
                    .SemiBold()
                    .FontColor(Primary);

                column.Item()
                    .PaddingTop(3)
                    .Text(
                        "This invoice confirms that your membership payment " +
                        "has been completed successfully.")
                    .FontSize(8)
                    .FontColor(TextSecondary);
            });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem()
                .Text(text =>
                {
                    text.Span("Generated by ")
                        .FontSize(8)
                        .FontColor(TextSecondary);

                    text.Span("GYM Management")
                        .FontSize(8)
                        .SemiBold()
                        .FontColor(Primary);
                });

            row.ConstantItem(90)
                .AlignRight()
                .Text(text =>
                {
                    text.Span("Page ")
                        .FontSize(8)
                        .FontColor(TextSecondary);

                    text.CurrentPageNumber();

                    text.Span(" / ")
                        .FontSize(8)
                        .FontColor(TextSecondary);

                    text.TotalPages();
                });
        });
    }

    private static void ComposeCard(
        IContainer container,
        string title,
        Action<ColumnDescriptor> content)
    {
        container
            .Background(Colors.White)
            .Border(1)
            .BorderColor(Border)
            .CornerRadius(10)
            .Padding(15)
            .Column(column =>
            {
                column.Item()
                    .Text(title)
                    .FontSize(8)
                    .Bold()
                    .LetterSpacing(0.8f)
                    .FontColor(TextSecondary);

                column.Item()
                    .PaddingTop(8);

                content(column);
            });
    }

    private static string FormatAmount(decimal amount)
    {
        return $"{amount:N2} EGP";
    }
}