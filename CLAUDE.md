# CSE Department Library Management System — Claude Code instructions

Web app for the CSE Department library: book titles + individual physical copies, Student and
Department Fellow members, issue/return with history, member self-service portal, due-date and
overdue email reminders (no fines in v1). Stack: ASP.NET Core MVC, C#, EF Core, SQL Server,
ASP.NET Core Identity, Razor/Bootstrap, xUnit. Built from the `DefaultTemplate/` starter.

## Start of every session (in this order)

1. Read `docs/SESSION_HANDOFF.md` — current state, next action, what is forbidden.
2. Before architectural or implementation work, read `docs/PROJECT_DECISIONS.md` and
   `docs/PHASE_0_PLAN.md` (or the active phase plan).
3. Open detailed docs only when the task needs them (see Doc map). Do not load everything.
4. Run `git status`, `git branch --show-current`, `git log --oneline -10`, and inspect the
   files the task touches. **Never assume an earlier session's proposed work was completed** —
   verify with Git, files, build and tests.

## Non-negotiable rules

- **Communicate with the user in Bangla**, keeping English technical terms. Write project docs
  and code in English.
- **Architecture:** follow `docs/PROJECT_DECISIONS.md`. Repository + Unit of Work is mandatory:
  Services inject `IUnitOfWork` and use its typed repository properties; repositories never
  call `SaveChanges`; no EF Core queries in Services; no DbContext/UoW/repositories in
  controllers. Fetch only needed data (projection, server-side paging, no N+1).
- **Only `Approved` decisions are binding.** `Proposed`/`Pending` items need the user's approval
  before implementation. Never upgrade a proposal to approved on your own.
- **Database safety:** never touch, migrate, drop, or reset the user's existing `DefaultTemplate`
  database on `.\SQLEXPRESS` (or any existing DB). Tests use isolated LocalDB databases named
  `CseLibrary_Test_*` only. No destructive command (DB drop, `git reset --hard`, force push,
  deleting files/migrations) without explicit user approval.
- **Small, reviewable commits.** Stop at each review gate in the phase plan and wait for the
  user's explicit approval before starting the next gate.
- **At each gate / meaningful milestone:** update `docs/SESSION_HANDOFF.md`, the phase plan
  checklist, and `docs/DECISION_LOG.md` (new decisions). Mark work Verified only with evidence
  (build/test output, git commit).
- **Git workflow (D-07 v7, Approved Session 13):** `main` → `develop` → `feature/phase-0-gate-N`,
  local only, `--no-ff`; show staged list + message and get the user's OK before every commit.
  Before each gate report run `/code-review` + `/ponytail-review` on the gate diff.
- Do not commit documentation or code unless the user asked for it.
- Ask the user when an unresolved decision materially affects the implementation.
- Never silently overwrite requirements or decisions; mark old ones `Superseded` with a link.

## Doc map

| File | Owns |
|---|---|
| `docs/SESSION_HANDOFF.md` | Current state, evidence, next action, restrictions |
| `docs/PROJECT_DECISIONS.md` | All decisions with status (Approved/Proposed/Pending/Rejected/Superseded) |
| `docs/PHASE_0_PLAN.md` | Phase 0 steps 0.1–0.12, gates, acceptance tests, checklist |
| `docs/DECISION_LOG.md` | Chronological discussion history and supersessions |
| `docs/archive/1st discussion.md` | Original requirements discussion (SRS v1.0, user's choices §8, spec v1.1). Historical source, byte-identical (SHA-256 `6826f819…a86b`); **read only specific sections when needed** (~85 KB) |
| `docs/requirements.md` | Functional requirements (FR-M9-01..06 from M9, FR-M10-01 from M10–M12, interim traceability; archive FRs added at each phase start) |
| `docs/business-rules.md` | Business rules (BR-M9-01..08 from M9, BR-M10-01..14 from M10–M12; archive BRs added at each phase start) |
| `docs/architecture.md` | Layers, dependency rules, current vs target structure; diagram in `docs/diagrams/` |
| `README.md` | Project overview, repository layout, quick start |

Created only when they have content: `docs/database.md`, `docs/development.md`, `docs/status.md`.

## Commands (once Phase 0 lands; verify paths first)

- Build: `dotnet build` · Test: `dotnet test` · Migrations: `dotnet ef migrations add <Name>`
  (project/startup paths: see `README.md` / phase plan).
