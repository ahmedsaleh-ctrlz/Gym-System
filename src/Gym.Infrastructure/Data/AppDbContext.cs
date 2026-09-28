using Gym.Application.Common.Interfaces;
using Gym.Domain.Attendance;
using Gym.Domain.Coaches;
using Gym.Domain.Identity;
using Gym.Domain.Members;
using Gym.Domain.Notifications;
using Gym.Domain.Payments;
using Gym.Domain.Payments.Invoices;
using Gym.Domain.People;
using Gym.Domain.People.PersonImages;
using Gym.Domain.Plans;
using Gym.Domain.PromoCodes;
using Gym.Domain.PromoCodes.PromoCodeUsage;
using Gym.Domain.Subscriptions;
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
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<PersonImage> PersonImages => Set<PersonImage>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();

    public DbSet<PromoCodeUsage> PromoCodeUsages => Set<PromoCodeUsage>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}