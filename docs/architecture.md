# Architecture

This file owns the **shape** of the solution: layers, dependency direction and current-vs-target
structure. The binding rules and their rationale live in
[PROJECT_DECISIONS.md](PROJECT_DECISIONS.md) — this file links to them instead of repeating them.

## Target (Phase 0 end)

| Project (D-41) | Layer | Owns | May reference |
|---|---|---|---|
| `src/CseLibrary.Web` | Web | Controllers, views, ViewModels, authorization, composition root | Services, Core; Data only for DI registration |
| `src/CseLibrary.Services` | Services | Business rules, use-case orchestration | Core, `Microsoft.Extensions.Identity.Core` |
| `src/CseLibrary.Data` | Data | `ApplicationDbContext`, repositories, `UnitOfWork`, migrations | Core, EF Core |
| `src/CseLibrary.Core` | Core | Entities, enums, DTOs/read models, interfaces | nothing EF/ASP.NET Core |
| `tests/CseLibrary.Tests` | Tests | Characterization, integration, architecture tests | all |

Request path: Controller → Service → `IUnitOfWork` (typed repository properties) → repository →
`ApplicationDbContext` → SQL Server.

Binding rules:
- Layer boundaries and project references — [D-10](PROJECT_DECISIONS.md#d-10-layer-boundaries--approved)
- Repository + Unit of Work, no service locator — [D-12](PROJECT_DECISIONS.md#d-12-repository--unit-of-work--approved)
- Repository contract (no `IQueryable` leaks, projected reads) — D-13
- Transactions — D-14 · Query efficiency (projection, server-side paging, no N+1) — D-15
- Explicit mapping, no AutoMapper — D-17
- Enforced by architecture tests E1/E3 ([PHASE_0_PLAN.md](PHASE_0_PLAN.md)).

## Current (received template, commit `1fdbe5e`)

Interactive diagram generated from the code:
[diagrams/template-architecture.html](diagrams/template-architecture.html)
(source: [diagrams/template-architecture.json](diagrams/template-architecture.json), every node
cites file/line evidence at `1fdbe5e`).

Step 0.3 renamed the projects without changing behaviour: `DefaultTemplate` → `src/CseLibrary.Web`,
`Default.service` → `src/CseLibrary.Services`, `Default.repository` → `src/CseLibrary.Data`,
`Default.core` → `src/CseLibrary.Core`. The diagram and the gap table below use the received names.

```
Browser ─HTTPS─► Web (DefaultTemplate) ─I*Service─► Services (Default.service)
                   │ [HasPermission]                 ├─ IUnitOfWork + Query() ─► UoW + Repository<T> ─┐
                   └─► Permission policy ─► Services  └─ user/role writes ─────► ASP.NET Identity ────┤
                                                                                                      ▼
                                       Startup DbSeeder ─MigrateAsync─► ApplicationDbContext ─► SQL Server
                                                                                              (DefaultTemplate DB)
```

Gaps against the target (fixed in Gate 2, steps 0.6–0.8):

| Gap | Where | Decision |
|---|---|---|
| Services run EF queries via `IRepository<T>.Query()` / `Include` | `Default.service/Services/*` | D-10, D-13 |
| Generic `Repository<TEntity>()` accessor on the UoW | `Repositories/UnitOfWork.cs` | D-12 |
| UoW is `IDisposable` and disposes the DI-owned DbContext | `Repositories/UnitOfWork.cs` | D-13 |
| AutoMapper | `Default.service/Mapping/MappingProfile.cs` | D-17 |
| Users list loads every user, then roles per user (N+1) | `UserService.cs:22-27` | D-15, D-16 |
| Services project references the repository project and ASP.NET Core | `Default.service.csproj` | D-10 |

## Regenerating the diagram

After an architecture change, update `diagrams/template-architecture.json` (or add a new diagram
for the renamed solution) and run from the repository root:

```
node <archify>/bin/archify.mjs finalize architecture docs/diagrams/<name>.json docs/diagrams/<name>.html --repo-root . --quality showcase --out-dir <scratch> --json
```

`meta.repository.revision` must be a committed revision; evidence is checked against it.
