using Gym.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gym.Infrastructure.Data.Configuration;
public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.HasOne(u => u.Person)
            .WithOne()
            .HasForeignKey<AppUser>(u => u.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => u.PersonId).IsUnique();
    }
}