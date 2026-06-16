using Gym.Domain.Attendance;
using Gym.Domain.Coaches;
using Gym.Domain.Identity;
using Gym.Domain.Members;
using Gym.Domain.Payments;
using Gym.Domain.Plans;
using Gym.Domain.Subscriptions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Gym.Application.Common.Interfaces;

public interface IAppDbContext
{
    public DbSet<Member> Members { get; }

    public DbSet<Coach> Coaches { get; }

    public DatabaseFacade Database { get; }
    public DbSet<RefreshToken> RefreshTokens { get; }

    public DbSet<Plan> Plans { get; }

    public DbSet<Subscription> Subscriptions { get; }

    public DbSet<Attendance> Attendances { get; }

    public DbSet<Payment> Payments { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}