using CseLibrary.Core.Entities;

namespace CseLibrary.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Permission> Permissions { get; }
    IRepository<RolePermission> RolePermissions { get; }
    IRepository<GeneralConfig> GeneralConfigs { get; }
    IRepository<TEntity> Repository<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
