using CseLibrary.Core.DTOs;
using CseLibrary.Core.Entities;
using CseLibrary.Core.Interfaces.Repositories;
using CseLibrary.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace CseLibrary.Data.Repositories;

public class GeneralConfigRepository(ApplicationDbContext db) : Repository<GeneralConfig>(db), IGeneralConfigRepository
{
    public async Task<IReadOnlyList<GeneralConfigDto>> GetListAsync(CancellationToken ct = default) =>
        await ToDto(Db.GeneralConfigs.OrderBy(c => c.Group).ThenBy(c => c.Key)).ToListAsync(ct);

    public Task<GeneralConfigDto?> GetDetailsAsync(int id, CancellationToken ct = default) =>
        ToDto(Db.GeneralConfigs.Where(c => c.Id == id)).FirstOrDefaultAsync(ct);

    public Task<GeneralConfigDto?> GetByKeyAsync(string key, CancellationToken ct = default) =>
        ToDto(Db.GeneralConfigs.Where(c => c.Key == key)).FirstOrDefaultAsync(ct);

    public Task<bool> KeyExistsAsync(string key, int? excludeId, CancellationToken ct = default) =>
        Db.GeneralConfigs.AnyAsync(c => c.Key == key && c.Id != excludeId, ct);

    private static IQueryable<GeneralConfigDto> ToDto(IQueryable<GeneralConfig> q) =>
        q.Select(c => new GeneralConfigDto
        {
            Id = c.Id, Key = c.Key, Value = c.Value, Description = c.Description, Group = c.Group
        });
}
