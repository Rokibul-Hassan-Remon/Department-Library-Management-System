using System.Collections.Concurrent;
using Default.core.Entities;
using Default.core.Interfaces;
using Default.repository.Data;

namespace Default.repository.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _db;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();
    private bool _disposed;

    public UnitOfWork(ApplicationDbContext db)
    {
        _db = db;
        Permissions = Repository<Permission>();
        RolePermissions = Repository<RolePermission>();
        GeneralConfigs = Repository<GeneralConfig>();
    }

    public IRepository<Permission> Permissions { get; }
    public IRepository<RolePermission> RolePermissions { get; }
    public IRepository<GeneralConfig> GeneralConfigs { get; }

    public IRepository<TEntity> Repository<TEntity>() where TEntity : class
        => (IRepository<TEntity>)_repositories.GetOrAdd(typeof(TEntity), _ => new Repository<TEntity>(_db));

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _db.SaveChangesAsync(cancellationToken);

    public void Dispose()
    {
        if (_disposed) return;
        _db.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
