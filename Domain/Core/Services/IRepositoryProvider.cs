namespace Domain.Core.Services;

public interface IRepositoryProvider : IDisposable
{
    IRepositoryService<TEntity> GetRepository<TEntity>() where TEntity : class;
    Task CommitAsync();
}