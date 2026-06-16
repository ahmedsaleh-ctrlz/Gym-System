using Gym.Domain.People;
using Gym.Domain.People.PersonImages;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gym.Infrastructure.Data.Configuration
{
    public class PersonConfiguration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.FirstName)
                .IsRequired()
                .HasMaxLength(50);
            builder.Property(p => p.LastName)
                .IsRequired()
                .HasMaxLength(50);
            builder.Property(p => p.DateOfBirth)
                .IsRequired();

            builder.Property(c => c.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasOne(p => p.Image)
                .WithOne()
                .HasForeignKey<PersonImage>(i => i.Id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}