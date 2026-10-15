using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces.Repositories;
using CseLibrary.Data.Data;

namespace CseLibrary.Data.Repositories;

public class AuditLogRepository(ApplicationDbContext db) : IAuditLogRepository
{
    public async Task AddAsync(AuditLog log, CancellationToken ct = default) => await db.AuditLogs.AddAsync(log, ct);
}
