using Aidan.EFCore;
using Domain.Core.Services;

namespace Application.Infra.Database.Services;

public class SqliteRepositoryProvider : IRepositoryProvider
{
    private EFCoreSqliteContext DbContext { get; }
    private Dictionary<Type, object> Repositories { get; }

    public SqliteRepositoryProvider(
        EFCoreSqliteContext dbContext, 
        Dictionary<Type, object>? repositories = null)
    {
        DbContext = dbContext;
        Repositories = new Dictionary<Type, object>(repositories ?? new());
    }

    public void Dispose()
    {
        DbContext.Dispose();
    }

    public IRepositoryService<TEntity> GetRepository<TEntity>() where TEntity : class
    {
        if(!Repositories.TryGetValue(typeof(TEntity), out var repository))
        {
            //throw new InvalidOperationException($"Repository for {typeof(TEntity).Name} not found.");
        }

        //return (IRepositoryService<TEntity>)repository;
        return new SqliteRepository<TEntity>(DbContext);
    }

    public async Task CommitAsync()
    {
        await DbContext.SaveChangesAsync();
    }

}