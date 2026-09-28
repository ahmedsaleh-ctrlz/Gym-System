using Gym.Domain.Plans;
using Gym.Domain.PromoCodes;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gym.Infrastructure.Data.Configuration;

public sealed class PromoCodeConfiguration : IEntityTypeConfiguration<PromoCode>
{
    public void Configure(EntityTypeBuilder<PromoCode> builder)
    {
        builder.ToTable("PromoCodes");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.Code)
            .IsUnique();

        builder.Property(p => p.DiscountType)
            .IsRequired();

        builder.Property(p => p.DiscountValue)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.MaxDiscount)
            .HasPrecision(18, 2);

        builder.Property(p => p.MinimumPurchaseAmount)
            .HasPrecision(18, 2);
        builder.Property(p => p.Audience)
            .IsRequired();

        builder.Property(p => p.UsageLimit)
            .IsRequired();

        builder.Property(p => p.UsageCount)
            .IsRequired();

        builder.Property(p => p.ExpiresAtUtc)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.HasOne(p => p.Plan)
            .WithMany()
            .HasForeignKey(p => p.PlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}