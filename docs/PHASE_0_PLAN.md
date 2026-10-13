# Phase 0 Plan — Baseline, Architecture Foundation, Security, Audit

Goal: turn `DefaultTemplate` into a verified, secure foundation that follows the approved
architecture, before any library domain entity (Book, Member…) is added (Phase 1).
Decision references (D-xx) point to `docs/PROJECT_DECISIONS.md`.

## Rules for executing this plan

- **Plan status: not started.** Nothing below is implemented (verified 2026-10-12: branch `main`,
  single commit `af89144`, `DefaultTemplate/` and `knowledge Docs/` untracked).
- **Session 13 (DECISION_LOG §29) — supersedes the branch/approval notes in this bullet:** D-07 v7
  Approved. Branches `develop` + `feature/phase-0-gate-N` (local only); staged list + message
  shown before each commit; `.gitattributes` (`* -text`) + `*.user` in step 0.1; hash check vs
  the R1 backup manifest; new files LF; `/code-review` + `/ponytail-review` before every gate
  report. Step 0.1 and Gate 1 authorized (stop at the Gate 1 report). Steps 0.2–0.4 updated
  below.
- Steps run in order. **One commit per step.** Branch: `phase-0/baseline` (Approved, D-06).
  **M14 (pending):** D-07 (Proposed, v6 — Session 7) would replace this with gate branches
  (`develop` from `main`, `feature/phase-0-gate-1` from `develop`, local only), review-before-commit
  (exact staged tree + message approved before each commit), byte-for-byte `.gitattributes` and
  pre-/post-commit inventory checks (B1–B6 / C1–C4) in step 0.1, records (step 0.1's records in
  step 0.2) and backup rules; T1-EOL (Pending) needed before 0.2. Until D-07 is approved, nothing
  below changes; the planned edits are listed in DECISION_LOG §21 ("Documentation impact list").
  Session 8: v6.1 correction draft for T1/T4 recorded in DECISION_LOG §22 — not applied, awaiting
  the user's decision. Session 9: final F2/F4 wording in DECISION_LOG §23 — still not applied.
  Session 10: Variant A/B, received-files copy (R1/R2/R3) and `CURRENT.txt` rule proposed in
  DECISION_LOG §24 — not approved, not applied. Session 11: Variant A review findings (B4, G1–G6,
  M1–M9, R2 failure) in DECISION_LOG §25 — not applied; independent verified backup of the
  received folders at `H:/Internship Project/DLMS-Backups/received_20261013-152404/` (§26).
  Session 12: approval (A) — v6.1 applied to D-07 (Variant A + G1, G2, M1, M9; still Proposed);
  approval (D) — `<backup-root>` = `H:/Internship Project/DLMS-Backups/` (DECISION_LOG §28).
  This section is still unchanged until approval (B).
- **Stop at each gate.** Do not start the next gate until the user explicitly approves.
  - **Gate 1:** 0.1–0.5 · **Gate 2:** 0.6–0.8 · **Gate 3:** 0.9–0.12
- **Current authorization (M6 C, confirmed strictly M8 §4):** once the user approves the
  documentation diff, only **step 0.1** may run; verify baseline, branch/status and repository
  state; then stop and report. Steps 0.2–0.5 need explicit review and approval.
  **Update 2026-10-13 (DECISION_LOG §18):** the documentation was approved, but step 0.1 now
  requires a **separate explicit approval**; it has not run.
- Stop earlier if a destructive operation, an unresolved architectural decision or a failed
  acceptance test needs a decision. At each gate report commit hashes, changed files,
  build/test results, unresolved issues and the next proposed action.
- No migration reset, destructive DB operation or `AspNetUserClaims` cleanup without separate
  explicit approval.
- A step is checked `[x]` only with evidence: commit hash + build/test output recorded in
  `docs/SESSION_HANDOFF.md`. Use `[~]` for implemented-but-unverified.
- Steps that depend on a Pending/Proposed decision must not start until the decision is approved
  (see "Requires" column).
- Never touch the existing `DefaultTemplate` database or any non-test DB (D-40).
- **M9 (Session 4):** new requirements FR-M9-01..06 (`docs/requirements.md`) were reconciled
  into this plan without moving implementation earlier. Only FR-M9-05 (0.9) and FR-M9-06 (0.3,
  0.8) touch Phase 0; borrowing (FR-M9-01/02), keyset pagination (FR-M9-03) and AJAX loading
  (FR-M9-04) belong to later phases (tests I/J/K in `requirements.md`). Step 0.3 is **blocked**
  until Q-M9-9 (D-47) is answered.
- **M10 (Session 5):** FR-M9-01/02 are superseded by the configurable borrowing-policy system
  FR-M10-01 (D-48, tests P1–P21; revised M11, M12) — Lending phase (Phase 3), after Members (Phase 2).
  **No Phase 0 step, test or file changes.** Only optional Phase 0 touch-point: an explicit
  `ReadCommitted` level in `ExecuteInTransactionAsync` (0.8, Proposed — otherwise Phase 3).
  Phase 0 contracts it relies on, already planned: typed UoW
  repositories (0.7–0.8), `ExecuteInTransactionAsync` + `ConcurrencyConflictException`
  translation (D-14, 0.7–0.8), audit interceptor + `IAuditable` (0.11),
  DB permissions (D-37, 0.9). `RowVersion` columns and the member-row lock come with Phase 3.
- **M12 (Session 5) safety check:** the M9–M12 additions authorize **no** Phase 0 action — no
  rename, package, migration, database, branch, commit or implementation. The policy table,
  its seed migration and every P-test belong to Phase 3 and need their own approvals. Only
  step 0.1 awaits approval.

## Gate 1 — Baseline, docs, structure, runtime, test harness

| # | Step | Requires | Main files | Done when |
|---|---|---|---|---|
| [x] 0.1 | **Verified (Session 14):** commit `1fdbe5e` (tree `b48a82b6…a3b7`, 151/151 SHA-256 match); post-commit `git status` = only `CLAUDE.md`, `docs/` untracked; 29 CRLF files stored `i/crlf`. **Session 13 (D-07 v7):** branches `develop` from `main`, `feature/phase-0-gate-1` from `develop`; add root `.gitattributes` (`* -text`, `/.gitignore text`, `/LICENSE text`); verify committed files = R1 manifest minus `bin/`/`obj/`/`*.user` (151) with matching SHA-256. _Original text:_ Create branch `phase-0/baseline` from `main`. Commit the template and the original requirements folder **as received** (baseline). Add `*.user` to `.gitignore` — this intentionally excludes `DefaultTemplate/DefaultTemplate/DefaultTemplate.csproj.user` (IDE-local). Session docs (incl. the M9-seeded `docs/requirements.md`, `docs/business-rules.md`) are **not** part of this commit. | D-06 (Approved), user approval of the docs diff (Session 2–5, incl. M9–M12 — given 2026-10-13, §18) **and** a separate explicit approval of step 0.1 (not yet given) | `DefaultTemplate/**`, `knowledge Docs/**`, `.gitignore` | Commit exists; `git status` shows only the session docs untracked |
| [x] 0.2 | **Verified (Session 14):** commit `9cf511a` — archive moved (SHA-256 unchanged), `README.md`, `architecture.md`, `diagrams/template-architecture.*`, `CLAUDE.md` paths (DECISION_LOG §30). **Slimmed (Session 13):** commit session docs + `requirements.md`/`business-rules.md`; `git mv` archive to `docs/archive/` (SHA-256 unchanged); `README.md`; `architecture.md` with an `archify` dependency diagram from real code; update `CLAUDE.md` paths. `database.md`/`development.md`/`status.md` only when they have content; archive FR/BR traceability at each phase start. _Original text:_ Documentation. Commit the reviewed session docs (`CLAUDE.md`, `docs/SESSION_HANDOFF.md`, `PROJECT_DECISIONS.md`, `PHASE_0_PLAN.md`, `DECISION_LOG.md`). Also commit `docs/requirements.md` + `docs/business-rules.md` (seeded in M9) and **extend** them with the archive FRs/BRs (every FR tagged; FR-M9 ID mapping kept). `git mv` archive to `docs/archive/1st discussion.md`; create `README.md`, `architecture.md` (incl. Mandatory Engineering Standards, dependency diagram, UoW/repository contracts), `database.md`, `development.md`, `status.md`; update doc paths in `CLAUDE.md`. One owning file per fact, no duplication. | D-05 (Approved) | `docs/**`, `README.md`, `CLAUDE.md` | Archive SHA-256 unchanged (`6826f819…a86b`); every FR/BR carries a status tag; no assumption marked Confirmed |
| [x] 0.3 | **Verified (Session 14):** commit `3c8d98a` — `src/CseLibrary.{Web,Services,Data,Core}`, `CseLibrary.sln` at root, namespaces, Dockerfile, `_Layout` CSS bundle name, `global.json` (10.0.100, `latestFeature`), local `dotnet-ef` 10.0.12; `dotnet build` 0 errors (2 × known NU1903). `tests/` created in 0.5. Migration list check deferred to 0.5 (test DB only). UI brand text "DefaultTemplate" left (visible change, not in 0.3) (DECISION_LOG §31). Rename and restructure — **no behaviour change.** `src/<Prefix>.{Core,Data,Services,Web}`, `tests/<Prefix>.Tests`, `<Prefix>.sln` at root, namespaces; keep `UserSecretsId`; `<Prefix>` = `CseLibrary` (D-41) or `LibraryManagement` (M9) per Q-M9-9. No extra projects for Domain/Entities/Repository/Service (namespaces only, D-47 Proposed). Migration files keep schema and IDs; namespaces updated. Rename checklist (Dockerfile, launchSettings, `dotnet ef` paths, …) in D-47. `global.json` + `.config/dotnet-tools.json` (local `dotnet-ef`) only if confirmed (D-41 open item). | D-41 (Approved); D-47 Rejected → `CseLibrary` (Session 13); `global.json` + local `dotnet-ef` Approved | all projects | `dotnet build` succeeds |
| [x] 0.4 | **Verified (Session 14):** commit `468a82c` — connection strings + net10.0, build 0 errors, no pending model changes. App-start check **moved to 0.5** and passes there (test DB, in-memory seed password — user sets no secret; DECISION_LOG §33). **First, before any run:** point the dev connection string **and the base `appsettings.json`** (Session 13) at `CseLibrary` (location per D-40 open item) and verify read-only — incl. `dotnet user-secrets list` — that nothing resolves to `DefaultTemplate` (startup runs `MigrateAsync()`). Then upgrade net8.0 → net10.0; EF Core / Identity / Serilog packages to compatible versions; Dockerfile base images 10.0. Report any compatibility problem before working around it. | D-42 (Approved, conditional), D-40 (Approved) | 4 × `*.csproj`, `Dockerfile`, `appsettings.Development.json` | Build 0 errors; resolved connection string shown to be `CseLibrary`; app starts against the **new** `CseLibrary` DB |
| [x] 0.5 | **Verified (Session 14):** commit `22d27ae` — `tests/CseLibrary.Tests` (xUnit 2.9.3, Mvc.Testing 10.0.12), `TestDatabase` guard, shared `AppFactory`, `SqlCommandCounter`, 15 tests green twice, no test DB left behind (DECISION_LOG §34). Test harness + characterization tests **before** refactoring. xUnit project, `TestDatabase` fixture (LocalDB `CseLibrary_Test_{guid}`, strict name guard checked before migrate **and** before cleanup, drops only the name it generated in this run — D-40), `WebApplicationFactory` (`public partial class Program`), SQL command counter. Characterize current behaviour: login, GeneralConfig CRUD, Users list, permission grant/revoke (documents today's stale-claim defect). | D-43 (Approved), 0.4 | `tests/<Prefix>.Tests/**` (prefix per 0.3), `Program.cs` | `dotnet test` green; DB-name guard test passes; no non-test DB touched |

**⛔ Gate 1 review — stop and wait for user approval.** _Session 14: reported; review findings in DECISION_LOG §35; awaiting the user's fix choice and approval (PROJECT_DECISIONS §9 "Session 14")._
_Session 15: findings fixed in correction commit `8504115` (DECISION_LOG §37); build 0 errors, tests
16/16; awaiting Gate 1 approval (merge → `develop`)._

## Gate 2 — Mapping, contracts, Repository + UoW

| # | Step | Requires | Main files | Done when |
|---|---|---|---|---|
| [ ] 0.6 | Remove AutoMapper; explicit mapping. | D-17 (Approved) | delete `Mapping/MappingProfile.cs`; 4 services; `Services/DependencyInjection.cs`; csproj | 0.5 tests green; no NU1903; no `AutoMapper`/`IMapper` reference |
| [ ] 0.7 | Core contracts: `Result`/`Result<T>`, `ErrorCodes`, `PageRequest`/`PagedResult<T>` (max 100), `ConcurrencyConflictException`, `DuplicateKeyException`, slim `IRepository<T>`, feature repository interfaces (incl. read-only `IUserReadRepository`), `IUnitOfWork` with **typed repository properties** + `SaveChangesAsync` + `ExecuteInTransactionAsync`. Exact signatures shown for review. Paging contract must not preclude a later keyset contract (FR-M9-03, D-15 M9 extension); no keyset implementation in Phase 0. | D-12, D-13 (Approved); D-15 details, D-34 `Result` (Proposed) | `Core/Results/*`, `Core/Paging/*`, `Core/Exceptions/*`, `Core/Interfaces/*` | Unit tests for `Result`, page-size clamp |
| [ ] 0.8 | Data implementation + service refactor. Repositories (projection reads, tracked `GetForUpdateAsync`), UoW (lazily created typed repos over one scoped DbContext — valid only while repos need nothing but the DbContext, D-12; execution-strategy transactions per D-14, nested calls rejected, rollback + tracker clear on failed `Result`, exception translation; no `Repository<T>()`, no `IDisposable`). Services inject only `IUnitOfWork`. Fix Q1–Q9 (D-16). Users index paging UI (offset count-and-page, server-rendered — Q-M9-8; no AJAX in Phase 0). Per atomic use case: duplicate-operation risk, chosen protection (constraint / idempotency / other verified), feasible tests, remaining limitations (D-14 rule 3) — presented at Gate 2. Remove Services → Data/EF/`Microsoft.AspNetCore.App` references; use `Microsoft.Extensions.Identity.Core` (D-10). | D-10, D-12, D-13, D-14, D-15 (Approved); D-16 | `Data/Repositories/*`, `Data/UnitOfWork.cs`, `Data/DependencyInjection.cs`, 4 services, `Services.csproj`, Users views/controller | Tests D1–D4, E1–E4, F1–F4, G1 + 0.5 tests green |

**⛔ Gate 2 review — stop and wait for user approval.**

## Gate 3 — Security, errors, audit, verification

| # | Step | Requires | Main files | Done when |
|---|---|---|---|---|
| [ ] 0.9 | Security baseline: configurable `IdentityOptions`, lockout on, fallback policy, **class-level `[Authorize]` on every controller (FR-M9-05; inventory in D-31 M9 extension: today no controller has it, Home actions and `Account.Logout` are anonymous)**, explicit `[AllowAnonymous]` allowlist, global antiforgery, cookie settings, deactivation (`IsActive` + security stamp + `CanSignInAsync`). Database-backed permission authorization (D-37): no permission claims in the cookie, login writes nothing to `AspNetUserClaims`, handler checks `IsActive` + role membership + permissions per request with request-local cache. Cleanup of existing claim rows only after separate approval. | D-31 values + M9 approach, D-32 mechanism (Proposed); Q-M9-10; D-37 (Approved in principle) | `Web/Identity/*`, `Web/Authorization/*`, all `Controllers/*`, `Program.cs`, `appsettings.json` | Tests A1–A11 |
| [ ] 0.10 | Error handling and logging: exception handler, status-code pages, Error view with Reference ID only, Serilog TraceId in output, verify enricher config, dev SQL logging. | D-34 | `HomeController`/`ErrorController`, `Views/Shared/Error.cshtml`, `StatusCode.cshtml`, `appsettings*.json` | Tests B1–B2 |
| [ ] 0.11a | **HiLo demonstration (no migration change).** In a uniquely named disposable LocalDB database `CseLibrary_Test_{guid}` (D-40 guard; never the `CseLibrary` dev DB — M8 §2): HiLo id generation, audit rows carrying the new `EntityId`, rollback behavior, `EnableRetryOnFailure` compatibility. Produce the migration + `dotnet ef migrations script` diff. **Stop for approval** (D-29, D-30). | D-29 (conditional) | test project, spike migration (not applied to any non-test DB) | Demonstration tests green; diff shown; user approval recorded |
| [ ] 0.11 | Audit: `AuditLog` entity + config + indexes, `IAuditable`, `.Audited()`/`.NotAudited()` property classification (D-23), `ICurrentUser` (Web impl), `TimeProvider`, `AuditSaveChangesInterceptor`, `ISecurityAuditService` events, key strategy per approved D-29, migration per approved D-30. | D-21–D-27, **D-29/D-30 approval from 0.11a** | `Core/Entities/AuditLog.cs`, `Core/Interfaces/*`, `Data/Interceptors/*`, `Data/Configurations/*`, `Services/Security/*`, `Web/Infrastructure/HttpCurrentUser.cs`, `Migrations/*` | Tests C1–C7 |
| [ ] 0.12 | Manual smoke test (admin login, create/deactivate user, lockout, error page, audit row) + docs: `status.md`, `SESSION_HANDOFF.md`, `PROJECT_DECISIONS.md`, `DECISION_LOG.md`. | 0.9–0.11 | `docs/**` | Smoke results recorded; Phase 0 exit criteria met |

**⛔ Gate 3 review — stop and wait for user approval before Phase 1.**

## Acceptance tests

| ID | Step | Test |
|---|---|---|
| A1 | 0.9 | Unauthenticated request to a protected route → redirect to Login |
| A2 | 0.9 | Authenticated user without permission → AccessDenied/403 |
| A3 | 0.9 | N consecutive wrong passwords (configured, default 5) → locked out |
| A4 | 0.9 | Deactivated user with an existing cookie: permission-protected route denied on the next request; signed out after the stamp validation interval (advance `TimeProvider`) |
| A5 | 0.9 | POST without antiforgery token → 400 |
| A6 | 0.9 | Reflection: no anonymous endpoint except explicit `[AllowAnonymous]` allowlist |
| A7 | 0.9 | Existing cookie: permission revoked → next request denied; permission granted → next request allowed |
| A8 | 0.9 | Existing cookie: user removed from role → next request denied |
| A9 | 0.9 | Login writes no rows to `AspNetUserClaims` |
| A10 | 0.9 | Reflection (FR-M9-05): every controller class carries a class-level `[Authorize]` (or derived attribute); every action is either covered by it or on the reviewed `[AllowAnonymous]` allowlist; every non-allowlisted action of a feature controller also has a `[HasPermission]` (authentication ≠ permission) unless listed as "authenticated-only" (e.g. `ChangePassword`, `Logout`) |
| A11 | 0.9 | Fallback safety net: an endpoint without any authorization metadata (test-only controller) → redirect to Login when unauthenticated |
| B1 | 0.10 | Unexpected exception → page shows Reference ID, no stack trace; same ID in log |
| B2 | 0.10 | 404 → friendly status page |
| C1 | 0.11 | Create/update/delete of an `IAuditable` entity writes audit rows; Created row carries the new `EntityId` (depends on D-29); update lists only changed audited fields |
| C2 | 0.11 | Rolled-back transaction leaves no audit row |
| C3 | 0.11 | Identity changes (user/role) create no automatic entity audit rows |
| C4 | 0.11 | Role-permission change, user-role change, deactivation write security events in the same transaction |
| C5 | 0.11 | Failed login writes a security event (known user) |
| C6 | 0.11 | Every property of every `IAuditable` entity is classified `.Audited()`/`.NotAudited()` (missing → fail); a Password/Hash/Token/Secret/Stamp/Credential-like property classified `.Audited()` → fail |
| C7 | 0.11 | No Identity entity implements `IAuditable` |
| D1 | 0.8 | Users list: SQL command count is the same for 5 and 50 users (no N+1); roles correct per user; paging boundaries correct |
| D2 | 0.8 | Requested page size > max is clamped |
| D3 | 0.8 | Permission lookup for a user = 1 SQL command |
| D4 | 0.8 | Users list SQL selects no `PasswordHash`/`SecurityStamp` |
| E1 | 0.8 | Architecture: no controller constructor takes `DbContext`, `IUnitOfWork` or a repository; Services assembly references no Data, EF Core or ASP.NET Core assembly; Core references no EF Core/ASP.NET Core; `IUnitOfWork` has no generic or `Type`-parameter repository accessor; `IRepository<T>` exposes no `IQueryable`, `GetAllAsync`, `FindAsync`, `Update`; repository sources contain no `SaveChanges` call. Checks inspect referenced **assemblies** and framework references, not namespaces (`UserManager` is in `Microsoft.Extensions.Identity.Core` but namespace `Microsoft.AspNetCore.Identity`) |
| E2 | 0.8 | In one DI scope, the UoW's `ApplicationDbContext` and the Identity `UserStore`/`RoleStore` context are the same instance |
| E3 | 0.8 | No Service constructor takes a repository interface (direct repository injection forbidden) |
| E4 | 0.8 | Project structure (FR-M9-06, D-47): solution contains exactly the four `src/<Prefix>.*` projects + `tests/<Prefix>.Tests`; project references are Web → Services, Core, Data (composition root only); Services → Core; Data → Core; Core → no project; no `Default.*`/`DefaultTemplate` name remains in project, assembly or namespace names |
| F1 | 0.8 | Role update failing midway (failed `IdentityResult` or thrown exception) leaves no partial change |
| F2 | 0.8 | Simulated transient failure (test execution strategy): delegate re-runs with freshly loaded state, commits once, no duplicate rows |
| F3 | 0.8 | Failed `Result` → rollback; change tracker empty; a later `SaveChangesAsync` in the same scope persists nothing from the failed attempt |
| F4 | 0.8 | Nested `ExecuteInTransactionAsync` → `InvalidOperationException` |
| G1 | 0.8 | Concurrency conflict → `ConcurrencyConflictException`; duplicate key → `DuplicateKeyException` |
| H1 | 0.5 | DB guard: fixture refuses to migrate or clean up a database whose name does not start with `CseLibrary_Test_`; cleanup targets only the name generated in that run |

## Phase 0 exit criteria

- [ ] Build: 0 errors, 0 vulnerability warnings, target net10.0 (D-42).
- [ ] No `AutoMapper`, `IQueryable` exposure (`Query()`), `GetAllAsync`/`FindAsync`, or
      `Repository<T>()` accessor remains.
- [ ] All acceptance tests green, run only against `CseLibrary_Test_*` databases.
- [ ] Manual smoke test recorded.
- [ ] Archive byte-identical; docs separate verified from unverified; handoff names one next task
      (Phase 1: Catalog).

## Known unverified risks to check during Phase 0

- Serilog `CreateBootstrapLogger` with `WebApplicationFactory` ("logger already frozen").
- `WithMachineName`/`WithThreadId` enrichers configured without their packages.
- HiLo + `EnableRetryOnFailure` interaction (step 0.11a).
- Ambiguous commit (connection lost during commit): simulating it reliably in a test is hard
  (e.g. a transaction interceptor throwing after `TransactionCommitted`); each atomic use case
  documents its risk, protection, tests and remaining limitations (D-14 rule 3) — never reported
  as fully proven.
- ~~Permission claims lost on security-stamp refresh (D-33)~~ — corrected in Session 2: the real
  defect is that revoked permissions stay in the cookie (D-37).
