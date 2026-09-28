using Gym.Domain.Members;
using Gym.Domain.PromoCodes;
using Gym.Domain.PromoCodes.PromoCodeUsage;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gym.Infrastructure.Data.Configuration;

public sealed class PromoCodeUsageConfiguration
    : IEntityTypeConfiguration<PromoCodeUsage>
{
    public void Configure(EntityTypeBuilder<PromoCodeUsage> builder)
    {
        builder.ToTable("PromoCodeUsages");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.UsedAtUtc)
            .IsRequired();

        builder.HasIndex(p => new
        {
            p.PromoCodeId,
            p.MemberId
        })
        .IsUnique();

        builder.HasOne(p => p.PromoCode)
            .WithMany()
            .HasForeignKey(p => p.PromoCodeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(p => p.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}