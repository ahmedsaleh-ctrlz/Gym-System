using Gym.Domain.Payments.Invoices;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gym.Infrastructure.Persistence.Configurations;

public sealed class InvoiceDataConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.InvoiceNumber)
            .IsUnique();

        builder.Property(x => x.PaymentId)
            .IsRequired();

        builder.HasIndex(x => x.PaymentId)
            .IsUnique();

        builder.Property(x => x.MemberName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.PlanName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.SubscriptionStartDate)
            .IsRequired();

        builder.Property(x => x.SubscriptionEndDate)
            .IsRequired();

        builder.Property(x => x.SubTotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Discount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Tax)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Total)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.PromoCode)
            .HasMaxLength(100);

        builder.Property(x => x.PaymentMethod)
            .IsRequired(false);

        builder.Property(x => x.PaymentReference)
            .HasMaxLength(200);

        builder.Property(x => x.IssuedAt)
            .IsRequired();

        builder.HasOne(x => x.Payment)
            .WithOne()
            .HasForeignKey<Invoice>(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}