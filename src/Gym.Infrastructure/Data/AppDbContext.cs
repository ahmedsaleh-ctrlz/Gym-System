
using Gym.Application.Common.Interfaces;
using Gym.Domain.Coaches;
using Gym.Domain.Identity;
using Gym.Domain.Members;
using Gym.Domain.People;
using Gym.Domain.People.PersonImages;
using Gym.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Gym.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) 
    : IdentityDbContext<AppUser>(options), IAppDbContext
{
    public DbSet<Member> Members => Set<Member>();

    public DbSet<Coach> Coaches => Set<Coach>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<PersonImage> PersonImages => Set<PersonImage>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
