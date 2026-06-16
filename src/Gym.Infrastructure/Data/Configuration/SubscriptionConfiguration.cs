using Gym.Domain.Subscriptions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gym.Infrastructure.Data.Configuration;

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");

        builder.Property(s => s.PriceSnapshot)
            .HasPrecision(18, 2)
            .IsRequired();
    }
}