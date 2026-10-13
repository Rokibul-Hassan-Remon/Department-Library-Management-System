using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces;
using CseLibrary.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace CseLibrary.Data.Repositories;

/// <summary>Write-side base. Repositories never save; the Unit of Work does (D-12).</summary>
public abstract class Repository<T>(ApplicationDbContext db) : IRepository<T> where T : BaseEntity
{
    protected ApplicationDbContext Db { get; } = db;

    public Task<T?> GetForUpdateAsync(int id, CancellationToken ct = default) =>
        Db.Set<T>().FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task AddAsync(T entity, CancellationToken ct = default) => await Db.Set<T>().AddAsync(entity, ct);

    public void Remove(T entity) => Db.Set<T>().Remove(entity);
}
