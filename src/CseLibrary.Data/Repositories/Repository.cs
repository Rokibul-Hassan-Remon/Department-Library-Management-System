using System.Linq.Expressions;
using CseLibrary.Core.Interfaces;
using CseLibrary.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace CseLibrary.Data.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _db;
    protected readonly DbSet<T> _set;

    public Repository(ApplicationDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public Task<T?> GetByIdAsync(object id) => _set.FindAsync(id).AsTask();

    public async Task<IEnumerable<T>> GetAllAsync() => await _set.AsNoTracking().ToListAsync();

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _set.AsNoTracking().Where(predicate).ToListAsync();

    public IQueryable<T> Query() => _set.AsQueryable();

    public Task AddAsync(T entity) => _set.AddAsync(entity).AsTask();

    public Task AddRangeAsync(IEnumerable<T> entities) => _set.AddRangeAsync(entities);

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);

    public void RemoveRange(IEnumerable<T> entities) => _set.RemoveRange(entities);

    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) => _set.AnyAsync(predicate);

    public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        => predicate == null ? _set.CountAsync() : _set.CountAsync(predicate);
}
