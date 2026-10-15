using CseLibrary.Core.Entities;

namespace CseLibrary.Core.Interfaces.Repositories;

/// <summary>Append-only (D-21): audit rows are added, never read back, changed or deleted by services.</summary>
public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken ct = default);
}
