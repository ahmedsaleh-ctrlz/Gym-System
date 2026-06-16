using System;
using System.Collections.Generic;
using System.Text;

using Gym.Domain.Plans;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gym.Infrastructure.Data.Configuration;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(200);

        builder.Property(x => x.Cost)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.AllowedFreezeCount)
            .IsRequired();

        builder.Property(x => x.MaxTotalFreezeDays)
            .IsRequired();
    }
}