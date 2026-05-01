using Gym.Domain.Classes;
using Gym.Domain.Classes.ClassBookings;
using Gym.Domain.Employees;
using Gym.Domain.Identity;
using Gym.Domain.Members;
using Gym.Domain.Members.MemberProgresses;
using Gym.Domain.Payments;
using Gym.Domain.Plans;
using Gym.Domain.PromoCodes;
using Gym.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Gym.Application.Common.Interfaces;

public interface IAppDbContext
{
    public DbSet<Member> Members { get;}
    public DbSet<Employee> Employees { get;}
    public DbSet<GymClass> Classes { get;}
    public DbSet<Subscription> Subscriptions { get;}

    public DbSet<Payment> Payments { get;}
    public DbSet<Plan> Plans { get;}
    
    public DbSet<PromoCode> PromoCodes { get;}
    public DbSet<RefreshToken> RefreshTokens { get;}

    public DbSet<ClassBooking> ClassBookings { get;}
    public DbSet<MemberProgress> MemberProgresses { get;}

    public DatabaseFacade Database { get; }


    public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
