using CseLibrary.Tests.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace CseLibrary.Tests.Spikes;

/// <summary>
/// Step 0.11a (D-29 condition 1): HiLo demonstration on its own disposable LocalDB database
/// (<see cref="TestDatabase"/>, D-40 guard). Uses a spike model only — the app's model and migrations are untouched.
/// Shows: ids at AddAsync, audit row with the new EntityId in the same SaveChanges, rollback, retry strategy.
/// </summary>
public sealed class HiLoSpikeTests : IAsyncLifetime
{
    private const int BlockSize = 10;
    private readonly TestDatabase _database = new();

    public async Task InitializeAsync()
    {
        TestDatabase.EnsureSafe(_database.ConnectionString);
        await using var db = NewContext();
        await db.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync()
    {
        _database.Drop();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Id_is_assigned_at_AddAsync_from_a_block_of_ten()
    {
        await using var db = NewContext();
        var first = new SpikeItem { Name = Unique() };
        var second = new SpikeItem { Name = Unique() };

        await db.Items.AddAsync(first);
        await db.Items.AddAsync(second);

        Assert.NotEqual(0, first.Id); // known before SaveChanges
        Assert.Equal(first.Id + 1, second.Id); // same block, no round trip
        Assert.Equal(BlockSize, await SequenceIncrement());
    }

    [Fact]
    public async Task Audit_row_carries_the_new_EntityId_in_the_same_SaveChanges()
    {
        await using var db = NewContext();
        var item = new SpikeItem { Name = Unique() };
        await db.Items.AddAsync(item);

        Assert.Equal(2, await db.SaveChangesAsync()); // item + audit row, one call, one transaction

        var audit = await db.Audits.AsNoTracking().SingleAsync(a => a.EntityId == item.Id);
        Assert.Equal((nameof(SpikeItem), "Created"), (audit.EntityType, audit.Action));
    }

    [Fact]
    public async Task Rollback_leaves_neither_the_entity_nor_its_audit_row()
    {
        await using var db = NewContext();
        await using (var tx = await db.Database.BeginTransactionAsync())
        {
            var item = new SpikeItem { Name = Unique() };
            await db.Items.AddAsync(item);
            await db.SaveChangesAsync();
            await tx.RollbackAsync();
            await AssertGone(item.Id);
        }

        // A failing SaveChanges (duplicate unique name) also writes nothing, audit included.
        db.ChangeTracker.Clear();
        var name = Unique();
        await db.Items.AddAsync(new SpikeItem { Name = name });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var duplicate = new SpikeItem { Name = name };
        await db.Items.AddAsync(duplicate);
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        await AssertGone(duplicate.Id);
    }

    [Fact]
    public async Task Retrying_execution_strategy_reruns_with_a_new_id_and_commits_once()
    {
        await using var db = NewContext(retry: true);
        var attempts = 0;
        var ids = new List<int>();
        var name = Unique();

        await db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            attempts++;
            db.ChangeTracker.Clear(); // as UnitOfWork.ExecuteInTransactionAsync does
            await using var tx = await db.Database.BeginTransactionAsync();
            var item = new SpikeItem { Name = name };
            await db.Items.AddAsync(item);
            ids.Add(item.Id);
            await db.SaveChangesAsync();
            if (attempts == 1) throw new SimulatedTransientException(); // tx disposed → rolled back
            await tx.CommitAsync();
        });

        Assert.Equal(2, attempts);
        Assert.NotEqual(ids[0], ids[1]); // the lost id is a gap (documented D-29 trade-off)
        await AssertGone(ids[0]);
        var saved = await db.Items.AsNoTracking().SingleAsync(i => i.Name == name);
        Assert.Equal(ids[1], saved.Id);
        Assert.Equal(1, await db.Audits.CountAsync(a => a.EntityId == ids[1]));
    }

    private async Task AssertGone(int id)
    {
        await using var fresh = NewContext();
        Assert.False(await fresh.Items.AnyAsync(i => i.Id == id));
        Assert.False(await fresh.Audits.AnyAsync(a => a.EntityId == id));
    }

    private async Task<int> SequenceIncrement()
    {
        await using var conn = new SqlConnection(_database.ConnectionString);
        await conn.OpenAsync();
        await using var cmd = new SqlCommand("SELECT CAST(increment AS int) FROM sys.sequences WHERE name = 'SpikeItemHiLo'", conn);
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    private SpikeContext NewContext(bool retry = false) => new(new DbContextOptionsBuilder<SpikeContext>()
        .UseSqlServer(_database.ConnectionString, sql =>
        {
            if (retry) sql.ExecutionStrategy(d => new RetryOnSimulatedFailure(d));
        })
        .AddInterceptors(new SpikeAuditInterceptor())
        .Options);

    private static string Unique() => "item_" + Guid.NewGuid().ToString("N")[..8];

    private sealed class SpikeItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    private sealed class SpikeAudit
    {
        public long Id { get; set; }
        public string EntityType { get; set; } = "";
        public int EntityId { get; set; }
        public string Action { get; set; } = "";
    }

    private sealed class SpikeContext(DbContextOptions<SpikeContext> options) : DbContext(options)
    {
        public DbSet<SpikeItem> Items => Set<SpikeItem>();
        public DbSet<SpikeAudit> Audits => Set<SpikeAudit>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            b.HasSequence<int>("SpikeItemHiLo").IncrementsBy(BlockSize);
            b.Entity<SpikeItem>().Property(i => i.Id).UseHiLo("SpikeItemHiLo");
            b.Entity<SpikeItem>().HasIndex(i => i.Name).IsUnique();
        }
    }

    // What the D-21 interceptor would do: the key is already known while saving, so the audit row goes in the same SaveChanges.
    private sealed class SpikeAuditInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
        {
            var db = eventData.Context!;
            var added = db.ChangeTracker.Entries<SpikeItem>().Where(e => e.State == EntityState.Added).Select(e => e.Entity).ToList();
            db.AddRange(added.Select(i => new SpikeAudit { EntityType = nameof(SpikeItem), EntityId = i.Id, Action = "Created" }));
            return ValueTask.FromResult(result);
        }
    }

    private sealed class SimulatedTransientException : Exception;

    // The real EnableRetryOnFailure strategy, also retrying the simulated failure.
    private sealed class RetryOnSimulatedFailure(ExecutionStrategyDependencies dependencies)
        : Microsoft.EntityFrameworkCore.SqlServerRetryingExecutionStrategy(dependencies, 2, TimeSpan.Zero, null)
    {
        protected override bool ShouldRetryOn(Exception exception) =>
            exception is SimulatedTransientException || base.ShouldRetryOn(exception);
    }
}
