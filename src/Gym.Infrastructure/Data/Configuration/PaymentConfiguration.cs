using Gym.Domain.Payments;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gym.Infrastructure.Data.Configuration;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.Status)
            .IsRequired();

        builder.HasOne(p => p.Subscription)
            .WithMany()
            .HasForeignKey(p => p.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}