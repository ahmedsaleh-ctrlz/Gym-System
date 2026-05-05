using Gym.Domain.Coachs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Gym.Infrastructure.Data.Configuration
{
    public class CoachConfiguration : IEntityTypeConfiguration<Coach>
    {
        public void Configure(EntityTypeBuilder<Coach> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasQueryFilter(e => e.IsActive);

            builder.Property(m => m.HireDate)
             .IsRequired().HasDefaultValueSql("GETDATE()");


            builder.HasOne(e => e.Person)
                .WithOne()
                .HasForeignKey<Coach>(c => c.PersonId);
        }
    }
}
