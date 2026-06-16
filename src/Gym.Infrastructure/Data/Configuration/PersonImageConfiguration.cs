using Gym.Domain.People.PersonImages;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gym.Infrastructure.Data.Configuration;

public class PersonImageConfiguration : IEntityTypeConfiguration<PersonImage>
{
    public void Configure(EntityTypeBuilder<PersonImage> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ImageUrl)
            .HasMaxLength(2048);
    }
}