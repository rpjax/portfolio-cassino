using Aidan.Core.Linq;

namespace Domain.Core.Services;

public interface IRepositoryService<TEntity> 
    where TEntity : class
{
    IAsyncQueryable<TEntity> AsQueryable();
    Task CreateAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(TEntity entity);
}
