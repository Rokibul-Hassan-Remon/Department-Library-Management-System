using CseLibrary.Core.Entities;

namespace CseLibrary.Core.Interfaces;

/// <summary>Write side only (D-13). Reads are purpose-specific projections on each feature repository.</summary>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>Tracked load for an update or delete.</summary>
    Task<T?> GetForUpdateAsync(int id, CancellationToken ct = default);

    Task AddAsync(T entity, CancellationToken ct = default);

    /// <summary>Soft delete: the SaveChanges interceptor sets <see cref="RecordStatus.Deleted"/> (D-51).</summary>
    void Remove(T entity);
}
