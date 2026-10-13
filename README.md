# CSE Department Library Management System

Web application for the CSE Department library: book titles and individual physical copies,
Student and Department Fellow members, issue/return with history, a member self-service portal,
and due-date / overdue email reminders (no fines in v1).

**Stack:** ASP.NET Core MVC, C#, EF Core, SQL Server, ASP.NET Core Identity, Razor/Bootstrap, xUnit.

## Status

Phase 0 (foundation) in progress — see [docs/PHASE_0_PLAN.md](docs/PHASE_0_PLAN.md) and
[docs/SESSION_HANDOFF.md](docs/SESSION_HANDOFF.md). The code is still the received starter in
`DefaultTemplate/` (net8.0); step 0.3 renames it to `src/CseLibrary.*` + `tests/CseLibrary.Tests`
and step 0.4 moves it to .NET 10.

## Repository layout

| Path | Contents |
|---|---|
| `DefaultTemplate/` | Received 4-project starter (Web, service, repository, core) — renamed in step 0.3 |
| `docs/` | Requirements, business rules, decisions, architecture, phase plans |
| `docs/archive/` | Original requirements discussion (byte-identical, historical) |
| `CLAUDE.md` | Instructions for AI-assisted sessions |

## Documentation

- [Requirements](docs/requirements.md) · [Business rules](docs/business-rules.md)
- [Architecture](docs/architecture.md) · [Decisions](docs/PROJECT_DECISIONS.md) ·
  [Decision log](docs/DECISION_LOG.md)

## Quick start

Not runnable yet as a library app. Do **not** run the template before step 0.4: its
`appsettings.json` still points at the `DefaultTemplate` database, and startup applies migrations.
Build/run/test commands will be documented here once steps 0.3–0.5 land.

## License

See [LICENSE](LICENSE).
