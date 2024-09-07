using Aidan.Core.Linq;
using Aidan.EFCore;
using Aidan.EFCore.Linq;
using Domain.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace Application.Infra.Database.Services;

public class SqliteRepository<TEntity> : IRepositoryService<TEntity>
    where TEntity : class
{
    private EFCoreSqliteContext DbContext { get; }
    private DbSet<TEntity> DbSet { get; }

    public SqliteRepository(EFCoreSqliteContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<TEntity>();
    }

    public IAsyncQueryable<TEntity> AsQueryable()
    {
        return new EFCoreAsyncQueryable<TEntity>(DbSet);
    }

    public async Task CreateAsync(TEntity entity)
    {
        await DbSet.AddAsync(entity);
    }

    public Task UpdateAsync(TEntity entity)
    {
        DbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TEntity entity)
    {
        DbSet.Remove(entity);
        return Task.CompletedTask;
    }
}
