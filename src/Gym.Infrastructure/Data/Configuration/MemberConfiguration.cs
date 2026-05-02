
using Gym.Domain.Members;
using Gym.Domain.People;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gym.Infrastructure.Data.Configuration;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.JoinDate)
            .IsRequired().HasDefaultValueSql("GETDATE()");

        builder.Property(m => m.Notes).HasMaxLength(500);
        builder.Property(m => m.IsDeleted).IsRequired();
        builder.HasQueryFilter(m => !m.IsDeleted);

        builder.HasOne(m => m.Person)
        .WithOne()
        .HasForeignKey<Member>(m => m.PersonId);



    }
}
