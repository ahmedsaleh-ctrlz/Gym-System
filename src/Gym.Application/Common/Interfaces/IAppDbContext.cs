using Gym.Domain.Coachs;
using Gym.Domain.Identity;
using Gym.Domain.Members;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Gym.Application.Common.Interfaces;

public interface IAppDbContext
{
    public DbSet<Member> Members { get;}

    public DbSet<Coach> Coaches { get;}

    public DatabaseFacade Database { get; }
    public DbSet<RefreshToken> RefreshTokens { get;}
   
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
