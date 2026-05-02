using Gym.Domain.Members;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Gym.Application.Common.Interfaces;

public interface IAppDbContext
{
    public DbSet<Member> Members { get;}

    public DatabaseFacade Database { get; }


    public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
