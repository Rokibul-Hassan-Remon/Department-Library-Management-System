# CSE Department Library Management System

Web application for the CSE Department library: book titles and individual physical copies,
Student and Department Fellow members, issue/return with history, a member self-service portal,
and due-date / overdue email reminders (no fines in v1).

**Stack:** ASP.NET Core MVC, C#, EF Core, SQL Server, ASP.NET Core Identity, Razor/Bootstrap, xUnit.

## Status

Phase 0 (foundation) in progress — see [docs/PHASE_0_PLAN.md](docs/PHASE_0_PLAN.md) and
[docs/SESSION_HANDOFF.md](docs/SESSION_HANDOFF.md). The code is the received starter, renamed to
`CseLibrary.*` (step 0.3), still on net8.0 until step 0.4.

## Repository layout

| Path | Contents |
|---|---|
| `CseLibrary.sln` | Solution |
| `src/CseLibrary.Web` | MVC controllers, views, authorization, composition root (`Program.cs`) |
| `src/CseLibrary.Services` | Business services |
| `src/CseLibrary.Data` | `ApplicationDbContext`, repositories, Unit of Work, migrations |
| `src/CseLibrary.Core` | Entities, DTOs, interfaces |
| `global.json`, `.config/dotnet-tools.json` | SDK pin (10.0, latest feature band); local `dotnet-ef` |
| `docs/` | Requirements, business rules, decisions, architecture, phase plans |
| `docs/archive/` | Original requirements discussion (byte-identical) and the template's README |
| `CLAUDE.md` | Instructions for AI-assisted sessions |

## Documentation

- [Requirements](docs/requirements.md) · [Business rules](docs/business-rules.md)
- [Architecture](docs/architecture.md) · [Decisions](docs/PROJECT_DECISIONS.md) ·
  [Decision log](docs/DECISION_LOG.md)

## Quick start

Not runnable yet as a library app. Do **not** run the template before step 0.4: its
`appsettings.json` still points at the `DefaultTemplate` database, and startup applies migrations.

```
dotnet tool restore
dotnet build CseLibrary.sln
```

Run/test commands follow once steps 0.4–0.5 land.

## License

See [LICENSE](LICENSE).
