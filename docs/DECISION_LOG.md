# Decision Log

Chronological history of discussions, proposals, approvals and supersessions. Current binding
state lives in `docs/PROJECT_DECISIONS.md`; this file explains how it got there. Append new
entries at the bottom; never rewrite past entries (add a correcting entry instead).

## Before Session 1 — original requirements discussion (archive)

Source: `knowledge Docs/Requirement/1st discussion.md` (ChatGPT conversation export, undated).
- Module breakdown → SRS v1.0 ("Proposed Requirements — pending approval by the department";
  §1.3 deferred the member portal to a later release).
- §8 "Here is my Ans": user chose ASP.NET Core MVC + SQL Server, member access **Both**
  (staff + portal), individual copies, **no fine initially**, reminders 3 days before + due date +
  overdue. → D-01, D-02. Supersedes SRS v1.0 §1.3 on the portal.
- §9 "Implementation-Ready Specification v1.1": BR-01..BR-18, schema, single-project MVC
  structure, outbox suggestion. Treated as **Proposed**; the single-project structure is
  superseded by D-03 (keep the template).
- Known defects of the export: both ERD diagrams survived only as CSS text; unrelated ITESO
  library image link at lines 7–9.

## Session 1 — 2026-10-12

### 1. Kickoff review (user M1)
- Assistant explored the repo: one requirements file; untracked `DefaultTemplate/` (.NET 8,
  4-project Onion, Identity, permission auth, generic Repository/UoW, AutoMapper, Serilog) not
  mentioned in the docs.
- Reported conflicts/gaps: single-project vs template; `IsActive` vs `IsDeleted`; portal
  needs email before it (phase order); no lost-book closure state; MemberCode/InstitutionalId
  duplication; borrowing-limit race; "queue email after commit" can lose notifications;
  template security gaps (no lockout, weak password, `IsActive` only checked at login,
  AutoMapper advisory, no tests).
- Proposed a docs structure (CLAUDE.md, SESSION_HANDOFF, requirements, architecture, …).

### 2. "Proceed carefully" + Mandatory Engineering Standards (user M2)
- **Approved:** docs structure in principle; keep `DefaultTemplate` (D-03); Bangla
  communication + English docs (D-04); layering, error-handling, logging/audit/business-history
  separation, central audit table first (D-10, D-20, D-34 principles).
- Assistant built the template (`dotnet build`: success, 2 × NU1903 AutoMapper warnings) and
  found: .NET 8 support ends 10 Nov 2026; only .NET 10 runtime installed; no authorization
  fallback policy; generic repo leaks `IQueryable`; `UnitOfWork.Dispose` disposes the DI-owned
  DbContext.
- Assistant **proposed** "Option A": services use `ApplicationDbContext` directly, migrate the
  template services and delete Repository/UoW. Also proposed .NET 10 upgrade, `CseLibrary.*`
  rename, AutoMapper removal, `Result` + error codes, central audit via interceptor.
- Assistant **revised its own earlier proposal** for lending concurrency: `ExecuteUpdateAsync`
  conditional updates replaced by rowversion + filtered unique index + member `UPDLOCK`,
  because `ExecuteUpdate` bypasses the change tracker and the audit interceptor (D-35).

### 3. Revised architecture (user M3, items 1–8)
- **Rejected Option A** (D-11). Strict Repository + Unit of Work made mandatory (D-12):
  no EF queries in services, no DbContext/repositories in controllers, UoW coordinates
  persistence/transactions.
- **Approved:** central `AuditLogs` via `SaveChangesInterceptor` (D-21); no automatic audit of
  Identity tables, explicit allowlist preferred (D-22); explicit security events (D-24
  principle); entity-specific history only with a concrete requirement (D-26); remove
  AutoMapper, no replacement library (D-17); security baseline with configurable values (D-31);
  isolated test database, never touch the existing DB (D-40); data-fetching / projection /
  pagination / no-N+1 rule (D-15).
- Assistant reviewed template queries (Q1–Q9, D-16) and **proposed**: repositories injected
  directly into services with UoW limited to save + transactions; slim `IRepository<T>`;
  execution-strategy-aware transactions; exception translation; `IAuditable` marker; HiLo keys;
  `UserClaimsPrincipalFactory`; Services project without EF/Data references; Phase 0 steps
  0.1–0.12 with tests A–G; three possible review groupings.

### 4. Session handoff request (user M4)
- **Supersedes** the "inject repositories directly into services" proposal: user prefers
  **typed repository properties on `IUnitOfWork`**, services inject only the UoW, UoW must not
  become a generic service locator (D-12).
- **Approved** review gates: Gate 1 = 0.1–0.5, Gate 2 = 0.6–0.8, Gate 3 = 0.9–0.12 (D-06).
- **HiLo kept Pending** until a dedicated review of rationale, alternatives and migration impact
  (D-29).
- Session docs created (uncommitted, for user review): `CLAUDE.md`, `docs/SESSION_HANDOFF.md`,
  `docs/PROJECT_DECISIONS.md`, `docs/PHASE_0_PLAN.md`, this file. `docs/decisions.md` and
  `docs/open-questions.md` from the earlier proposal are superseded by `PROJECT_DECISIONS.md`.

## Session 2 — 2026-10-12

### 5. Session-doc review
- Assistant verified repo state against the handoff (branch `main`, `af89144`, four untracked
  items, archive SHA-256 unchanged, all projects net8.0, only .NET 10 SDK/runtime installed).
- Reported doc inconsistencies: (1) **safety** — `Program.cs` runs `MigrateAsync()` at startup
  and `appsettings.json` targets `DefaultTemplate` on `.\SQLEXPRESS`, but step 0.4 did not list a
  connection-string change; `UserSecretsId` may override it; (2) Gate 1 also depended on Proposed
  D-05/D-40 details/D-43; (3) open-decision lists omitted Proposed items used in Gates 2–3;
  (4) plan step 0.8 presented lazy repository creation as decided; (5) Pending/Proposed wording
  mismatch; (6) `global.json`/`dotnet-ef` coupled to the rename; (7) `.csproj.user` vs "as
  received".

### 6. Revised plan review (user M5)
- Re-confirmed UoW as the single repository access point with rules 1–10 (typed properties, one
  scoped context, no SaveChanges in repositories, no service locator, no unrestricted
  `Query`/`GetAllAsync`/`FindAsync`/`Update`, projection/paging, transactions only when needed,
  careful retry/Identity review). Layer boundaries, DB safety, audit principles, claims and
  query-efficiency requirements restated. HiLo not to be adopted automatically.
- Assistant produced a decision report (§1–§7). Code-verified findings: the permission handler
  falls back to the DB only when the cookie lacks the claim, so **revocations do not take effect**
  while grants do; the earlier "claims lost on stamp refresh" risk (D-33) was **wrong** and is
  corrected. Proposed: lazy repository creation; HiLo on `IAuditable` entities only;
  `ExecuteInTransactionAsync` design with re-runnable-delegate rules; opt-in
  `.Audited()`/`.NotAudited()` audit fields; database-backed per-request permission check instead
  of cookie claims; acceptance-test changes.

### 7. Decision-report approval (user M6)
- **Approved:** UoW typed repository properties (D-12); layer boundaries + compile-time
  enforcement (D-10); repository contract (D-13, signatures at Gate 2); audit-field opt-in policy
  (D-23) and `IAuditable` mechanism (D-22); acceptance tests incl. D1/D4, E1/E2, F1–F4,
  A4/A7–A9, C1/C6/C7 plus a new direct-repository-injection test (E3); branch `phase-0/baseline`
  (D-06); `src/CseLibrary.*` + `tests/CseLibrary.Tests` (D-41); .NET 10 LTS subject to
  compatibility checks (D-42); documentation structure without duplication (D-05); `CseLibrary`
  dev DB + `CseLibrary_Test_{guid}` with strict guards (D-40); test strategy (D-43).
- **Approved conditionally:** lazy repository creation only while repositories need nothing but
  the DbContext; propose a DI-compatible factory if that changes. User correction: omitting DI
  registration is **not** compile-time enforcement → test E3.
- **Approved in principle, proof required:** transaction/retry design (D-14); DB-backed
  permission authorization (D-37, new) — complete only when A4/A7–A9 pass. D-33 marked
  Superseded by D-37 once the replacement design was documented (this entry).
- **HiLo (D-29):** preferred candidate only; isolated demonstration + migration/script diff +
  approval required before any migration change (new step 0.11a).
- **Restrictions:** no destructive DB operation, migration reset or `AspNetUserClaims` cleanup
  without separate approval; documentation changes may proceed now; after the user approves the
  documentation diff, only step 0.1 may run — no blanket Phase 0 approval.

### 8. Docs update and session close
- Assistant recorded M5/M6 in `PROJECT_DECISIONS.md`, `PHASE_0_PLAN.md` (new step 0.11a, tests
  A7–A9, C7, D4, E2, E3, F2–F4), this file and `SESSION_HANDOFF.md`. No code, commit, branch or
  database change was made in Session 2.
- Questions raised and **not yet answered** (listed in `SESSION_HANDOFF.md` §6): docs-diff
  approval; D-33 supersession timing; HiLo demo DB (`CseLibrary_Test_*` vs dev `CseLibrary`);
  per-use-case ambiguous-commit guard instead of a general proof; D-41 tooling; D-40 dev
  connection-string location.
- Session closed at the user's request (conversation too long); continue from the handoff.

## Session 3 — 2026-10-12

### 9. M6 re-sent (user M7)
- The user re-sent the M6 approval text (A1–A8, B, C) unchanged and asked that it be recorded
  consistently in the four session docs. Assistant re-checked each point against
  `PROJECT_DECISIONS.md`, `PHASE_0_PLAN.md`, this file and `SESSION_HANDOFF.md`: **all points were
  already recorded in Session 2; no new decision and no status change.** Repo state re-verified
  (branch `main`, `af89144`, archive SHA-256 unchanged, `phase-0/baseline` not created).
- Points reported back to the user instead of assumed (see `SESSION_HANDOFF.md` §6):
  1. D-33 is already marked Superseded in the same diff that documents D-37 — confirm this meets
     the "only after documenting the replacement design" condition.
  2. HiLo demo DB: user wording "isolated new `CseLibrary` database" vs plan's auto-dropped
     `CseLibrary_Test_*` LocalDB database.
  3. "Ambiguous commits cannot cause duplicate business operations": proposed as a documented +
     tested guard per atomic use case, not a general proof.
  4. "Stop at the relevant review gate" after step 0.1: docs read this strictly as **stop after
     0.1** (not continue to the end of Gate 1).
  5. New technical note for test E1: `UserManager`/`RoleManager` live in assembly
     `Microsoft.Extensions.Identity.Core` but namespace `Microsoft.AspNetCore.Identity`, so the
     "no ASP.NET Core" check must inspect **referenced assemblies / framework references**, not
     namespaces. Today `Default.service.csproj` references `Microsoft.AspNetCore.App` and
     `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (to be removed in 0.8).

### 10. Answers to the open questions (user M8)
- **D-33 → D-37:** timing accepted, provided the same change documents the replacement design,
  its security rationale and acceptance tests. D-37 gained an explicit "Security rationale"
  paragraph; D-33 kept as history, marked Superseded.
- **HiLo demo DB (D-29):** uniquely named disposable LocalDB `CseLibrary_Test_{guid}`; never the
  dev DB for disposable tests. Guard validates the name before migration **and** cleanup; cleanup
  only targets the DB created by that run (D-40; new acceptance test H1). Migration reset or
  consequential schema change still needs the script shown + separate approval.
- **Ambiguous commits (D-14 rule 3):** per-use-case approach approved in principle. Each atomic
  use case documents its risk and a verified protection; a unique constraint is not assumed to
  cover every operation; feasible tests; remaining limitations reported, never "fully proven".
  Detailed assessment at Gate 2.
- **Workflow:** strict — show docs changes → after approval run step 0.1 only → verify
  baseline/branch/status → stop and report. 0.2–0.5 only after explicit approval.
- User asked for the complete contents / a reviewable diff of the four docs before step 0.1;
  no commit, application change or database change during the review.

### 11. Docs update and session close
- Assistant recorded M8 in `PROJECT_DECISIONS.md` (D-06, D-14 rule 3, D-29, D-33, D-37 security
  rationale, D-40 guard details, §9 table), `PHASE_0_PLAN.md` (authorization rule, 0.5, 0.8,
  0.11a, new test H1, E1 assembly note, known risks), this file and `SESSION_HANDOFF.md`.
- Showed the user the Session 3 diff (4 files, +103/−29, against a snapshot taken at session
  start) and links to the full files; Session 2 content has no earlier baseline (files untracked).
- **Not yet received:** the user's approval of the docs. No code, commit, branch or database
  change was made in Session 3.
- Session closed at the user's request (conversation too long); continue from the handoff.

## Session 4 — 2026-10-12

### 12. New requirements and documentation consolidation (user M9)
- **Request:** consolidate six new functional requirements into the docs, reconcile them with
  existing decisions, add acceptance tests, and stop with a review package. Documentation only:
  no code, project, package, database, branch or commit change; no step 0.1.
- **New requirements** (owned by new `docs/requirements.md`, numbered `FR-M9-01..06` because the
  archive SRS already uses `FR-01..FR-14`): configurable borrowing limit (initial 3);
  outstanding-loan restriction on new borrowing requests (server-side); server-side pagination
  preferring keyset; jQuery AJAX incremental loading; explicit controller-level authorization;
  library-specific project naming. Rules in new `docs/business-rules.md` (`BR-M9-01..08`).
- **Why new files now:** M9 asked for the policy in the requirements and business-rules documents;
  they were planned for step 0.2 (D-05). Seeded with M9 content only; 0.2 still consolidates the
  archive into them (Q-M9-11 asks the user to confirm this).
- **Reconciled into existing decisions (no new ID, status unchanged):** D-15 gained a Proposed
  "M9 extension" (keyset design, consistency model, limitations, exceptions — Users list stays
  offset count-and-page as Approved); D-31 gained a Proposed "M9 extension" with a code-verified
  controller inventory (no class-level `[Authorize]`, no `FallbackPolicy`; `Home.*` and
  `Account.Logout` anonymous today) and the class-level `[Authorize]` + fallback approach;
  D-35 and D-41 got cross-references; D-05 notes the early seeding.
- **New entries:** D-44 borrowing-limit enforcement (Proposed); D-45 outstanding-loan restriction
  (Pending — conflict); D-46 jQuery Load More (Proposed); D-47 project naming (Pending — conflict).
- **Conflicts found and left for the user (not resolved by the assistant):**
  1. FR-M9-02 read literally (any unreturned loan blocks a new request) ⇒ effective limit 1,
     contradicting FR-M9-01 (limit 3) — Q-M9-1.
  2. No member-submitted "borrowing request" exists in the approved scope D-02 / archive (staff
     issue directly; reservations deferred) — Q-M9-2.
  3. M9 name `LibraryManagement.*` vs Approved D-41 `CseLibrary.*`. Structure, layout and
     dependency direction are identical; only the prefix differs. D-41 stays Approved; step 0.3
     blocked until Q-M9-9 is answered.
  4. Archive limit is per member **category** (BR-05, FR-14); M9 says per member — Q-M9-4.
- **Tests added:** Phase 0 — A10 (class-level authorization reflection), A11 (fallback safety
  net), E4 (project structure/references). Later phases (in `requirements.md`) — I1–I10
  (borrowing), J1–J8 (pagination), K1–K4 (AJAX). No existing test renumbered.
- **Preserved unchanged:** D-33 Superseded by D-37 (history kept); D-29 HiLo conditional, demo only
  in `CseLibrary_Test_{guid}` with name validation before migrate and cleanup; D-14 rule 3
  per-use-case ambiguous-commit rule; strict gate workflow (only step 0.1 after approval);
  D-12 Repository + UoW with typed properties; D-10 boundaries; D-21–D-27 audit; D-37; D-40.
- **No approved decision changed status.** Nothing implemented or verified in Session 4.

### 13. Review package and session close
- Assistant showed the M9 review package: changed-file summary, FR → decision → test → gate
  mapping, conflicts/assumptions/open questions (Q-M9-1..11), confirmation that no approved
  decision changed, and repo verification (branch `main`, HEAD `af89144`, `phase-0/baseline`
  absent, `DefaultTemplate/` unmodified, archive SHA-256 unchanged).
- Diff evidence: PROJECT_DECISIONS +190/−0 and DECISION_LOG +43/−0 (pure additions — no existing
  text altered); PHASE_0_PLAN +14/−6; SESSION_HANDOFF +23/−12; CLAUDE.md +3/−2 (doc map); new
  `requirements.md` (126 lines) and `business-rules.md` (35 lines). The patch was generated against
  a Session 4 start snapshot stored in the session scratchpad (**outside the repo, not
  persistent**); this entry is the durable record.
- **Not yet received:** the user's approval of the docs or answers to Q-M9-1..11. No code, commit,
  branch, package or database change was made in Session 4.
- Session closed at the user's request (conversation too long); continue from the handoff.

## Session 5 — 2026-10-12

### 14. Borrowing-policy clarification (user M10)
- **Request:** the user withdrew the M9 A/B/C framing ("which outstanding loans block a
  request") as a misunderstanding. The requirement is a **configurable, extensible borrowing-policy
  system** per member category, set by an authorized Admin/Librarian, not one fixed rule.
  The listed examples (limit, duration/renewals, outstanding-loan block, overdue block,
  per-category rules, activation/changes) are examples, not approved rules. Keep scope
  proportional; no generic rule engine; no reservation/request workflow outside the approved
  scope; documentation only; stop after the diff.
- **What changed (documentation only):**
  - New `FR-M10-01` (`requirements.md`) with tests P1–P15. `FR-M9-01`, `FR-M9-02` and tests
    I1–I10 → **Superseded by FR-M10-01**; original text kept in place, marked.
  - New `BR-M10-01..12` (`business-rules.md`). BR-M9-01, -06, -07, -08 → Superseded;
    BR-M9-05 superseded in part (lost/cancelled counting still open); BR-M9-02..04 stay.
  - New **D-48** borrowing policy system (`PROJECT_DECISIONS.md`): typed per-category
    `BorrowingPolicies` table, pure evaluator, eligibility check vs enforcement, issue
    transaction with member-row lock, concurrency analysis. D-44, D-45 → **Superseded by D-48**
    (text kept).
  - Q-M9-1 superseded; Q-M9-2 answered (i); Q-M9-4 answered in part; new Q-M10-1..8.
- **How the M9 conflict was resolved:** "an unreturned loan blocks another borrowing" is the
  value `MaxActiveLoans = 1`, and "limit 3" is the value 3 of the same per-category setting; the
  overdue variant is a separate boolean. The conflict was between two fixed readings and
  disappears once both are configuration. No value was chosen by the assistant.
- **Recorded as Approved (user's own M10 statements):** rules are per-category configuration
  edited by authorized staff, never hard-coded; no reservation/request workflow (= Q-M9-2 (i));
  no generic rule engine. Everything else in D-48 is **Proposed**; values **Pending** department.
- **Preserved:** archive BR-05 and FR-14 (fully covered; FR-14's non-borrowing settings stay with
  FR-14), BR-03, BR-04, BR-06, BR-16, BR-17; D-02 scope; D-12/D-14/D-35 designs reused unchanged.
  No approved decision changed status.
- **Proportionality choices (Proposed):** three settings only (renewals deferred — not in scope),
  typed columns over key/value or rule engine, no per-member override, no global default (fail
  closed), no future-dated activation, history via `AuditLogs` instead of a history table, no
  policy cache.
- Nothing implemented or verified. No code, project, database, branch or commit change.

### 15. Policy semantics correction and review package (user M11, 2026-10-13)
- **Request:** before approving the docs, correct the M10 semantics and prepare a final review
  package; documentation only; no step 0.1.
- **Corrections made:**
  1. *Independent settings.* M10's claim "any outstanding loan blocks" = `MaxActiveLoans = 1`
     was withdrawn (not generally true for a configurable system). New independent setting
     `BlockIssueWhenAnyActiveLoan` beside `MaxActiveLoans` and `BlockIssueWhenOverdue`; new
     BR-M10-13; evaluation semantics (AND, all reasons) and combination matrix P16 added in
     FR-M10-01. Cross-setting constraints: recommendation "none, show effective behaviour",
     Proposed (Q-M10-9). No default values selected (BR-M10-14).
  2. *Values vs logic.* FR-M10-01 now separates configurable values from evaluation logic, states
     that administrators cannot enter code or expressions (P20), and lists which future changes
     need code, migrations or scope approval. The M10 phrase "a new rule can be added without
     rewriting the lending workflow" was reworded so it does not promise development-free rules.
  3. *D-48 review.* D-48 is now **wholly Proposed**. The three M10 "Approved parts" were
     reclassified as [Confirmed] **requirements** in FR-M10-01 (they are the user's requirements,
     not approvals of the design). Added: architecture assessment table (typed table, pure
     evaluator, missing policy, category change, policy-edit concurrency, member-row lock) and a
     locking section with isolation assumptions (READ COMMITTED, RCSI on/off; no SNAPSHOT),
     single issue entry point, lock order, limitations and tests P17–P19. New Proposed
     dependency on D-14: explicit `ReadCommitted` level (optional at 0.8).
  4. *Decisions now vs later.* §9 now states that only the docs approval is needed before step
     0.1; all Q-M10 questions moved under "can wait until the Lending phase — no answer needed
     now".
- **Preserved:** M10 history (§14 unchanged; D-44/D-45 text kept; Superseded FR/BR text kept;
  withdrawn sentences kept with an M11 note). Archive BR-05/FR-14/BR-17 unchanged. No approved
  decision changed status; nothing marked Approved because it appears in the docs.
- Nothing implemented or verified. No code, project, database, branch or commit change.

### 16. Consistency audit before documentation approval (user M12, 2026-10-13)
- **Request:** focused audit of the six docs (+ `CLAUDE.md`) against the archive: traceability,
  policy semantics, SRS compatibility, concurrency claims, Phase 0 safety, decision consistency;
  documentation-only corrections recorded as M12; no step 0.1.
- **Inspected in full:** `requirements.md`, `business-rules.md`, `PROJECT_DECISIONS.md`,
  `DECISION_LOG.md`, `PHASE_0_PLAN.md`, `SESSION_HANDOFF.md`, `CLAUDE.md`. Archive: targeted
  sections only (FR-06, FR-14, SRS §4 rules, §5.6 tables, §14 open decisions, spec v1.1 BR table,
  roles table, table list, reminder schedule §7.1, phase list), located by search — not read end
  to end.
- **Corrections (this entry corrects §14 and §15 without rewriting them):**
  1. *Archive table name.* §14/§15 said the archive's `LibraryPolicies` mixed per-category rules
     with global settings and proposed `BorrowingPolicies`. Spec v1.1 defines `LibraryPolicies`
     (§4.3) as exactly the per-member-type duration/limit table, and SRS §8 lists the page
     `/Admin/LibraryPolicies`; the claim was wrong. D-48 now keeps the archive name (`LibraryPolicy`, `LibraryPolicies.Manage`).
  2. *"Fully covered" claim.* §14 said BR-05/FR-14 were fully covered. FR-14 also lists
     renewals and fines. FR-M10-01 now has an item-by-item reconciliation table: renewals
     deferred (not discarded), fines not applicable in v1 (D-02), non-borrowing FR-14 settings
     stay with FR-14.
  3. *FR-06 due date.* Made explicit that FR-06 step 5 (librarian sets the due date) stays in
     force and the policy only prefills it. Q-M10-7 no longer recommends "allow earlier only";
     it states that any restriction amends FR-06 and needs approval.
  4. *Policy manager conflict.* Archive roles table: only Admin manages policies; M10 wording:
     Admin/Librarian. Recorded as a conflict in Q-M10-6, BR-M10-10 (not resolved).
  5. *Carried-forward requirement.* FR-M9-02's confirmed "server-side enforcement" is now stated
     explicitly in FR-M10-01 (it was only implied by P10).
  6. *Concurrency overclaims.* D-48 §4 "yes by construction" replaced with "design intent";
     P17 (a) needs a method-level check whose mechanism (and any test package) is not chosen;
     deactivation serializes only if category/`IsActive` are member-row columns (assumption;
     portal-login deactivation is a different row); deadlock retry and lock-wait timeout got a
     test (P21); one verification set everywhere (P12, P14, P17–P19, P21).
  7. *Smaller items.* Interim traceability table in `requirements.md` (moves to `status.md` in
     0.2); due-date time semantics cite the archive's inconsistent sources; D-48 uses D-13's
     `LendingTransactions` property name; seeded policy rows are not audited (D-25) and need no
     HiLo (D-29); D-35 cross-reference; PHASE_0_PLAN 0.1 "Requires" covers M9–M12 and 0.5 uses
     `<Prefix>`; M12 safety note; handoff wording; `CLAUDE.md` doc map.
- **Checked, no change needed:** D-41 (Approved) vs D-47 (Pending) — consistent, D-41 stays until
  Q-M9-9; D-29 conditional status identical in all docs; D-37 "approved in principle" + separate
  cleanup approval identical in all docs; D-33 Superseded history intact; no duplicate decision
  IDs; the three borrowing settings are independent in requirements, rules and D-48; the
  evaluator returns all reasons (P16 matrix re-checked row by row); no default value selected.
- No status changed; nothing marked Approved. Nothing implemented or verified. No code, project,
  database, branch or commit change.

### 17. Final evidence review and five corrections (user M13, 2026-10-13)
- **Review (M13):** re-read the permanent docs and checked the cited archive lines against their
  section headings (archive not read end to end). All 13 M12 corrections present; five defects
  found, two of them introduced in M12. The user approved applying exactly these five
  (documentation only); applying them is not documentation or implementation approval.
- **Corrections (this entry corrects §16 without rewriting it):**
  1. *DF-1 attribution.* `requirements.md` reconciliation row attributed "Due date: determined by
     the configurable lending policy" to spec v1.1; it is from the initial draft business-rules
     table (archive §4, line 123, before the SRS at line 132; spec v1.1 starts at line 1129).
  2. *DF-2 P21.* Refined: victim-attempt rollback (incl. outbox), outcome after retry (one loan or
     a business refusal, never two loans), lock release after timeout, technical failure with
     Reference ID (D-34) rather than a business refusal, retries-exhausted case; states that it
     is a test specification only and the behaviour stays Proposed (D-48 §4), not Verified.
  3. *DF-3 D-48 §1 wording.* "SRS FR-14 screen" → SRS FR-14 ("Library Policy Settings") is a
     requirement; the SRS does not say which table stores those settings. No table mapping added.
  4. *DF-4.* `business-rules.md` borrowing-policy heading now "revised M11, M12".
  5. *DF-5.* `SESSION_HANDOFF.md` §2 verification notes now cover M10–M13 and the full-hash
     archive check in M13.
- No status changed; nothing marked Approved or Verified. Nothing implemented. No code, project,
  package, database, migration, branch or commit change; step 0.1 not executed.

### 18. Documentation package approved (user, 2026-10-13)
- **Approved:** the documentation package in its state after M13 (final gate review reported no
  blocking defect): `CLAUDE.md`, `docs/SESSION_HANDOFF.md`, `docs/PROJECT_DECISIONS.md`,
  `docs/PHASE_0_PLAN.md`, `docs/DECISION_LOG.md`, `docs/requirements.md`,
  `docs/business-rules.md`.
- **Scope — documentation only.** The approval accepts these files as the reviewed record. It
  does **not** change any decision status: Proposed, Pending, conditional and "approved in
  principle" items keep their status, and nothing is Verified. Open questions (incl. Q-M9-11)
  are not treated as answered by it.
- **Step 0.1 is not authorized by this approval.** The user requires a separate explicit approval
  before step 0.1 (stricter than D-06 / M6 C, which let documentation approval unlock step 0.1).
- **Limitations kept:** the original archive has not been read end to end (only cited sections);
  full FR/BR traceability (archive SRS FR-01..14, spec v1.1 BR-01..18, D-02 scope) remains for
  step 0.2; P-tests (and I/J/K tests) are specifications, not implementation evidence.
- No code, project, package, database, migration, branch or commit change; step 0.1 not executed.

### 19. Git branching workflow proposed (user M14, 2026-10-13) — documentation only
- **Request (M14):** adopt a team-style workflow: `main` (stable) / `develop` (integration) /
  `feature/<name>` from `develop` / optional `hotfix/<name>`; PR review into `develop`; release
  review into `main`; feature branches never merge into `main`; protect `main`; branches do not
  isolate databases. Proposal only — no branch, commit, merge, push or step.
- **Assistant's proposal:** new **D-07 (Proposed)**, placed after D-06. Conflict found: D-06
  (Approved) fixes the Phase 0 branch as `phase-0/baseline` from `main`, which step 0.1 creates.
  Options: (A) decide D-07 before step 0.1 — `develop` from `main`, a gate branch from `develop`,
  baseline commit there; (B) keep 0.1 per D-06, adopt D-07 from Phase 1.
- **User direction (M14 follow-up), preferences pending final approval:** Option A; one feature
  branch per review gate; push and GitHub PRs deferred (local-only, local diff review + explicit
  merge approval); `--no-ff` merges, no squash/rebase of shared branches; D-06 status and history
  unchanged — on approval only the branch clause is marked superseded.
- **Revised D-07 (in `PROJECT_DECISIONS.md`)** clarifies: baseline commit = the only commit of
  step 0.1, on `feature/phase-0-gate-1`; `main`/`develop` stay at `af89144` until approved
  merges; one commit per step inside the gate branch; one `--no-ff` merge commit per gate as an
  integration commit (no extra content; conflicts → stop); a gate branch authorizes no step;
  database/migration controls unchanged (shared dev DB + startup `MigrateAsync()` hazard noted).
- **Finding (M14):** after step 0.1, checking out `develop`/`main` before the gate merge removes
  `DefaultTemplate/` and `knowledge Docs/` from the working tree (ignored `bin/`, `obj/`,
  `*.csproj.user` not restored) → D-07 rule: stay on the open gate branch; any other checkout is
  reported and approved first.
- **Read-only check:** `origin` = GitHub `Rokibul-Hassan-Remon/Department-Library-Management-System`;
  local `main` = `origin/main` = `af89144`; no push has ever been authorized.
- **Not yet done (awaiting approval):** D-07 final wording; D-06 supersession line; PHASE_0_PLAN,
  SESSION_HANDOFF §8 and CLAUDE.md changes listed in D-07. Only a "pending" note was added under
  D-06 and in PHASE_0_PLAN. No status changed. Step 0.1 still needs separate explicit approval.
- Session 5 closed after this entry at the user's request (conversation too long); continue from
  `SESSION_HANDOFF.md`.

## Session 6 — 2026-10-13

### 20. D-07 review: revisions v1 → v4 (user, Session 6) — documentation only, D-07 still Proposed
Four review rounds on D-07. Each time the user explicitly said: **no file edits, no Git state
change, no branch, stage, commit, merge, push, and no step 0.1**; D-07 stays Proposed; approving
D-07 is never step 0.1 authorization. All checks were read-only. At the end the user asked to
record the outcome in the docs (this entry; D-07 v4 text placed in `PROJECT_DECISIONS.md`).

**Round 1 — two corrections requested.**
- *Working-tree safety:* do not require approval for every ordinary checkout; instead inspect
  `git status` before switching, identify uncommitted/untracked/ignored/branch-specific files,
  never discard user data, stop and report when a switch could lose data or create ambiguity.
  Document that tracked files from one branch's commit disappear on another branch, and that
  ignored `bin/`, `obj/`, `*.csproj.user` are not restored by checkout. → **Supersedes the §19
  rule** "stay on the open gate branch; any other checkout is reported and approved first"
  (§19 text left unchanged as history).
- *Commit semantics:* exactly one step commit per approved step on its gate branch; a `--no-ff`
  gate merge commit is a separate integration commit, not a step commit, no unrelated changes;
  no squash/rebase/rewrite.
- Assistant raised: "exactly one commit" + "no amend" leaves post-review corrections undefined —
  options (a) separate correction commit, (b) review the diff before committing.

**Round 2 — user chose option (b): review and approve the complete step diff before the step
commit.** Consequences recorded: M8 §4 sequence changes from "run → verify → stop and report
(commit first)" to "complete step → show diff + verification → explicit approval → exactly one
step commit → report hash and results"; this needs a **partial supersession of D-06** (items
(2)–(4) of "Confirmed strictly (M8 §4)", §10 text unchanged). A correction after a commit became
an exception rule (correction commit), not assumed permitted. Migration-diff approval ≠ applying
it to a database (D-29/D-40) added.

**Round 3 — user requested:** baseline format clarified as inventory + integrity check, not a
substitute for content inspection; any file inspectable before approval; commit only after
approval of the exact staged tree and message; two separate approvals for corrections (request
vs final diff); post-merge correction isolation; a tree hash is not a backup; if the handoff
cannot preserve the actual changes, stop and ask.
- **Finding (read-only): `core.autocrlf=true`** (from `C:/Program Files/Git/etc/gitconfig`), no
  `.gitattributes`. A plain `git add` would convert CRLF → LF in the stored blobs, so the
  baseline would **not** be byte-identical — conflicts with D-05 (archive byte-identical) and
  "as received".

**Round 4 — user requested final proposals for T1–T5 and a consistency check.** Outcome = D-07
v4 (in `PROJECT_DECISIONS.md`). Decisions proposed (none approved):

| # | Proposal | Alternatives / why not | Needs from the user |
|---|---|---|---|
| T1 | Root `.gitattributes`: `* -text`, plus `/.gitignore text` and `/LICENSE text`; completeness + SHA-256 equality checks (working tree = staged blob = committed blob) | local `core.autocrlf=false` (machine-only, not repo-level); normalizing (breaks D-05); plain `* -text` (would make `.gitignore`/`LICENSE` show as modified) | add `.gitattributes` to step 0.1 scope; LF for new authored files |
| T2 | Corrections on the next open gate branch, isolated (clean tree, approved paths only, own commit, separate "Corrections to earlier gates" section, merge request lists every commit); after Gate 3: `feature/phase-0-correction-<k>` | separate branch per correction: outside "one branch per gate", needs a `develop` back-merge into the open gate branch (undefined in D-07) and extra approvals. Weakness of chosen option: correction reaches `develop` only at the next gate merge | approve |
| T3 | Prospective records in the step's own diff; retrospective records (hash, tree-match, approval/merge outcomes) in the next step's commit, as a separately identified "Records-only" section; no records commit; working tree clean between commits | uncommitted records would block gate merges/branch switches; a records commit only if the user proposes it. Weakness: docs show a step's hash one step late; Git log is the authority meanwhile | approve |
| T4 | Verified repo-external backup per uncommitted review state: `staged.patch`, `unstaged.patch`, file copies, `MANIFEST.sha256`, `STATE.md`, `CURRENT.txt`; verified by checksums + `git apply --cached --check` in a temporary index | tree hash or handoff text cannot restore content; scratchpad is session-specific | **`<backup-root>` path + write permission** (before 0.1) |
| T5 | Six separate approvals: step start; step tree/message; correction request; correction final diff; gate review; gate merge (also release review/merge, push, deletion, backup, DB approvals) | — | approve (overhead accepted knowingly) |

**Read-only verification report (Session 6, 2026-10-13):**
- Git 2.52.0.windows.1; branch `main`, HEAD `af89144` = `origin/main`; no other branch; no stash;
  untracked: `CLAUDE.md`, `DefaultTemplate/`, `docs/`, `knowledge Docs/`.
- Config: `core.autocrlf=true` (system); `core.eol`, `core.attributesFile` unset; no
  `.gitattributes` anywhere, no `.git/info/attributes`; `filter.lfs.*` configured globally but no
  path has a `filter` attribute (no effect).
- Tracked `.gitignore` and `LICENSE`: `i/lf w/crlf` (LF blobs, CRLF working tree); `.gitignore`
  has no `*.user` rule.
- Inventory: 409 regular files under `DefaultTemplate/` + `knowledge Docs/` = 152 candidates +
  257 ignored (only `bin/`, `obj/` of the four projects); 151 to stage after `*.user`; 0 symlinks,
  0 empty directories, 0 non-ASCII names; ~8.4 MB.
- Line endings of the 151: 29 CRLF-only, 98 LF-only, 0 mixed, 24 without newline.
- Filter simulation (`git hash-object` without `-w`, nothing written): under the current config
  **29 blobs would change** on add; archive blob filtered `8e2cd341…` vs raw `f494a343…`; archive
  SHA-256 `6826f819…a86b` unchanged.

**Consistency (v4):** D-06 — explicit partial supersession (branch clause; M8 §4 items (2)–(4);
correction exception), rest Approved. M8 §4 — items (1), (5) unchanged. D-05 — satisfied only if
T1 is approved, otherwise conflict remains. D-29/D-30 — diff approval ≠ applying a migration;
generated files committed as written; revert/regeneration keeps separate approval. D-40 — backups
outside repo and DBs; verification only on test DBs. Gates — merge commits carry no records;
previous gate's retrospective records go into the next gate's first step. Scope conflict: step
0.1 grows by `.gitattributes` (needs approval). Workflow trade-off: hash recorded one step late.

**Documentation impact list (to apply only after D-07 approval):**
1. `PROJECT_DECISIONS.md`: header line (D-07 approved, D-06 partly superseded); D-06 heading →
   "Approved (partly superseded by D-07, <date>)" and "M14 (pending)" paragraph → the
   partial-supersession wording in D-07; D-07 heading → Approved; §9: D-07 rows resolved, keep
   "D-07 (later)".
2. `PHASE_0_PLAN.md`: rules line 11–14 (gate branches; exactly one step commit after approval of
   exact tree/message; correction exception; T1/T3/T4 references); authorization rule (new
   sequence incl. backup); gate report rule (step vs correction commits, "Corrections to earlier
   gates", classified commit list for merges); checklist rule (`[x]` prospective in own commit,
   hash retrospective in next commit, Git log authoritative; `[~]` also "verified, awaiting
   approval of the staged tree"; correction commits listed under the corrected step); step 0.1
   row (Step = D-07 Phase 0 mapping 1–3; Requires = D-07 Approved + T1 + `<backup-root>` + §18 +
   separate 0.1 approval; Main files + `.gitattributes`; Done when = completeness, equality,
   tree-match, `main`/`develop` at `af89144`, nothing pushed, only session docs untracked);
   0.2/0.3 Done when (blob ids unchanged by `git mv`; archive check on working tree and committed
   blob; 0.2 carries 0.1's records); "⛔ Gate N" lines (merge only on explicit merge approval;
   corrections section; retrospective records into next gate's first step); exit criteria
   (`develop` → `main` only after release review + approval; post-Gate-3 correction branch).
3. `SESSION_HANDOFF.md`: header; §2 (branches not yet created; line-ending facts); §5 (D-06
   partly superseded; D-07 summary); §6 (only step 0.1 approval; `<backup-root>` before 0.1; EOL
   policy before 0.2); §7 (commit only after exact tree/message approval; correction (A)+(B); no
   stash/switch/reset/gc while uncommitted; no verified backup → stop and ask); new §9
   "Uncommitted review state" (until 0.2; then a permanent pointer to `<backup-root>/CURRENT.txt`);
   §8 (backup-root decision, then step 0.1 per D-07 Phase 0 mapping).
4. `CLAUDE.md`: commit rule (review-before-commit, correction commits, T3, T4 stop-and-ask);
   branching rule line (incl. byte-for-byte `.gitattributes`); start-of-session step 4 (check
   `<backup-root>/CURRENT.txt` and verify per T4).
5. `DECISION_LOG.md`: approval entry (§21 or next number); "Unresolved decisions" table updated.

**Done in this session (documentation only, at the user's request):** D-07 entry replaced by the v4
wording (still **Proposed**); D-06 "M14 (pending)" note updated to point at the v4 partial
supersession (D-06 unchanged otherwise, still Approved); `PROJECT_DECISIONS.md` header line and §9
rows (D-07 v4, T1, T1 policy, T4) added, stale "Required now" line corrected; PHASE_0_PLAN pending
note and SESSION_HANDOFF updated; this entry. **No status changed, no Git state changed, nothing
staged, committed or executed.** Session 6 closed here at the user's request (conversation too
long) — continue from `SESSION_HANDOFF.md`.

## Session 7 — 2026-10-13

### 21. D-07 review rounds 5–6: v4 → v5 → v6 (user, Session 7) — documentation only, D-07 still Proposed
In both rounds the user said: the overall direction / approval boundaries / read-only approach are
acceptable, but **D-07 is not approved, the documentation edits are not approved and step 0.1 is
not authorized**; no file edits, staging, branch creation or switch, backup, commit, merge, push,
config change or step 0.1. All checks were read-only. Approvals are never inferred from a request.
At the end of the session the user asked to record the outcome in the docs (this entry; D-07 v6
text placed in `PROJECT_DECISIONS.md`). v5 was never written to the docs; v6 contains it.

**Round 5 — user requested (v4 → v5):**
1. T1: compare the complete received-file inventory with staged + explicitly excluded files;
   verify working-tree, staged-blob and committed-blob SHA-256; any mismatch stops without
   modifying received files. → `L_disk` / `L_excluded` / `L_staged` defined; disjoint + union =
   `L_disk`; only approved rules `[Bb]in/`, `[Oo]bj/`, `*.user`; no edit, re-stage, unstage,
   renormalize, amend, reset or extra commit to make a check pass.
2. New-file LF policy kept Pending until explicitly confirmed; consequences of `.gitattributes`
   spelled out. → separate decision **T1-EOL (Pending)**; D-07 approval does not confirm it;
   `.gitattributes`' own LF bytes are approved with step 0.1's staged tree only; step 0.2 cannot
   start before T1-EOL is decided. Consequences: reach/override of `core.autocrlf`, byte-exact
   storage, not binary (textual diff/merge, `^M`), CRLF-writing tools produce CRLF blobs, mixed
   per-file conventions by design, `.gitignore`/`LICENSE` unchanged (`i/lf w/crlf`).
3. T3: retrospective records never modify a committed step or go into an integration-only merge
   commit. → stated explicitly.
4. T4: distinguish patch-application checks from full backup verification; no backup before the
   user provides and authorizes the location. → levels (a) integrity, (b) patch-application
   ("patch-checked", not verified), (c) reconstruction; no folder, file, test write or permission
   probe before authorization.
5. T2, T5, DB/migration, branch-switching, no-history-rewrite protections retained unchanged.

**Round 6 — user requested (v5 → v6):**
1. T1: define precisely how `L_disk`, `L_excluded`, `L_staged` are generated and compared;
   distinguish files staged by this step from pre-existing index entries. → generation order with
   commands (`I_before` = index, precondition index = HEAD and no entry under the two folders;
   `L_disk` recorded twice; `L_excluded` via `git ls-files -o -i` + `git check-ignore -v
   --no-index`; one staging command; `I_after`, `L_staged`, `D_step` = exact expected change set);
   NUL-separated, `LC_ALL=C` sorted, byte comparison; case-only collision → stop.
2. Separate pre-commit from post-commit checks. → **B1–B6** (inventory, index scope, working tree,
   staged blobs, archive in working tree + index, staged tree `T_staged`; re-checked just before the
   commit) vs **C1–C4** (`HEAD^{tree}` = approved tree, committed blobs, archive committed blob,
   final state); no C-check is claimed before the commit exists.
3. T4 full reconstruction for added/modified/deleted/renamed paths and non-empty `unstaged.patch`.
   → patches with `--no-renames` (rename = delete + add), `CHANGES.txt` (authored change set incl.
   untracked files), `files/` copies, `history.bundle`; reconstruction in a bare clone of the
   bundle inside the backup folder with temporary indexes: staged tree must equal `T_staged`;
   working-tree state reconstructed from `unstaged.patch` + copies and checked per path
   (`git hash-object --path` without `-w` against the real repo). Real index, working tree, HEAD,
   refs, config and object store never written. **Limitation found:** a patch alone depends on
   HEAD objects that exist only in the local repo (no push) → bundle added. Further limitations
   (ignored paths, symlinks/submodules, unstaged mode-only changes, unrepresentable paths, checks
   that cannot run) → backup not verified → stop and ask.
4. T3 and step 0.2: step 0.1's records explicitly in step 0.2's declared scope (PHASE_0_PLAN 0.1
   row, SESSION_HANDOFF §2/§3/§4/§6/§8 + pointer replacing "Uncommitted review state",
   DECISION_LOG entry), additive, shown against a `pre-records/` snapshot because the files enter
   Git first in 0.2; no amend, no records in merge commits.
5–6. T1-EOL stays Pending; no write before the backup location is authorized; T2, T5,
   D-29/D-30/D-40, DB/migration protections, branch-switch safeguards, no-rewrite rules unchanged.

**Proposed D-06 partial-supersession wording (v6, exact; applied only on D-07 approval):**
> **Superseded in part by D-07 (approved <date>, DECISION_LOG §<n>):** (a) the branch clause —
> Phase 0 work runs on gate branches `feature/phase-0-gate-N` created from `develop`, local only;
> (b) items (2)–(4) of "Confirmed strictly (M8 §4)" — each step now runs: complete the step →
> pre-commit verification → show the complete diff, verification, exact staged tree hash and
> commit message (as-received content additionally as inventory + integrity check, any file
> inspectable on request) and make a verified backup → explicit user approval of that exact tree
> and message → exactly one step commit → post-commit verification (`HEAD^{tree}` = approved tree)
> and report → stop (D-07 "Step workflow");
> (c) "one commit per step" now has one exception: correction commits under D-07's exception rule
> (separate request and final-diff approvals), which are not step commits. Gate and release
> `--no-ff` merge commits are integration commits — not step commits, and they carry no records.
> Everything else above remains Approved: gates, stop rules, "do not commit documentation without
> the user's review", M6 C (each step and gate needs its own explicit approval), items (1) and (5)
> of M8 §4, and gate reporting.

**Read-only verification report (Session 7, 2026-10-13):**
- Git 2.52.0.windows.1; branch `main`, HEAD `af89144` = `origin/main`; only `main` locally; no
  stash; untracked exactly `CLAUDE.md`, `DefaultTemplate/`, `docs/`, `knowledge Docs/`.
- Index: 2 entries (`.gitignore` `7282dbf2…`, `LICENSE` `e70108c0…`, 100644, stage 0); index =
  HEAD; 0 unmerged; none under the received folders → `I_before` precondition holds today.
- Config: `core.autocrlf=true` (system); `core.filemode=false`, `core.symlinks=false`,
  `core.ignorecase=true` (`.git/config`); `core.eol`, `core.attributesFile`, `core.safecrlf`,
  `core.quotepath`, `diff.renames` unset; no `.gitattributes`; `.git/info/` only `exclude`.
- Tracked `.gitignore`, `LICENSE`: `i/lf w/crlf attr/`.
- `L_disk`: 409 regular files; 0 non-regular; 0 non-ASCII paths; 0 case-only collisions; **1 empty
  directory** `DefaultTemplate/DefaultTemplate/obj/Debug/net8.0/staticwebassets` inside ignored
  `obj/` (corrects §20 "0 empty directories"; no effect on the baseline).
- 152 untracked non-ignored + 257 ignored (157 `.gitignore:21 [Bb]in/`, 100 `.gitignore:22
  [Oo]bj/`); one `*.user` file → 151 to stage, `L_excluded` 258.
- Line endings of the 151 (byte counts): 29 CRLF-only, 98 LF-only, 0 mixed, 24 without any
  newline; **53 without a final newline** (incl. the 24; new detail). Filter simulation
  (`git hash-object` without `-w`): 29 blobs would change under the current config.
- Session docs (`CLAUDE.md`, `docs/*.md`): LF-only (0 CR).
- Archive SHA-256 `6826f819e3ea5cd6c756cababcd4b2e17bd5dd95199bfcb0e31829c07255a86b` unchanged.
- Not exercised (would write files): `git bundle`, `git apply`, `git clone` — T4 level (c) is a
  specification, not evidence.

**Documentation impact list (v6) — to apply only after D-07 approval; replaces the §20 list:**
1. `PROJECT_DECISIONS.md`: header line (D-07 approved, D-06 partly superseded); D-06 heading →
   "— Approved (partly superseded by D-07, <date>)" and its "M14 (pending)" paragraph → the
   wording above; D-07 heading → "— Approved (<date>)", with T1-EOL and T4 `<backup-root>` still
   marked Pending inside unless decided separately; §9: "D-07 (M14, v6)" and "D-07 T1 scope"
   resolved; keep "D-07 T1-EOL", "D-07 T4" (backup root) and "D-07 (later)" until resolved.
2. `PHASE_0_PLAN.md`: rules lines 11–16 (gate branches; exactly one step commit after approval of
   exact tree/message; correction exception; T1/T3/T4 references); authorization rule (pre-commit
   verification → backup → approval → commit → post-commit verification); gate report rule (step
   vs correction commits, "Corrections to earlier gates", classified commit list for merges);
   checklist rule (`[x]` prospective in own commit, hash and post-commit results retrospective in
   next step commit, additive, never by amend or in a merge commit, Git log authoritative; `[~]`
   also "verified, awaiting approval of the staged tree"; correction commits under the corrected
   step); step 0.1 row (Step = D-07 Phase 0 mapping 1–4; Requires = D-07 Approved + T1 scope +
   `<backup-root>` named and write-authorized + §18 + separate 0.1 approval; Main files +
   `.gitattributes`; Done when = B1–B6 before commit, C1–C4 after commit); step 0.2 row (Step +
   "step 0.1 records (T3)"; Requires + "T1-EOL confirmed"; Done when + blob ids unchanged by
   `git mv`, archive check before commit (working tree) and after commit (committed blob), records
   hunks shown against `pre-records/`); 0.3 Done when (blob ids unchanged by `git mv`); "⛔ Gate N"
   lines (merge only on explicit merge approval; corrections section; retrospective records into
   next gate's first step); exit criteria (`develop` → `main` only after release review +
   approval; post-Gate-3 correction branch).
3. `SESSION_HANDOFF.md`: header; §2 (branches not yet created; line-ending facts); §5 (D-06 partly
   superseded; D-07 summary); §6 (step 0.1 approval; `<backup-root>` before 0.1; T1-EOL before
   0.2); §7 (commit only after exact tree/message approval; index = HEAD before staging;
   correction (A)+(B); no stash/switch/reset/gc while uncommitted; failed B/C check → stop without
   modifying anything; no write of any kind before the backup location is authorized; no verified
   backup → stop and ask); new §9 "Uncommitted review state" (until 0.2; then permanent pointer to
   `<backup-root>/CURRENT.txt`); §8 (backup-root decision, then step 0.1 per D-07 Phase 0 mapping).
4. `CLAUDE.md`: commit rule (review-before-commit, pre/post-commit verification, correction
   commits, T3 no amend / no records in merges, T4 stop-and-ask); branching rule line (incl.
   byte-for-byte `.gitattributes`); start-of-session step 4 (check `<backup-root>/CURRENT.txt`,
   verify per T4).
5. `DECISION_LOG.md`: approval entry (next number); "Unresolved decisions" table updated.

**Done in this session (documentation only, at the user's request at session end):** D-07 entry
replaced by the v6 wording (still **Proposed**); `PROJECT_DECISIONS.md` header line, D-06 "M14
(pending)" pointer (v6, §21) and §9 rows (D-07 v6, T1 scope, T1-EOL, T4) updated; PHASE_0_PLAN
pending note and SESSION_HANDOFF updated; this entry. **No status changed, no Git state changed,
nothing staged, committed, backed up or executed.** Pre-edit snapshot of the docs in the Session 7
scratchpad (not persistent). Session 7 closed here at the user's request (conversation too long) —
continue from `SESSION_HANDOFF.md`.

## Session 8 — 2026-10-13

### 22. D-07 v6 targeted review (round 7) and v6.1 correction draft (round 8) — documentation only, D-07 still Proposed
In both rounds the user said: no full rewrite of D-07, no repository file edit, staging, commit,
branch creation, backup write or step 0.1; D-07 stays Proposed; no approval is inferred. Round 7
was a targeted technical review of T1 and T4 (correctness and recovery risk only, no cosmetic
wording). Round 8: the user reviewed the findings and asked for a **v6.1 draft** with the material
corrections only (F1, F2 mandatory; F3, F4 recommended hardening). **The v6.1 draft is not
applied to the D-07 text in `PROJECT_DECISIONS.md`**; it is recorded below for the user's
decision. At session end the user asked to record the session in the docs (this entry).

**Evidence method.** Project repository: read-only only (`git config --show-origin`,
`git check-attr`, `find`, reading `C:/Program Files/Git/etc/gitattributes`). Mechanisms were
tested in throwaway repositories in the Session 8 scratchpad (not persistent), Git
2.52.0.windows.1. Nothing was written to the project repository, `.git/` or any backup location.

**Read-only facts (this machine, Session 8):** no `.gitattributes` anywhere under
`DefaultTemplate/` or `knowledge Docs/`; `.git/info/` contains only `exclude` (no `attributes`);
`core.attributesFile` unset; system gitattributes sets only `diff=astextplain` for
`*.doc/docx/dot*/pdf/rtf/od*` (no received file matches; no byte change); system config defines
`filter.lfs.*` (`required=true`) but no path has `filter=lfs` → inactive; no received file
contains `$Id`; `core.autocrlf=true` (system), `core.filemode=false` (`.git/config`); no global or
system `color.*`, `diff.noprefix`, `diff.mnemonicPrefix`, `diff.relative` or `apply.*` setting.

**Round 7 findings:**
- **F1 (T1, medium) — byte-for-byte claim too broad.** `* -text` only disables line-ending
  conversion (`text`/`eol`, `core.autocrlf`/`core.eol`). Scratch tests showed bytes still change
  when: `$GIT_DIR/info/attributes` sets `text` (higher precedence than in-tree; CRLF normalized);
  a subdirectory `.gitattributes` sets `text` (overrides the root); `ident` is set (`$Id$`
  collapsed on add, expanded on checkout) or `working-tree-encoding` is set (UTF-16 re-encoded) —
  both independent of `-text`; a lower-precedence `core.attributesFile` cannot override `text`
  but can add `ident`. `filter` (clean/smudge) is independent per Git documentation (not tested).
  Step 0.1's staged and committed bytes are still proved by B4/C2; the unchecked part is the
  claim "on any machine or clone … checkout writes them back unchanged" and future nested
  `.gitattributes` (policy (5) covered only the root file).
- **F2 (T4, medium) — staged deletion vs untracked copy contradiction.** After `git rm --cached`
  (or a staged deletion whose file reappears), the path is both staged `D` and untracked (scratch
  test). "Contents" requires a copy in `files/`, but (c)4 said "deleted → absent from `files/`" →
  either a false verification failure or, if followed literally, the only backup of that file is
  dropped (data loss). No step 0.1 impact (no deletions); applies to later steps.
- **F3 (T4, low, conditional) — patch generation depends on configuration.** Scratch tests:
  `color.ui=always` injects ANSI escapes into the patch; `diff.noprefix=true` makes `git apply`
  fail. Regeneration check (a) uses the same config and cannot detect it; (c) fails → backup
  unverified → stop (no silent loss). Not set on this machine today.
- **F4 (T4, low) — "real index never written" is inaccurate.** Scratch test: plain
  `git status` rewrote the index file (stat refresh); `git --no-optional-locks status` did not;
  `git write-tree` after `git add` rewrote it (cache-tree). Entries/`T_staged` are unchanged.
- **Confirmed sound (scratch tests):** bare clone from bundle + temporary `GIT_INDEX_FILE` +
  `git apply --cached` + `git write-tree` reproduced `T_staged` including CRLF content under
  `-text`; staged rename recorded as `D`+`A`; filesystem rename recorded as unstaged `D` + untracked
  new path; `diff-index` on `index.work` showed exactly the unstaged `D` entries; (c)3 relies on
  (c)2 having written the staged blobs into the verification repository (order already correct).

**Round 8 — v6.1 draft (exact replacement text; not applied, awaiting the user's decision):**

*F1 — T1.*
1. "Reach" bullet, replace entirely:
   > - **Reach:** the file is versioned, so its rules apply in every commit and branch that
   >   contains it (not in commits without it: `af89144`; `main`/`develop` until the Gate 1
   >   merge). In any clone they take precedence over `core.autocrlf`, `core.eol`, and over
   >   `text`/`eol` set in `core.attributesFile` or the system attributes file. They do **not** take
   >   precedence over that clone's `$GIT_DIR/info/attributes` or over a `.gitattributes` in a
   >   subdirectory, and they do not unset other content-changing attributes (`filter`, `ident`,
   >   `working-tree-encoding`), which any attributes source — including lower-precedence ones — can
   >   still set. A clone's local attribute sources are not versioned and are outside this
   >   decision's control.
2. "`* -text`" bullet, first sentence ("Git never converts … missing final newline.") replaced by:
   > - **`* -text`:** Git performs no line-ending conversion for these paths, on add or on
   >   checkout, wherever the rule is in effect (see Reach). This alone does not guarantee
   >   byte-for-byte storage or checkout: bytes are unchanged only where, for that path in that
   >   clone, `text` resolves to `unset` and `filter`, `ident` and `working-tree-encoding` are
   >   `unspecified`. Byte-for-byte **storage** is established per commit by checks (step 0.1: B4
   >   before the commit, C2 after it; later steps: policy (4)). Byte-for-byte **checkout** in
   >   another clone or on another machine, or after any attribute-source change, is not guaranteed
   >   or verified by this decision. Where these conditions hold, the received files keep their
   >   current form (…unchanged from v6…).

   Same bullet, last sentence: "`git add --renormalize` has no effect on these paths." →
   "Where these conditions hold, `git add --renormalize` causes no change on these paths."
3. T1 "Conventions", append: "All read-only Git commands of the generation steps and of B1–B6 /
   C1–C4 run as `git --no-optional-locks …` from the repository root."
4. Generation 1 (`I_before`) precondition, append: "Also reported (not assumed from earlier
   sessions): contents of `$GIT_DIR/info/attributes` if present, and `git config --show-origin
   --get-all core.attributesFile`. These are reported only; the per-path effect of all attribute
   sources is checked by B4."
5. B2, append: "No `L_staged` path is named `.gitattributes`; the root `.gitattributes` is the only
   one in `I_after`."
6. B4, replace entirely:
   > - **B4 Staged blobs:** for every `L_staged` path, SHA-256(`git cat-file blob :<path>`) = its
   >   `L_disk` value; blob id = `git hash-object --no-filters -- <path>` (no `-w`);
   >   `git check-attr -z --stdin text eol filter ident working-tree-encoding` over all `L_staged`
   >   paths gives `text: unset` and `unspecified` for the other four. For `.gitattributes` the
   >   same; for `.gitignore` and `LICENSE`: `text: set`, the other four `unspecified`. Any other
   >   value → stop. These results describe this clone's attribute sources at the time of the
   >   check only.
7. Policy (4) and (5), replace:
   > (4) Step workflow 3 reports, for every staged authored or generated path, `git ls-files --eol`
   > and `git check-attr text eol filter ident working-tree-encoding`. Expected: `text: unset`
   > (`/.gitignore`, `/LICENSE`: `set`) and the other four `unspecified`. Any convention change,
   > deviation or other attribute value is flagged and stops the step for approval;
   > (5) Attribute sources: (i) creating, changing or deleting a `.gitattributes` anywhere in the
   > tree (root or subdirectory) changes the T1 rule and needs explicit approval of that path and
   > its content. A `.gitattributes` found under a received folder (in `L_disk` or a later diff)
   > → stop; (ii) `$GIT_DIR/info/attributes`, `core.attributesFile` and the system attributes file
   > are local configuration: the assistant never creates or changes them, since every Git config
   > or attribute-source change needs explicit approval. Their per-path effect is checked by B4 and
   > (4) only at the time of each check; (iii) any `git add --renormalize` needs explicit approval
   > and never applies to received files.

   Validated by: B2, B4 (step 0.1), policy (4) (later steps).

*F2 + F3 — T4.*
8. "Contents": bullets `staged.patch`, `unstaged.patch`, `CHANGES.txt`, `files/` replaced by:
   > - **Fixed commands.** At backup time, in the regeneration check (a) and on resume (rule 4),
   >   the backup's Git outputs are produced with exactly these commands, run from the repository
   >   root, with `P` = `--binary --full-index --no-renames --no-ext-diff --no-textconv --no-color
   >   --src-prefix=a/ --dst-prefix=b/ --no-relative`:
   >   `staged.patch` — `git --no-optional-locks diff --cached P HEAD`;
   >   `unstaged.patch` — `git --no-optional-locks diff P` (index → working tree, tracked paths; may
   >   be non-empty outside a review stop);
   >   path lists — `git --no-optional-locks diff --cached --name-status --no-renames -z HEAD`,
   >   `git --no-optional-locks diff --name-status --no-renames -z`,
   >   `git --no-optional-locks ls-files -o --exclude-standard -z`.
   >   Other diff output configuration not neutralized by `P` (e.g. `diff.suppressBlankEmpty`) is
   >   detected only by check (c).
   > - `CHANGES.txt` — the authored change set `A`: the union of the three path lists, excluding
   >   untouched as-received untracked content (checked against the T1 manifest instead). One
   >   entry per path, listing **every** state that applies: staged (A/M/D/T), unstaged (M/D/T),
   >   untracked. A path may have several, e.g. staged A + unstaged M, or staged D + untracked (the
   >   file remains or reappears in the working tree after a staged deletion, e.g.
   >   `git rm --cached`). Each entry also records the SHA-256 of the working-tree file, or
   >   "absent", determined by testing the working-tree path directly, not from the lists.
   >   Consistency: an untracked state occurs only for paths absent from the index, and an
   >   unstaged state only for paths in the index. Any other combination → stop.
   > - `files/` — byte copies of exactly those paths in `A` whose working-tree value is not
   >   "absent", whatever their states.
9. "Renames" paragraph, append: "A staged deletion whose file is still or again present in the
   working tree is recorded as staged D plus its working-tree hash, and is copied in `files/`."
10. Check (a), replace:
    > - (a) **Integrity:** every backup file is listed in `MANIFEST.sha256` and matches it. The set
    >   of paths in `files/` equals exactly the paths of `A` not recorded "absent", and each copy has
    >   the SHA-256 recorded in `CHANGES.txt`, equal to its working-tree file. Regenerating both
    >   patches and `CHANGES.txt` with the fixed commands gives byte-identical results.
    >   `git bundle verify history.bundle` passes and the bundle contains the recorded HEAD.
11. Check (c)4, replace ((c)1–3 unchanged; they verify index content only):
    > 4. **Per path in `A`, by final state** (index = `index.work`; working tree = the recorded
    >    value):
    >    - in `index.work`, present in the working tree (staged A/M/T and/or unstaged M/T): copy in
    >      `files/`; its blob id in `index.work` = `git hash-object --path=<path> --
    >      <folder>/files/<path>` run against the real repository without `-w` (applies that path's
    >      attributes as `git add` would); copy SHA-256 = recorded value;
    >    - in `T_staged`, not in `index.work` (unstaged D, including staged A + unstaged D):
    >      recorded "absent", no copy;
    >    - not in `index.work`, present in the working tree (untracked, including a staged deletion
    >      whose file remains or reappears, ignored or not): copy in `files/`, SHA-256 = recorded
    >      value. The copy is the only source, so byte equality is the proof;
    >    - not in `T_staged`, not in `index.work`, absent from the working tree (staged D only):
    >      recorded "absent", no copy.
    >
    >    A path that fits none of these cases, or whose `CHANGES.txt` states contradict its case →
    >    not verified.
12. While-uncommitted rule 4 (resume): "manifest and patches against the latest backup" →
    "manifest and patches, regenerated with the fixed commands, against the latest backup".

   Validated by: (a), (c)2 (`write-tree` = `T_staged`), (c)3, (c)4. Scratch test: the fixed
   command with `color.ui=always`, `color.diff=always`, `diff.noprefix`, `diff.mnemonicPrefix`,
   `diff.relative=true`, run from a subdirectory, produced a byte-identical patch that applied to
   the same tree.

*F4 — T4.*
13. "Isolation", replace entirely:
    > **Isolation.** The backup and verification procedure uses the real repository only through
    > `git --no-optional-locks` with `diff`, `ls-files`, `status` and `check-attr`, `git hash-object`
    > without `-w`, and `git bundle create` (which writes only the bundle file in the backup
    > folder). All indexes, objects and the verification repository of the procedure live in the
    > backup folder. The procedure itself never writes the real index entries, HEAD, refs or
    > config. `--no-optional-locks` keeps `git status`/`git diff` from opportunistically rewriting
    > the real index with refreshed stat data. It is not a guarantee that nothing writes the real
    > repository: (1) `git write-tree`, used for `T_staged` in Step workflow 3–5, B6 and resume
    > rule 4, writes tree objects into the real object store and may rewrite the real index file
    > (cache-tree data; entries unchanged). It belongs to the step workflow, not to the backup
    > procedure; (2) other processes (IDE Git integration, editors, build tools) can write the
    > index, working tree or object store at the same time; this decision does not control them.
    > Concurrent content changes are detected only if they fall between generation and
    > regeneration (check (a)) or show up at resume (rule 4). The proof holds only for the moment
    > of the backup.

    Validated by: no automatic check (it states limits); concurrent content changes → (a), rule 4.

**Unverified (not claimed):** `filter` behaviour under `-text` (documentation only); that
`git diff --no-optional-locks` never writes the index (only `status` tested) and that this holds
in all Git versions; whether `write-tree` always or only with an invalid cache-tree rewrites the
index (observed once); `diff.suppressBlankEmpty` and other unneutralized diff config;
`hash-object --path` for `/.gitignore` (autocrlf + `text`) on real data; `git bundle verify`;
`git check-attr -z --stdin` on real data; B/C checks and full T4 (a)–(c) on the real 409 files;
attribute/config state of any other machine or clone.

**Done in this session (documentation only, at the user's request at session end):** this entry;
"Unresolved decisions" D-07 row; `PROJECT_DECISIONS.md` header line, D-07 pending-v6.1 note and
§9 D-07 row; PHASE_0_PLAN pending note; SESSION_HANDOFF. **The D-07 v6 text is unchanged; v6.1
is not applied. No status changed, no Git state changed, nothing staged, committed, backed up or
executed.** Session 8 closed here at the user's request (conversation too long) — continue from
`SESSION_HANDOFF.md`.

## Session 9 — 2026-10-13

### 23. D-07 v6.1 targeted corrections (round 9) and approval-readiness checklist — documentation only, D-07 still Proposed
The user asked for two final targeted corrections to the v6.1 draft (§22), each validated for
consistency, with explicit limits: no full rewrite, **no v6.2**, no further review cycle, no
project file edit, staging, commit, branch, backup, config change or step 0.1. D-07 stays
Proposed; no approval is implied. At session end the user asked to record the session in the docs
(this entry).

**Round 9a — F2, T4 check (c)4, second case.**
- The user first proposed: "in `index.work`, absent from the working tree (unstaged D, …)".
  **Assistant finding (accepted by the user):** this contradicts (c)3. `index.work` =
  `read-tree T_staged` + `git apply --cached unstaged.patch`; an unstaged deletion removes the path
  from `index.work`, and (c)3 requires `diff-index --cached T_staged` to equal exactly the unstaged
  entries (also seen in the Round 7 scratch test, §22). With that wording a real unstaged D
  (in `T_staged`, not in `index.work`, absent) fits no case → every unstaged deletion would fail
  falsely; the new case itself would be unreachable.
- **Final clause (accepted; replaces the second case of §22 item 11):**
  > - in `T_staged`, not in `index.work`, absent from the working tree (unstaged D, including
  >   staged A + unstaged D): recorded "absent", no copy;

  Kept unchanged: "A path that fits none of these cases, or whose `CHANGES.txt` states contradict
  its case → not verified."
- **Validation (accepted as reported):** the four cases are mutually exclusive — (1) in
  `index.work` + present; (2) in `T_staged`, not in `index.work`, absent; (3) not in `index.work`
  + present; (4) not in `T_staged`, not in `index.work`, absent. Consistent with (c)2 (staged D not
  in `T_staged`; staged A in it) and (c)3 (`unstaged.patch` covers tracked paths only, so
  `index.work` ⊆ `T_staged`; unstaged D removed). Uncovered states fail correctly: in `index.work`
  but absent → "fits none"; in `T_staged`, not in `index.work`, present → case 3 but its
  `CHANGES.txt` unstaged-D state contradicts → not verified. Case 1 blob check against
  `index.work` is valid because (c)3 applies unstaged M/T content.

**Round 9b — F4, "Isolation".** Final wording (accepted; replaces the sentence "All indexes,
objects and the verification repository of the procedure live in the backup folder." in §22
item 13; the rest of item 13 unchanged):
> The reconstruction verification repository, including its temporary indexes and objects, lives
> in the backup folder. The backup contains the history bundle and the other backup artifacts. The
> separate step-workflow command `git write-tree` may write tree objects to the real repository's
> object store, as described below.

Evolution: the user's first wording ("The backup and reconstruction verification repositories,
including their indexes and objects, …") was consistent but implied the backup is a repository
(it holds `history.bundle`, a file) → user's clarification; the assistant then flagged singular
"its index" vs the two temporary indexes `verify/index.staged` ((c)2) and `verify/index.work`
((c)3) → user's correction to "temporary indexes". Validation: does not imply the real object
store is never written; consistent with the F4 command list, "never writes the real index
entries, HEAD, refs or config" and item (1) (`write-tree` belongs to the step workflow).

**Review outcome (user, 2026-10-13):** the targeted review of D-07 v6.1 is complete for the
clauses discussed; final F2 and F4 wording accepted **for this review**. No further issue was
identified by these targeted checks — **not a full technical audit**. **Not decided:** whether
v6.1 F1 and F3 (§22 items 1–8, 12) are accepted as drafted (not discussed this session). v6.1 is
**not applied** to `PROJECT_DECISIONS.md` (D-07 text still v6).

**Approval-readiness checklist (prepared at the user's request; read-only).** Repository state
re-verified read-only (`git --no-optional-locks status`, `branch -a`, `log`, `stash list`,
`ls-files -s`, `diff --cached --quiet HEAD`, `config --show-origin core.autocrlf`, archive
`sha256sum`): `main`, HEAD `af89144`, only `main` local (+ `origin/main`), no stash, four
untracked items, index = HEAD (`.gitignore` `7282dbf2…`, `LICENSE` `e70108c0…`),
`core.autocrlf=true` (system), no `.gitattributes`, archive SHA-256 unchanged. **Not re-checked:**
409/151/258 counts, line-ending counts, attribute sources (Session 7–8 facts).
Checklist content, in brief:
1. **D-07 approval** would cover: D-07 text (v6 with v6.1 applied), D-06 partial-supersession
   wording (§21), documentation impact list (§21), T1–T5. Supersedes D-06 **in part only** (branch
   clause, M8 §4 items (2)–(4), correction-commit exception); no FR/BR superseded. Does not cover
   step 0.1, T1-EOL, `<backup-root>`, push/PR.
2. **T1-EOL** options: (a) LF — **recommended** (session docs and `.gitattributes` already LF;
   one convention; CRLF tool output flagged in review), (b) CRLF, (c) no policy. Under `* -text`
   Git does not enforce it — only the Step workflow 3 `ls-files --eol` check does. Blocks 0.2, not
   0.1.
3. **`<backup-root>`**: user-supplied path outside repo and scratchpad + explicit write
   authorization (not probed). No separate scratch location: `verify/` lives inside each backup
   folder; the session scratchpad holds only non-persistent doc-diff snapshots. To confirm:
   persistent local (non-sync) folder, write permission, short path (Windows path-length risk,
   unverified), disk space (step 0.1 backup holds 151 files as patch + copies; size not measured),
   same-drive risk, no delete/restore without approval.
4. **Step 0.1**: as D-07 Phase 0 mapping 1–4; T4 level (c) never run on the real repository — a
   specification until step 0.1.
5. **Separate approvals, in order:** (A) documentation approval — apply v6.1 (with final F2/F4,
   and F1/F3 if accepted) to D-07, D-07 still Proposed; (B) D-07 approval + impact list; (C)
   T1-EOL (before 0.2); (D) `<backup-root>` + write authorization (before 0.1); (E) step 0.1
   start; (F) exact staged tree hash + commit message.

**Done in this session (documentation only, at the user's request at session end):** this entry;
"Unresolved decisions" D-07 row; `PROJECT_DECISIONS.md` header line, D-07 pending note, §9 D-07
row; PHASE_0_PLAN pending note; SESSION_HANDOFF. **D-07 text unchanged (v6); v6.1 not applied;
no status changed; no Git state changed; nothing staged, committed, backed up or executed** (only
read-only Git commands). Session 9 closed here at the user's request (conversation too long) —
continue from `SESSION_HANDOFF.md`.

## Session 10 — 2026-10-13

### 24. D-07 clarifications, Session 10 additions and Variant A/B diffs — documentation only, D-07 still Proposed
The user granted **no approval**: no file edit (until the session-end recording below), staging,
commit, branch, backup, config change or step 0.1; D-07 stays Proposed. All repository commands
were read-only (`git status`, `git log`, reading docs). The proposed diffs were shown **in chat
only**; **v6.1 is not applied** — the D-07 text in `PROJECT_DECISIONS.md` is still v6. At session
end the user asked to record the session in the docs (this entry).

**Round 10a — clarifications requested by the user (answered):**
1. *F1/F3 clause list.* F1 = §22 items 1–7 (Reach, `* -text`, Conventions, generation 1, B2, B4,
   policy (4)/(5)). F3 = §22 item 8 "Fixed commands" bullet, the phrase "with the fixed commands"
   in item 10 (check (a)), and item 12 (rule 4). **Dependency found:** F2's `CHANGES.txt` bullet
   ("the union of the three path lists") relies on the path lists defined in F3's "Fixed commands"
   bullet; without F3 that phrase is undefined.
2. *Documentation scope.* (A) changes only: `PROJECT_DECISIONS.md` (D-07 body clauses per §22/§23,
   D-07 heading text, the "Pending v6.1" note, header session line, §9 D-07 row; status stays
   Proposed); `DECISION_LOG.md` (new entry + "Unresolved" D-07 row); `PHASE_0_PLAN.md` (pending
   note line 18 only); `SESSION_HANDOFF.md` (header, session bullet, §6, §8). (A) does **not**
   touch D-06, D-07 status, `CLAUDE.md`, PHASE_0_PLAN rules/step rows or any §21 impact-list item.
   (B) = the whole §21 impact list. **Consistency note for (B):** §21 item 4 (`CLAUDE.md` branching
   line "byte-for-byte `.gitattributes`") must be worded consistently with F1 if F1 is accepted.
3. *T4 failure during step 0.1.* Rule 3 → backup not verified → stop and ask; no commit, stash,
   switch, reset, clean, restore, re-stage, `gc`/prune; nothing deleted. State at that point:
   received files never modified (B3); authored changes only `.gitattributes` + `*.user` line
   (content fully specified in D-07); 153 staged entries in the real index (unreferenced blobs —
   reason `gc`/prune is forbidden); `develop` and `feature/phase-0-gate-1` at `af89144`; the
   verification repository lives only in `verify/`. Options afterwards (each needs approval):
   retry into a new timestamped folder; amend D-07 if the procedure is wrong; proceed without a
   verified backup; abandon the step. **Not claimed:** T4 level (c) has never run on the real
   repository (Session 8 scratch tests only; unverified list in §22).
   **Gap found:** T4 backups do **not** copy the 409 received files (untouched as-received content
   is excluded from `A`; only SHA-256 in `inventory/`; `history.bundle` holds commits only) →
   Round 10b item 4.
   **Ambiguity found:** v6 "then `<backup-root>/CURRENT.txt` is updated" does not say whether this
   happens before or after verification → Round 10b item 5.
4. *Backup-root minimum requirements* (no path chosen, nothing probed). Mandatory (D-07 text):
   outside the repository working tree and `.git`; outside the session scratchpad; named
   explicitly by the user with explicit write authorization; persistent; no delete/restore without
   approval. To confirm (§23 checklist, partly unverified): local non-sync folder; short path
   (Windows path length, not measured); enough disk space (not measured); same-drive risk (repo on
   `H:`); filesystem can hold the received paths byte-exactly.

**Round 10b — the user's review direction (not approval):**
1. F1: inclined to accept, **if** it states explicitly that attribute checks and hash verification
   apply to this repository/environment only, not a byte-identical checkout guarantee elsewhere.
2. F3: inclined to accept, **if** it does not claim the fixed flags neutralize all Git
   configuration, and explains verifiably which configuration effects check (c) detects.
3. F2/F4: keep the final wording accepted in §23.
4. Received files: documentation-only recommendation for the data-loss risk when no independent
   copy of the 409 files exists before step 0.1; no copy made now.
5. `CURRENT.txt`: never updated as verified before verification succeeds; rule for failed backup
   folders.
6. Show unified diffs of Variant A (F1+F2+F3+F4) and Variant B (F2+F4) in chat.

**Proposed wording (Session 10 additions; shown in chat, not applied).** Variant A = §22 items
1–13 with §23's final F2 (c)4 case 2 and F4 Isolation sentence, **plus** the following. Tags:
[F1+], [F3+], [R], [C].

*[F1+] new paragraph in T1, after "No B-check is reported as proving a committed state, and no
C-check is run or claimed before the commit exists.":*
> **Scope of these checks (environment).** B1–B6, C1–C4 and policy (4) describe this repository on
> this machine — its working tree, index, object store, Git version, configuration and attribute
> sources — at the moment each check runs, and nothing else. C2 proves that the committed blobs are
> byte-identical to the received files; because a blob's id is derived from its bytes, every intact
> clone that contains the commit holds the same blob bytes. The checks do **not** prove that
> checking out those blobs in another clone, on another machine, with another Git version, or after
> any attribute-source or configuration change writes byte-identical working-tree files. That would
> need its own check in that environment (e.g. SHA-256 of the checked-out files against the
> `L_disk` manifest), which is not part of this decision.

*[F3+] replaces the last sentence of the §22 item 8 "Fixed commands" bullet ("Other diff output
configuration not neutralized by `P` … is detected only by check (c)."):*
> `P` overrides only the options it names. It does **not** neutralize every Git configuration
> that can affect `git diff` output or `git apply` (e.g. `diff.suppressBlankEmpty`,
> `diff.context`, `apply.whitespace`; not tested). Check (a) cannot detect such an effect,
> because regeneration uses the same configuration. Check (c) detects any effect that makes a
> patch fail to apply or changes the content it reproduces: (c)2 compares the tree rebuilt from
> `staged.patch` with `T_staged`, which `git write-tree` computes from the real index
> independently of diff configuration; (c)4 compares each reconstructed tracked blob with
> `git hash-object` of its byte copy in `files/`, also independent of diff output; (c)3 compares
> the set of unstaged paths with `CHANGES.txt`. A detected effect fails (c) → not verified →
> rule 3. Not detected: an effect that changes patch bytes without changing the reproduced
> content (harmless for restoring), and effects on files outside (c) (`STATE.md`, ignored
> paths). Scratch-tested (Session 8): `color.ui`, `color.diff`, `diff.noprefix`,
> `diff.mnemonicPrefix`, `diff.relative`, running from a subdirectory. The statements about (c)
> follow from which values each comparison uses; they are not tested for every configuration.

*[R] new paragraph in T4, after "Location." (optional; user decision):*
> **Received files (not covered by T4 backups).** A T4 backup copies only the authored change set
> `A`; the 409 as-received `L_disk` files are recorded only by SHA-256 in `inventory/`, and
> `history.bundle` contains commits only. Before step 0.1's commit the working tree is the only copy
> of these files; after it the 151 staged files also exist as committed blobs in the same local
> repository (same disk, nothing pushed), and the 258 ignored files exist only in the working tree.
> Independent copy — **Pending (user decision, before step 0.1)**, one of:
> (R1) the user's original source of `DefaultTemplate/` and `knowledge Docs/` (e.g. the archive or
> download they came from), kept outside the repository and confirmed by comparing its SHA-256 list
> with `L_disk`;
> (R2) a byte copy of all 409 `L_disk` files under `<backup-root>/received_<yyyymmdd-hhmmss>/`,
> made only after its own explicit approval, after `L_disk` is first recorded (Phase 0 mapping 1)
> and before anything is created or staged; verified only when the copy's SHA-256 list equals
> `L_disk` byte-for-byte (T1 comparison rules). Its result is recorded like a T4 folder
> (`RESULT.txt`); it is never modified, deleted or restored from without explicit approval;
> (R3) no independent copy — only by the user's explicit decision, recorded with the risk: loss or
> corruption of the working tree (disk failure, accidental deletion, a process outside the
> assistant's control) before the commit loses the received files; after it, the 258 ignored files
> (regenerable build output, not byte-identical) remain at risk, and the committed blobs share the
> repository's disk.
> Recommended: (R1) if the original source exists, otherwise (R2).

Plus Phase 0 mapping item 1, appended: "then, only if (R2) was approved, make and verify the
received-files copy (T4 "Received files") before anything else is created or staged;". Size of
the 409-file copy not measured.

*[C] replaces v6 "then `<backup-root>/CURRENT.txt` is updated to name the folder.":*
> **Result and `CURRENT.txt`.** After the checks below, `RESULT.txt` is written in the folder:
> `verified`, `patch-checked`, or `not verified: <check>` with the failed check, its output and the
> time. `verify/` and `RESULT.txt` are created after `MANIFEST.sha256` and are not listed in it.
> `<backup-root>/CURRENT.txt` names only the latest **verified** folder: it is written only after
> (a), (b) and (c) have all passed for that folder, as the last write of the procedure, and never
> for a patch-checked or failed folder; until the first verified backup it does not exist.
> **A failed or patch-checked folder** is kept exactly as written — never deleted, overwritten,
> reused, completed or "repaired" without explicit approval; a retry writes a new timestamped
> folder, and `CURRENT.txt` keeps naming the previous verified folder, if any. The failed folder's
> path and result are reported to the user and recorded per rule 2.

[C] consequential edits: check (a) first sentence → "every backup file except `verify/` and
`RESULT.txt` is listed in `MANIFEST.sha256` and matches it" (closes a v6 gap: `verify/` is created
after the manifest); rule 4 → "… against the backup named in `<backup-root>/CURRENT.txt` (none
exists → stop and report); …" (Variant A keeps "regenerated with the fixed commands").

*Status edits (both variants):* D-07 heading → "Proposed (wording v6.1: Session 7 v6 + Session
8–10 corrections, awaiting the user's approval)" (Variant B: "wording v6 + F2/F4 corrections");
the "Pending v6.1 corrections" note → an "applied (Session 10, documentation approval (A))" note;
header session line; §9 D-07 row (v6.1; received-files R1/R2/R3 before 0.1; approval order
(B)–(F)); PHASE_0_PLAN line 18 note.

**Variant B (F2 + F4 only).** Identical to Variant A for: F2 Renames, F2 (c)4, F4 Isolation, [C],
[R]. Dropped: all F1 items and [F1+]; F3 "Fixed commands" bullet and [F3+]. T1 Reach, `* -text`,
Conventions, generation 1, B2, B4, policy (4)/(5) and the v6 `staged.patch`/`unstaged.patch`
bullets stay as in v6. Two **new wordings needed** (otherwise the text is inconsistent; need
approval):
1. T4 Contents, first line: "All Git commands of the backup procedure that read the real
   repository run as `git --no-optional-locks …` from the repository root (required by
   "Isolation")." — F4's accepted Isolation names `--no-optional-locks`, the v6 commands lack it.
2. F2 `CHANGES.txt` first sentence uses the v6 path definition instead of "the union of the three
   path lists": "the authored change set `A`: every path in `staged.patch`, in `unstaged.patch`,
   and every untracked, non-ignored authored file (`git ls-files -o --exclude-standard`), excluding
   untouched as-received untracked content (checked against the T1 manifest instead)." The rest of
   F2's bullet is unchanged.
Check (a) without "with the fixed commands"; rule 4 with [C] only. The D-07 note in Variant B
states as binding limitations: v6 Reach / `* -text` claims ("any machine or clone", "checkout
writes them back unchanged") are **not guarantees**; B4/C2 prove storage in this repository at
check time only; B4 checks only `text` (`filter`, `ident`, `working-tree-encoding`,
`$GIT_DIR/info/attributes`, subdirectory `.gitattributes` unchecked); patches depend on diff
configuration (e.g. `color.ui=always`, `diff.noprefix`) — (a) cannot detect it, (c) fails → not
verified → stop (false failure, not silent loss).

**Not decided:** (A) and which variant; F1/F3 acceptance (direction only); [R] option R1/R2/R3;
[C] wording; Variant B's two new wordings. Records-file diffs (DECISION_LOG/SESSION_HANDOFF) for
(A) are shown in full only when (A) is applied.

**Done in this session (documentation only, at the user's request at session end):** this entry;
"Unresolved decisions" D-07 row; `PROJECT_DECISIONS.md` header line, D-07 pending note, §9 D-07
row; PHASE_0_PLAN pending note; SESSION_HANDOFF. **D-07 text unchanged (v6); v6.1 not applied; no
status changed; no Git state changed; nothing staged, committed, backed up or executed.**
Pre-edit snapshot in the Session 10 scratchpad (not persistent). Session 10 closed here at the
user's request (conversation too long) — continue from `SESSION_HANDOFF.md`.

## Session 11 — 2026-10-13

### 25. Read-only technical review of Variant A (B4, (c)3, manifest/RESULT, R2 failure) — documentation only, D-07 still Proposed
The user treated Variant A as preferred but gave **no documentation approval (A)**; no file edit,
stage, branch, backup, config change or step 0.1 during the review. Real repository: read-only
only (`status`, `config --show-origin`, `ls-files -v/-o`, `check-attr`, all
`--no-optional-locks`). Mechanisms were tested in throwaway repositories in the Session 11
scratchpad (not persistent), Git 2.52.0.windows.1; no received file content was copied for these
tests (B4 used only the real path **names**, since attributes are path-based).

**1. B4 (`check-attr -z --stdin`).** Real repo, current state (no `.gitattributes`): 155 paths
(152 candidates under `F` + `.gitignore`, `LICENSE`, `.gitattributes`) → 2325 NUL fields = 155 × 5
triplets `<path>\0<attr>\0<value>\0`, in input order, attributes in command order; all
`unspecified`. Scratch, with the proposed `.gitattributes` and the real path names: `.gitignore`,
`LICENSE` → `text: set`; the other 153 → `text: unset`; `eol`, `filter`, `ident`,
`working-tree-encoding` → `unspecified` for all — **expected values correct**. Gaps: (B1) input
`/LICENSE` (pattern form) → `fatal`, exit 128, but inside a pipe the output is empty and the
pipeline exits 0 → the wording must require exit status 0, empty stderr and the triplet count;
(B2) run from a subdirectory, the same string names another path silently → root-only must be
stated in B4; (B3) `check-attr` reads the working-tree `.gitattributes`, `--cached` the index
one (scratch: `set` vs `unset`) → compare both. Fail-safe (scratch): legacy `crlf` has no byte
effect under `-text`; `eol=crlf` + `-text` keeps bytes but is not `unspecified` → stop (false
stop, no loss); `info/attributes` / subdirectory `text`/`ident` detected.
Proposed B4 wording (replaces §22 item 6):
> - **B4 Staged blobs:** for every `L_staged` path, SHA-256(`git cat-file blob :<path>`) = its
>   `L_disk` value; blob id = `git hash-object --no-filters -- <path>` (no `-w`). Attributes: from
>   the repository root, `git --no-optional-locks check-attr -z --stdin text eol filter ident
>   working-tree-encoding`, input = the `L_staged` paths, then `.gitattributes`, `.gitignore`,
>   `LICENSE` (repository-relative, no leading `/`), NUL-separated; output saved in `inventory/`.
>   Valid only if the exit status is 0, stderr is empty, and the output consists of exactly 5
>   triplets `<path>\0<attribute>\0<value>\0` per input path, paths byte-identical to the input,
>   in input order, attributes in the order above. Expected: `text` = `set` for `.gitignore` and
>   `LICENSE`, `unset` for every other path; `eol`, `filter`, `ident`, `working-tree-encoding` =
>   `unspecified` for all. The same command with `--cached` gives byte-identical output. Any other
>   value, count, order, error or difference → stop. These results describe this clone's
>   attribute sources at the time of the check only.

**2. (c)3 and multi-state paths.** Scratch, all in one repository: staged A + unstaged M, staged
M + unstaged M (case 1); staged A + unstaged D, staged M + unstaged D (case 2); staged D + file
kept (`rm --cached`), staged D + file reappearing with other content (case 3); unstaged M only —
(c)2 tree matched and (c)3 `diff-index` equalled the unstaged list exactly. Gaps:
- **G1 (silent loss, undetected):** changes to `assume-unchanged` (`h`) / `skip-worktree` (`S`)
  files appear in no list → not in `A`, not backed up. Real repo now: `ls-files -v` = 2 × `H`.
- **G2 (silent loss):** an untracked authored file ignored by `core.excludesFile` or
  `.git/info/exclude` is not in `A`; nothing detects it.
- **G3 (inconsistency):** staged D with the file present but now ignored is in the staged list,
  not the untracked list; (c)4 case 3 says "ignored or not → copy", Limitations say "authored
  change under an ignored path → not verified".
- **G4 (fail-safe, unnamed):** `git add -N` → unstaged `A` (outside M/D/T); unmerged → `U`,
  `write-tree` fails. Both stop today via "any other combination".
- **G5:** (c)3 must compare path **and** status letter.
- **G6 (config):** `diff.context=0` → apply fails (false failure); `--unified=3` in `P` restores a
  byte-identical patch. `apply.whitespace=fix` → tree mismatch (caught by (c)2); `--whitespace=nowarn`
  neutralizes it. `diff.suppressBlankEmpty=true` → applies, tree correct (harmless; now
  scratch-tested, [F3+] said "not tested").
Proposed wording: `P` += `--unified=3`; path-list commands also take `--no-relative`; every
`git apply` in (b)/(c) runs with `--whitespace=nowarn`. *Preconditions (backup time and resume),
each → not verified if violated:* `git --no-optional-locks ls-files -v -z` shows only tag `H` (no
sparse checkout); no unmerged entry; no intent-to-add entry; `git --no-optional-locks ls-files -o
-i --exclude-standard --directory -z`, each entry checked with `git check-ignore -v -z --no-index
--stdin`, matches only the root `.gitignore` patterns `[Bb]in/`, `[Oo]bj/`, `*.user` — any other
source or pattern is reported and stops. (c)3: "… path and status letter". Limitations, first
item: "an authored change under an ignored path that appears in none of the three path lists; a
staged deletion whose working-tree file is ignored is in `A` (staged list) and is handled by (c)4
case 3."

**3. `MANIFEST.sha256` / `verify/` / `RESULT.txt`.** M1: the manifest cannot list itself →
exclusions `MANIFEST.sha256`, `RESULT.txt`, `verify/`. M2 (scratch): `sha256sum -c --strict`
passes with an extra file → set equality both ways needed; Git Bash writes `<hash> *./<path>`
(binary marker) → format must be fixed; 0 backslashes in real paths. M3: (a) regeneration output
location undefined. M4: (b) needs the verification repository created in (c)1 → order. M5:
manifest not re-checked after (c). M6: `RESULT.txt` has no integrity binding; folder without
`RESULT.txt` undefined; `CURRENT.txt` holding only a name cannot detect a bad pointer. M7:
"patch-checked" vs "not verified: (c)N" unclear. M8: step 0.1's `inventory/` lives in the
step-start folder, but retries and Step workflow 4 create new folders. **M9: the §24 [R] claim
"T4 backups do not copy the received files" is inaccurate** — `A` is the union of the three lists
and the exclusion covers only untracked as-received content, so step 0.1's review-stop backup
holds the 151 staged received files (`staged.patch` + `files/`; (c)2 requires it). The real gap is
from step start to the first verified backup, plus the 258 ignored files.
Proposed wording:
> **Manifest.** `MANIFEST.sha256` is written last among the backup artifacts, from the folder
> root, as `sha256sum` output over every regular file except `MANIFEST.sha256`, `RESULT.txt` and
> anything under `verify/`, paths `./`-relative, sorted with `LC_ALL=C`. Check (a):
> `sha256sum -c --strict` passes **and** the set of files in the folder (same exclusions) equals
> the set listed. **Order:** artifacts → `MANIFEST.sha256` → (a) (regenerated outputs only under
> `verify/regen/`, compared with `cmp`) → (c)1 → (b) → (c)2–4 → manifest check repeated →
> `RESULT.txt` → `CURRENT.txt` (verified only). **`RESULT.txt`:** `verified` or `not verified`,
> then one line per check (a), (b), (c)1–4: `pass` / `fail: <output>` / `not run`; a folder
> without it is not verified; `verify/` is evidence only, never a restore source.
> **`CURRENT.txt`:** folder name + SHA-256 of its `MANIFEST.sha256` and `RESULT.txt`; rule 4
> checks both; mismatch, missing folder or no `CURRENT.txt` → stop. Resume never writes into an
> existing folder. **Inventory (step 0.1):** written once to
> `<backup-root>/0.1-inventory_<yyyymmdd-hhmmss>/` with its own manifest; each later step-0.1
> backup's `STATE.md` names it and its manifest SHA-256. **[R], first sentences:** "A T4 backup
> copies the authored change set `A`, which at step 0.1's review stop includes the 151 staged
> received files. Not covered by any T4 backup: the 409 `L_disk` files from the start of step 0.1
> until its first verified backup, and the 258 ignored files at all times …"

**4. R2 failure (proposed wording).**
> **R2 failure.** R2 fails if any part cannot be completed or checked (folder not creatable, a file
> not readable/copyable/writable, the copy's list differs from `L_disk` in any path, size or
> SHA-256, `RESULT.txt` not writable). Then at once: (1) **stop** — step 0.1 does not continue: no
> branch, no `.gitattributes`/`.gitignore` change, no staging, **no commit**; (2) the
> `received_<ts>/` folder is kept exactly as written — never deleted, overwritten, completed,
> re-copied into or repaired; `RESULT.txt` (if writable) records `not verified: <check>`; a
> folder without `RESULT.txt` is not verified; (3) report folder path, failed check, affected
> paths (expected vs actual), free disk space; record per rule 2; (4) **retry only after explicit
> approval**, into a **new** `received_<yyyymmdd-hhmmss>/`; `L_disk` re-recorded first and
> byte-identical to the first recording, else stop; deleting a failed folder needs separate
> approval; (5) alternatives (each needs approval): R1, R3 with recorded risk, amend D-07, abandon
> the step. `L_nonregular` entries and empty directories are not copied by R2; reported, not
> failures.

**Evidence classification.** (a) logic only: B4 wording; G3, G5; M1, M3–M9; item 4 entirely.
(b) scratch-tested: B4 format/order/expected values/edge cases; all multi-state combos, G1, G2,
G4, G6; M2. (c) real repository: only B4 parsing on the current state (no `.gitattributes`);
**untested** there: B4 expected values after `.gitattributes`, full (c), the new preconditions,
the manifest/RESULT procedure, R2.

**Status.** None of these wordings is applied to D-07 (text still v6); nothing approved. The
assistant also recommended (not decided): reduce scope — R1/independent copy instead of the R2
procedure, apply only the material findings (G1, G2, M1, M9) with (A)+(B), then step 0.1.

### 26. Independent backup of the received folders (user instruction, 2026-10-13) — executed and verified
**Instruction (user):** create an independent, timestamped backup of `DefaultTemplate/` and
`knowledge Docs/` outside the repository, verify it (paths, counts, sizes, SHA-256), keep the
originals unchanged, no Git change, no step 0.1; stop on failure without repair. Afterwards the
user confirmed that **the same drive `H:` is acceptable** and asked to record it. This is **not**
D-07's formal R2 procedure (D-07 still Proposed); it is a separate backup made on the user's
direct instruction.

**Location:** `H:/Internship Project/DLMS-Backups/received_20261013-152404/` (new folder; parent
`DLMS-Backups/` created then, nothing pre-existing). Layout: `files/DefaultTemplate/`,
`files/knowledge Docs/` (copied with `cp -r --preserve=timestamps`, exit 0, empty stderr);
`_verification/` = `original_before.sha256/.sizes`, `original_after.sha256/.sizes`,
`backup.sha256/.sizes`, directory and empty-directory lists, `backup_nonregular.txt`,
`copy_errors.txt` (empty), `RESULT.txt`.

**Verification (all passed):** 409 files = 409; 51,345,608 bytes = 51,345,608; sorted
path + SHA-256 lists byte-identical; path + size lists identical; 110 directories identical;
1 empty directory present in both (`DefaultTemplate/DefaultTemplate/obj/Debug/net8.0/staticwebassets`);
0 non-regular entries in either; no missing, extra, mismatched or unreadable file;
`sha256sum -c --strict` of the original manifest inside the backup passes; archive SHA-256
`6826f819…a86b` (D-05) matches; originals unchanged (SHA-256 and sizes before copy = after).
Git after the backup: `main`, HEAD `af89144`, only `main`, no stash, index hash unchanged
(`ls-files -s` SHA-256 `cc7be7f6…053b`), same four untracked items. `RESULT.txt`: `verified`.

**Not decided by this:** whether `H:/Internship Project/DLMS-Backups/` becomes D-07 T4's
`<backup-root>` (approval (D) — needs explicit confirmation), and the formal [R] choice inside
D-07 (this copy makes R1-like protection available; R2 as a procedure is unneeded unless the
user wants it). Same-drive risk accepted by the user.

### 27. Progress check — distance to coding (user question, Session 11) — no decision
**User asked:** how far the project is from starting coding.
**Answer given (facts):** no application code changed yet; Phase 0 not started; repository still
`af89144`. Sessions 6–11 were spent on D-07 (Git workflow, line endings, backups). Before step
0.1: approvals (A) documentation, (B) D-07, [R] (now practically covered by §26), (D)
`<backup-root>`, (E) step 0.1 start, (F) staged tree hash + message. First code change is step
0.3 (restructure, no behaviour change); real code Gate 2 (0.6–0.8) and Gate 3 (0.9–0.12); library
features (books, members, issue/return) only in Phase 1.
**Assistant's recommendation (not decided):** the D-07 backup/verification procedure has grown
heavier than step 0.1 needs; take R1-like protection (done via §26), apply only the material
findings (G1, G2, M1, M9) together with (A)+(B), stop further review rounds, and let the T4
procedure prove itself at its first real run in step 0.1. Estimated: 0.1–0.2 within 1–2 sessions,
then 0.3. The user's response was to order the independent backup (§26); the rest is open.

**Session 11 closed here** at the user's request (conversation too long) — continue from
`SESSION_HANDOFF.md`.

## Session 12 — 2026-10-13

### 28. Approvals (A) and (D) — v6.1 applied to D-07 (still Proposed); `<backup-root>` named
**Asked (assistant, from the Session 11 open items):** (1) is `H:/Internship Project/DLMS-Backups/`
the D-07 T4 `<backup-root>` — approval (D); (2) which §25 corrections go into Variant A —
recommendation G1, G2, M1, M9. **User:** "my permission is yes for this" (2026-10-13).
**Recorded as:**
- **(D) approved:** `<backup-root>` = `H:/Internship Project/DLMS-Backups/`, write authorization
  for T4 backup folders; same-drive risk already accepted (§26). Does not authorize step 0.1.
- **(A) approved:** documentation approval of Variant A with G1, G2, M1, M9, applied to the D-07
  entry in `PROJECT_DECISIONS.md` = §22 items 1–13, §23 final F2 (c)4 second case and F4
  Isolation sentence, §24 [F1+], [F3+], [R], [C] (+ consequential edits: check (a) exclusions,
  rule 4 `CURRENT.txt`), Phase 0 mapping item 1 R2 clause; §25 G1 and G2 as T4 "Preconditions"
  (only the `ls-files -v` and ignored-source checks; unmerged / intent-to-add (G4) not added),
  M1 (`MANIFEST.sha256` excludes itself, `RESULT.txt`, `verify/` in check (a)), M9 ([R] first
  sentences: step 0.1's review-stop backup holds the 151 staged received files; gap = step start
  to first verified backup + 258 ignored files). [R] also notes the §26 copy as R1-like protection.
- **Not applied (dropped by this choice):** §25 B4 wording (B1–B3), G3–G6, M2–M8, R2-failure
  wording. Known residual risks stay as reported in §25 (e.g. B1: a pipe can hide a `check-attr`
  fatal error; G6 `diff.context=0` → false failure; M2 extra-file not caught by `sha256sum -c`).
- **Not approved:** (B) D-07 itself, D-06 partial supersession, impact list, T1–T5; [R] choice
  R1/R2/R3; (C) T1-EOL; (E) step 0.1; (F). The user's pasted note said (A)+(B) together lead to
  step 0.1 — (B) is not inferred from "yes"; asked separately.
**Done:** D-07 heading → "wording v6.1", pending note → applied note, body clauses as above;
`PROJECT_DECISIONS.md` header line + §9 D-07 / T4 rows; `PHASE_0_PLAN.md` note; this entry +
Unresolved rows; `SESSION_HANDOFF.md`. Diff against a Session 12 scratchpad snapshot (481 lines,
not persistent). No Git change, nothing written to `<backup-root>`, nothing executed.
**Open question put to the user (unanswered at session end):** approve (B) D-07 v6.1 + D-06
partial supersession + §21 impact list + T1–T5, and [R] = R1 (the §26 copy)?

**Session 12 closed here** at the user's request (conversation too long) — continue from
`SESSION_HANDOFF.md`.

## Session 13 — 2026-10-13

### 29. R1 backup verification, senior review package, approval — process simplified
**R1 (user approved):** the Session 11 backup is the recovery source; no new copy. Read-only
verification: `RESULT.txt` = `verified`; `original_before`/`original_after`/`backup` manifests
byte-identical; `sha256sum -c --strict` inside `files/` exit 0 (409); actual file set = manifest
paths (409), sizes match (51 345 608 bytes), 110 dirs / 1 empty dir / 0 non-regular match; the
**current** originals re-hashed = manifest (empty diff); archive `6826f819…a86b` in both.
Limitations: timestamps/ACLs not compared; manifests not self-bound (M6) — mitigated by the
independent re-hash of the originals.
**Review (user asked for a 20+-year senior view):** twelve sessions without a commit; D-07 had
grown into ceremony out of proportion to a single-developer local repository. Real risks were
only (1) `core.autocrlf` conversion of 29 received files — fixed by `.gitattributes`, and (2) loss
of the received files — covered by R1. Found one real safety gap: base `appsettings.json` still
points at `Database=DefaultTemplate` and startup runs `MigrateAsync()`; overriding only in
`appsettings.Development.json` leaves non-Development environments migrating the template DB.
**Approved by the user ("ok approve"), items 1–13:** (1) D-07 v7 simplified (v6.1 Superseded;
D-06 branch clause and M8 §4 (2)–(4) partially superseded); (2) T1 `.gitattributes` + step 0.1
hash check; (3) R1 recovery source, optional gate-end `git bundle`; (4) T1-EOL = LF; (5) D-47
Rejected, `CseLibrary.*` kept; (6) `global.json` + local `dotnet-ef`; (7) D-40: `CseLibrary` on
`.\SQLEXPRESS`, base + Development config fixed, admin password in user-secrets; (8) Q-M9-11
yes; (9) step 0.2 slimmed (docs commit, archive `git mv`, README, `architecture.md`; other docs
when they have content; archive FR/BR traceability per phase start); (10) Q-M9-8 keep offset
paging; (11) D-14 `ReadCommitted` → Phase 3; (12) Q-M9-10 all pages authenticated except Login,
AccessDenied, Error; (13) D-16 Approved. The approval also authorizes running step 0.1 and Gate 1
with review before each commit and a stop at the Gate 1 report.
**User addition (same session):** integrate review tooling — recorded in D-07 v7 as the gate
checklist: `/code-review` + `/ponytail-review` on each gate diff; `archify` diagram in 0.2.
**Left to their gates (code review, no discussion round):** D-13, D-15, D-34, D-14 assessment,
D-31/D-32, D-37 cleanup, D-21/24/25/27, D-29/D-30.
**Architect re-check before acting (user asked again):** no further material issue; the only
addition was the step 0.1 hash check against the R1 manifest (replaces B1–B6/C1–C4).
**Executed (Session 13):** docs updated (PROJECT_DECISIONS header, D-06 note, D-07 v7 inserted,
v6.1 heading → Superseded, D-16, D-40, D-41, D-47, §9; PHASE_0_PLAN rules + rows 0.1–0.4;
SESSION_HANDOFF top block; this entry). Step 0.1: `develop` and `feature/phase-0-gate-1` created
from `main`; `.gitattributes` written; `*.user` appended to `.gitignore` (CRLF in working copy, as
the file was); `git add` of `.gitattributes`, `.gitignore`, `DefaultTemplate`, `knowledge Docs`
→ 153 paths; `check-attr text`: `.gitignore`/`LICENSE` set, received files unset; staged blobs
SHA-256 = R1 manifest minus `bin/`/`obj/`/`*.user` (151/151, PASS); tree
`b48a82b62f167454f77f2ae567ef8263414ea3b7`. Environment note: Python is not installed (use
Git Bash tools).
**Not done:** the commit — commit message shown, user's commit approval not yet given.
**Session 13 closed here** at the user's request (conversation too long) — continue from
`SESSION_HANDOFF.md` "Next action (Session 14)".

## Session 14 — 2026-10-13

### 30. Step 0.1 committed; step 0.2 (slimmed) prepared
- Pre-commit: branch `feature/phase-0-gate-1`, `git write-tree` = `b48a82b6…a3b7` (match),
  153 staged paths. User: "OK Proceed" → commit **`1fdbe5e`** "Add received template and
  requirements baseline (step 0.1)". Post-commit: only `CLAUDE.md`, `docs/` untracked;
  `git ls-files --eol`: 29 received files `i/crlf` (unchanged), `.gitignore` `i/lf attr/text`.
- Step 0.2: `git mv "knowledge Docs/Requirement/1st discussion.md" "docs/archive/1st discussion.md"`
  (SHA-256 `6826f819…a86b` unchanged); `README.md`; `docs/architecture.md` (target vs current,
  links to D-10/D-12/D-13/D-15/D-17 instead of copying them); `archify` diagram
  `docs/diagrams/template-architecture.{json,html}` from code at `1fdbe5e` (finalize: validate,
  deliver, check, browser-check all pass); `CLAUDE.md` doc map paths. No new decisions.
- Per D-07 v7 the step 0.2 commit waits for the user's OK on the staged list + message.
  User: "ok" → commit **`9cf511a`**.

### 31. Step 0.3 — rename to `CseLibrary.*` (no behaviour change)
- Directory `git mv` failed ("Permission denied": VS Code C# Dev Kit holds the folders) → tracked
  files moved one by one with `git mv`; ignored `bin/`, `obj/`, `*.csproj.user` stay in the old
  `DefaultTemplate/` folder (not deleted — needs user approval).
- **Incident:** while project folders were half-moved, C# Dev Kit **rewrote two `.csproj` files on
  its own** (removed `ProjectReference`s it saw as missing). Detected in `git diff`, restored from
  the index. Lesson: after moves, always diff `.csproj` files before staging.
- **Incident:** Git Bash `sed -i` stripped CR from CRLF files (1551-line diff) → reverted from index,
  redone with `sed -b` on matching files only (177 lines). Byte-level line endings preserved.
- Done: 4 projects → `src/CseLibrary.{Web,Services,Data,Core}`, `.csproj` names, namespaces/usings
  (incl. migrations, Views `@using`), `CseLibrary.sln` at root with `src\` paths (project GUIDs
  kept), `.dockerignore` → root, Dockerfile paths/entry DLL, `_Layout` `~/CseLibrary.Web.styles.css`
  (CSS isolation bundle follows the assembly name), template README → `docs/archive/`.
  `UserSecretsId` kept. `global.json` (`10.0.100`, `latestFeature`); `.config/dotnet-tools.json`
  with `dotnet-ef` 10.0.12 (SDK 10 created the manifest at the root — moved to `.config/`).
- Not changed: visible "DefaultTemplate" brand text in `_Layout` (UI change, not 0.3);
  `Database=DefaultTemplate` (step 0.4). `dotnet ef migrations list` **not** run (would connect to
  the configured DB) — deferred to 0.5 against `CseLibrary_Test_*`.
- `dotnet build CseLibrary.sln`: succeeded, 0 errors, 2 warnings (NU1903 AutoMapper — step 0.6).
  User: "ok" → commit **`3c8d98a`**.

### 32. Step 0.4 — connection strings and .NET 10 (in progress, not committed)
- `dotnet user-secrets list` → none; no `ConnectionStrings*` environment variables. Base
  `appsettings.json` and `appsettings.Development.json` → `Database=CseLibrary` on `.\SQLEXPRESS`.
- Read-only `sys.databases` on `.\SQLEXPRESS`: only the 4 system databases — **no `DefaultTemplate`,
  no `CseLibrary`**; first start creates `CseLibrary` (D-40 Approved).
- net8.0 → net10.0 (4 projects); EF Core / Identity / Configuration.Abstractions 10.0.12;
  Serilog.AspNetCore 10.0.0; **compatibility:** Serilog.AspNetCore 10 requires
  Serilog.Sinks.File ≥ 7 (NU1605 downgrade error) → Sinks.File 6.0.0 → 7.0.0. Dockerfile images
  10.0. Build: 0 errors; warnings NU1903 (AutoMapper, 0.6) and **NU1510** (Data references
  `Microsoft.Extensions.Configuration.Abstractions`, now part of the shared framework — removal
  suggested, not done: reported first).
- `dotnet ef migrations has-pending-model-changes` (no DB connection): "No changes have been made
  to the model since the last migration" → EF 10 will not throw `PendingModelChangesWarning` at
  `MigrateAsync`. Side observation: `Program.cs` catch-all logs EF design-time `HostAbortedException`
  as Fatal (template defect; step 0.10).
- **Open:** app start against `CseLibrary` needs the seed admin password in user-secrets (D-40:
  user-secrets, never committed) — set by the user.

### 33. M15 — new user requirements (Session 14) — recorded, not yet designed
User asked (Bangla, summarized):
1. An `archify` diagram of the **overall project architecture** (target system, not only the
   template) so docs can be understood visually.
2. An `archify` **database model / table-relationship** diagram, to be seen and understood
   **before** implementation.
3. A title can have **multiple physical copies**; issuing one copy reduces the title's available
   count by 1, and the database records **who holds that copy**.
4. Full lending history: who borrowed which copy, when, and when it was returned.
5. DB design quality "from a 20-year senior architect's perspective".
Assessment: 3–4 agree with the archive spec (FR-03, Book/BookCopy, accession number, BR-03/04/16,
D-35 filtered unique index) — no conflict. Design proposal (Proposed, needs approval) and diagrams
pending.
**User choices (Session 14):** M15 work starts **after the Gate 1 report**.
**Seed password (Session 14, user):** "no DB username/password — just connect to SQL Server simply."
The connection already uses Windows authentication (`Trusted_Connection=True`, no SQL login). The
seed password is the *app* admin login, which `DbSeeder` requires; the user does not want to set
one now. → 0.4 is committed on build + offline evidence; the **app-start check moves to step 0.5**,
where `WebApplicationFactory` supplies a test-only seed password in memory against LocalDB
`CseLibrary_Test_*` (no secret on disk, `.\SQLEXPRESS` untouched). Nothing is created on
`.\SQLEXPRESS` in Gate 1.
User: "ok" → step 0.4 commit **`468a82c`**.

### 34. Step 0.5 — test harness and characterization tests
- `tests/CseLibrary.Tests` (net10.0; xUnit 2.9.3, runner 4.0.1, Test.Sdk 18.10.1,
  Mvc.Testing 10.0.12), added to `CseLibrary.sln` (`dotnet sln add` also rewrote the solution's
  configuration list and added solution folders — harmless). `Program.cs`: `public partial class Program;`.
- `TestDatabase` (D-40): LocalDB `CseLibrary_Test_{guid}`; `EnsureSafe` requires server
  `(localdb)\MSSQLLocalDB` **and** the prefix (case-sensitive), checked before startup migration and
  before drop; drop targets only the generated name (`QUOTENAME`, parameterized).
- `AppFactory`: one shared host per run (Serilog bootstrap logger freezes once per process, D-43
  risk); connection string + test seed password via **process environment variables** (override
  every appsettings file) and `UseSetting`; `SqlCommandCounter` via EF `ConfigureDbContext`.
- Tests (15): app starts on the test DB with all migrations applied and migration ID
  `20260508164255_InitialCreate` intact (the 0.4 start check and the 0.3 migration check); H1 guard
  (5 refusals + generated-name accept/drop); login ok / wrong password; anonymous → Login;
  GeneralConfig create/edit/delete; Users list **+1 SQL command per user** (N+1, D-16);
  login writes permission claims to `AspNetUserClaims` (D-37 defect); permission granted → works on
  the next request; revoke behaviour (below).
- **Finding (corrects the 0.5 plan wording "stale-claim defect"):** `RefreshSignInAsync` is a no-op
  inside the login request on .NET 8+ (user not yet authenticated), and the cookie principal is
  built from `AspNetUserClaims` *before* login rewrites them. So each cookie carries the
  permissions written by the **previous** login: a revoked permission is denied on the current
  cookie (handler's DB fallback), **allowed again after the next login**, denied after the one
  after. Characterized as such; fixed by D-37 in 0.9 (tests A7, A9).
- `dotnet test`: 15/15 passed twice; LocalDB holds no `CseLibrary*` DB afterwards; `.\SQLEXPRESS`
  still only the 4 system DBs. Build warnings: NU1903 (AutoMapper, 0.6), NU1510 (§32).
  User: "ok" → commit **`22d27ae`**.

### 35. Gate 1 review (D-07 v7 tooling) — findings reported, nothing fixed
Diff reviewed: `1fdbe5e..HEAD` (authored changes; the received baseline is excluded).
**`/code-review` (medium), 10 findings:**
1. `Program.cs` catch-all swallows startup exceptions (exit code 0; EF `HostAbortedException` logged
   Fatal; test host loses the real cause) — fix `when (ex is not HostAbortedException)` + rethrow (0.10).
2. Test guard validates only the generated string, not the connection string the app resolves before
   `MigrateAsync` — move the check to the resolved DbContext options before seeding.
3. Process env vars set by `AppFactory` are never restored.
4. `UseSetting` connection string reportedly ineffective (eager config read) — dead second path.
5. `Guard_accepts_the_generated_name_and_drop_targets_only_it` asserts nothing meaningful about Drop.
6. No sweep of `CseLibrary_Test_*` databases left by aborted runs.
7. Dockerfile restores before copying the referenced csproj files (template pattern, kept in 0.3).
8. `CseLibrary.sln` has an empty `src` solution folder (added by `dotnet sln add`).
9. README status still says net8.0.
10. N+1 test asserts an exact command count — may flake (retry, stamp validation).
**`/ponytail-review`:** delete `UseSetting` line; drop the sync `WithService`; keep only async
interceptor overrides; remove `Configuration.Abstractions` (NU1510) and direct `Serilog.Sinks.File`;
`EF Tools` package only needed for VS PMC — net −21 lines possible.
Proposed handling (needs the user's choice): fix 2, 3, 4, 5, 8, 9, 10 and the ponytail items as one
correction commit on the gate branch (D-07 v7 allows it); 1 → step 0.10; 6 → optional; 7 → with
the Docker work, not Phase 0.

### 36. Session 14 close-out (user request: switch session)
User: "this tab has too much discussion — write down the decisions, discussion and findings in the
existing docs." Updated (uncommitted, awaiting OK): `SESSION_HANDOFF.md` (Session 14 block rewritten
as the Session 15 start: state, next actions, user preferences, environment gotchas),
`PROJECT_DECISIONS.md` (header Session 14 line, archive path, D-37 correction, D-40/D-41/D-42/D-43
execution notes, §9 "Session 14" open table), `requirements.md` (FR-M15-01..04), `PHASE_0_PLAN.md`
(0.5 [x], Gate 1 status), `README.md` (finding 9: net8.0 → .NET 10 — doc-only fix), this log.
No code changed after `22d27ae`.

### 37. Session 15 — Gate 1 review findings fixed (user: "YES", then "ok" per commit)
Commits: `346496a` (Session 14 close-out docs), `8504115` (correction).
**Fixed:** 2 — guard moved to `AppFactory.GuardDbContext` (`ConfigureDbContext` after the app's
`UseSqlServer`): checks the connection string the app's DbContext resolves, before seeding opens a
connection; new test runs the real `AddRepositoryLayer` with `.\SQLEXPRESS` and expects the refusal
without connecting (mutation-checked: fails with the guard line removed). 3 — env vars restored in
`finally` on dispose. 4 — `UseSetting` removed (env var is the single path). 5 — Drop test creates a
real `CseLibrary_Test_*` DB, drops it, asserts the app DB survives. 8 — empty `src` solution folder
removed. 10 — N+1 asserts `>= before + 3`. Ponytail: sync `WithService`, `Configuration.Abstractions`
(NU1510 gone), direct `Serilog.Sinks.File` (same 7.0.0 comes via `Serilog.AspNetCore`) removed.
**Not done (verified):** sync interceptor overrides kept — `UserService.GetAllAsync` runs sync
`Users.ToList()` on the Users page, which the counter must see; `EF Tools` package kept (Visual Studio
PMC). 1 → 0.10; 6 optional; 7 → Docker work; 9 fixed in `346496a`.
**Correction-diff review:** `/code-review` 9 findings — fixed 4 (try/finally in Drop test, `EnsureSafe`
before test `CREATE DATABASE`, assert guard message, env restore in `finally`); rejected 5 (single
collection so no env race; per-scope recheck cost negligible; exact N+1 count reverts the agreed
flake fix; Sinks.File version claim wrong; constructor check only re-validated the generated string).
`/ponytail-review`: lean already.
**Evidence:** `dotnet build CseLibrary.sln` 0 errors (warnings: NU1903 AutoMapper only);
`dotnet test CseLibrary.sln` 16/16; 0 `CseLibrary*` DBs left on LocalDB; `.\SQLEXPRESS` untouched.

### 38. Session 15 discussion and close-out
- **Input:** the user pasted another assistant's recommendation — fix findings 1(guard)/3/4/5/10 now,
  verify-then-remove `UseSetting` and packages, `Program.cs` catch-all → 0.10, Dockerfile/README with
  their own work, M15 after the Gate 1 correction and before Gate 2. Assistant agreed, adding finding 8
  (one-line `.sln` fix) and noting 9 was already fixed and 6 is optional. **User: "YES".**
- **Order agreed and executed:** commit Session 14 docs (`346496a`, OK) → correction + build/test →
  `/code-review` + `/ponytail-review` on the correction diff → commit (`8504115`, OK) → evidence docs
  (`97891e8`, OK). Details and evidence: §37.
- **M15 timing (confirmed):** after Gate 1 correction/approval, before Gate 2.
- **Gate 1 merge:** asked as a separate approval; the user's final "ok" was applied to the doc commit
  only. **Merge to `develop` still awaits explicit approval.**
- Session closed at the user's request (conversation too long); handoff: SESSION_HANDOFF Session 15.

### 39. Session 16 — Gate 1 approved and merged
- Session 15 close-out docs committed on the user's "ok" (`8a6df00`).
- Asked separately for Gate 1 approval + merge; **user: "ok"** → `feature/phase-0-gate-1` merged into
  `develop` `--no-ff` (`bbee8dc`, 9 commits). `main` unchanged; nothing pushed.
- Evidence on `develop`: `dotnet build CseLibrary.sln` 0 errors (6 warnings, all NU1903 AutoMapper
  12.0.1); `dotnet test CseLibrary.sln` 16/16; `sqlcmd` on LocalDB → 0 `CseLibrary*` DBs; `.\SQLEXPRESS`
  untouched.
- User order: update docs first, then M15 (§33). M15 output stays **Proposed**; no code/migration.
- Docs commit `fe960d9` on the user's "ok".

### 40. M15 delivered for review (Session 16) — Proposed, nothing implemented
- `docs/diagrams/target-architecture.{json,html}`: whole system, existing parts cite code at
  `fe960d9`, future parts cite PROJECT_DECISIONS / archive lines.
- `docs/diagrams/database-model.{json,html}`: tables as `database` nodes with labelled 1 — N links.
- Both pass archify `finalize` (validate, deliver, strict check, browser-check); one DB-diagram
  crossing removed by moving `AspNetUsers`; no perceptual (screenshot) review done.
- `docs/database.md` + **D-49 (Proposed)**: derived available count (no counter), open-loan
  invariant (filtered unique index + copy `RowVersion`), append-only history (`CHECK`s, FK
  `NO ACTION`, no delete), indexes; open choices O1–O5 with recommendations. Departs from the
  archive only in recommendation O3 (single `Members` table instead of subtype tables), which is
  flagged for a Phase 2 decision.
- Waiting for the user's review; Gate 2 needs its own approval.
- Committed `bdd5261` on the user's "ok".

### 41. Session 16 — ER diagram, naming, audit columns, record status
- User could not read the table design (opened the `.html` in the IDE; the archify overview has no
  columns). Added `docs/diagrams/database-tables.html`: hand-built SVG ER diagram, every column,
  PK/FK/UQ, FK → PK lines in crow's-foot notation, click-to-highlight, relationships in words.
  Rendered with headless Edge and inspected.
- **User:** PK should be plain `Id`, FK `<Table>Id`; keep Created/Modified By/At on tables for
  first-line troubleshooting. Assistant agreed, refined: singular entity name in FKs, role prefix
  for two FKs to one table, columns already exist in the template `BaseEntity`, set them in an
  interceptor, log tables exempt, last-change only (full history = `AuditLogs`). Naming question
  `Updated…` vs `Modified…` → **user: "option A" (keep `Updated…`)** → **D-50 Approved**.
- **User:** replace `IsActive` + `IsDeleted` with one `Status` enum (e.g. Active = 200, Deleted = 404),
  update the template Data layer accordingly, keep it extensible. Assistant recommended 1/2/3 over
  HTTP codes (meaning, `tinyint`, 0 = unset), an `Inactive` state, separation from business statuses,
  one global filter, central soft delete, filtered unique indexes; implementation in Gate 2.
  **User: "I like your suggested solution. so go for it"** → **D-51 Approved**; done in step 0.8.
  Gate 2 itself still needs explicit start approval.
- Overview diagram (`database-model`) sublabels aligned with D-50/D-51 (no `IsActive`); re-passes
  archify `finalize`.

### 42. Session 16 close-out (user request: switch session)
- Commits this session: `8a6df00`, `bbee8dc` (Gate 1 merge), `fe960d9`, `bdd5261` — each on the
  user's "ok". §41 work + this close-out are **staged, not committed** (commit on the user's OK).
- Open for Session 17: Gate 2 start approval (not given — "go for it" covered D-50/D-51 only);
  O1–O5 in `database.md`; items in PROJECT_DECISIONS §9 "Session 14"; deleting the merged
  `feature/phase-0-gate-1` branch.
- Handoff: SESSION_HANDOFF Session 16 block.

### 43. Session 17 — old `DefaultTemplate/` folder deleted
- User asked why the folder is kept. Verified: 0 tracked files; only ignored `bin/`, `obj/` (~42 MB),
  `DefaultTemplate.csproj.user` (IIS Express debug profile) and empty source directories. The
  original stays in Git history and in `H:/Internship Project/DLMS-Backups/received_20261013-152404/`.
- **User: "if there is no issue then delete these and commit after deleting these"** → folder
  deleted. The `DefaultTemplate` database on `.\SQLEXPRESS` is unrelated and untouched.
- Committed `371c43b` (docs) on the user's request.

### 44. Session 17 — Gate 2 started; step 0.6 (AutoMapper removed)
- Assistant asked explicitly for the Gate 2 start (and separately for deleting the merged
  `feature/phase-0-gate-1` branch). **User: "ok"** → per the Session 15 rule ("one ok = one
  approval") taken as the **Gate 2 start only**; branch deletion **not** approved, still open.
- Branch `feature/phase-0-gate-2` created from `develop` (`371c43b`).
- 0.6: AutoMapper packages + `MappingProfile` removed; explicit `ToDto()` extensions (D-17). Build 0
  errors / 0 warnings (NU1903 gone), no vulnerable or transitive AutoMapper package, tests 16/16.
  Staged list + message shown → **user "ok"** → commit `51e189a`.
- Lesson: Git Bash `sed -i` rewrites CRLF files as LF (the repo stores bytes as-is, `* -text`); after
  every scripted edit restore CRLF on files that had it, and check `git diff --stat` for whole-file
  rewrites.

### 45. Session 17 — step 0.7 signatures approved; contracts added
- Assistant showed exact signatures (Result/Result<T>, ErrorCodes, PageRequest/PagedResult,
  ConcurrencyConflictException/DuplicateKeyException, slim `IRepository<T>`, feature repositories,
  `IUnitOfWork` with one generic `ExecuteInTransactionAsync<TResult> where TResult : Result`) and five
  points to approve: (1) `Result` shape (D-34), (2) page size default 20 / max 100, admin tables
  unpaged (D-15 details), (3) read-only `Roles` + write-side `RolePermissions`, user permission lookup
  on `Permissions`, (4) one generic transaction method, (5) commit split so every commit builds.
  **User: "ok"** → all five Approved.
- While implementing: all repository interfaces and the new `IUnitOfWork` moved to 0.8 (the new
  `IRepository<T>` cannot coexist with the old one of the same name); 0.7 = Result, ErrorCodes,
  paging, exceptions + 9 unit tests (D2 clamp). Build 0/0, tests 25/25. **User "ok"** → `48a9acf`.

### 46. Session 17 — step 0.8 implemented and committed
- Repositories with DTO projections; `Repository<T>` write base (`GetForUpdateAsync`, `AddAsync`,
  `Remove`); `UnitOfWork` with lazily created typed repositories, no `IDisposable`, no
  `Repository<T>()`; `SaveChangesAsync` translates concurrency → `ConcurrencyConflictException`,
  SQL 2601/2627 → `DuplicateKeyException`; `ExecuteInTransactionAsync` inside the execution strategy
  (ChangeTracker.Clear per attempt, nested call → `InvalidOperationException`, failed Result →
  rollback + clear, **success → pending changes saved automatically, then commit**).
- Services inject only `IUnitOfWork` (+ `UserManager`/`RoleManager`); no EF queries; Services project
  references only Core + `Microsoft.Extensions.Identity.Core` (D-10). Service APIs return
  `Result`/`Result<T>` (tuples gone); user/role create and update run in one transaction (D-14, Q8).
- D-16: Users list = count + one page query with roles joined (Q1, no `PasswordHash`), Users index
  paged (20, Previous/Next); roles projected (Q2); user permission lookup = one join (Q3); edit pages
  read projections (Q7); UoW no longer disposes the DbContext (Q9). Q4 (claims on login) stays 0.9.
- D-50/D-51: `RecordStatus` enum; `BaseEntity.Status` + `ApplicationUser.Status` replace `IsDeleted`
  / `IsActive`; one global `Status <> Deleted` filter for every `BaseEntity`; `CHECK (Status IN (1,2,3))`
  incl. `AspNetUsers`; unique indexes filtered `[Status] <> 3`; `AuditColumnsInterceptor` sets
  `CreatedAt/By`, `UpdatedAt/By` (users/roles: `CreatedAt`, users `UpdatedAt`) and turns `Remove()`
  into soft delete; login requires `Active`. `RolePermissions.AssignedAt` kept (ER diagram: template
  table structure unchanged).
- `ICurrentUser { string? UserId }` (Core) + `HttpCurrentUser` (Web) added now because D-50 needs the
  current user; the wider D-21 design of `ICurrentUser` stays Proposed. `TimeProvider.System` registered.
- Removed as unused: `IRoleService.GetPermissionNamesForRoleAsync`, `IUserService.SetActiveAsync`,
  `IRoleReadRepository.GetPermissionNamesAsync` (from the approved signature), `DtoMappings`.
- Migration `20261013142332_RecordStatusAndAuditColumns`, **hand-edited**: the scaffold dropped
  `IsDeleted`/`IsActive` before reading them and added `Status` default 0 (violates the CHECK). Now:
  add nullable `Status` → convert (`IsDeleted=1 → 3` else 1; `IsActive=0 → 2` else 1) → NOT NULL +
  CHECK → drop old columns → filtered unique indexes; Down converts back.
- Tests (44): D1 (same SQL command count for N and N+45 users; roles; page boundaries), D3 (1 command),
  D4 (no `PasswordHash`/`SecurityStamp` in SQL), E1/E3/E4 (architecture, assemblies + csproj),
  E2 (one context for UoW and Identity stores), F1 (exception after Identity saved → nothing left),
  F2 (simulated transient failure → re-run from fresh state, one row), F3, F4, G1 (duplicate +
  concurrency), soft delete + audit columns through HTTP, inactive login denied, migration up/down
  on pre-existing rows in its own `CseLibrary_Test_*` DB. The template N+1 characterization test was
  replaced by D1.
- Evidence: build 0 errors / 0 warnings; tests 44/44 twice; `has-pending-model-changes` = none; no
  vulnerable packages; 0 `CseLibrary*` DBs left on LocalDB; `.\SQLEXPRESS` untouched.
  **User "ok"** → commit `0d01e4e`.

### 47. Session 17 — Gate 2 reviews (`develop..HEAD`) — fixed in Session 18 (§48)
`/code-review` (9 findings, no verify pass) and `/ponytail-review` ran on the gate diff.
| # | Finding | Assessment | Recommendation |
|---|---|---|---|
| CR1 | `ExecuteInTransactionAsync` re-runs work after an ambiguous commit (connection lost after commit) → role/user create reports "name already taken" although it succeeded | Real; the D-14 rule 3 risk | Fix now: `verifySucceeded`-style check for role/user create |
| CR2 | Deactivated user keeps access with an existing cookie; permission lookup ignores `Status` | Real, from the template | Cookie part = step 0.9 (D-32/D-37); filter `Status = Active` in the permission lookup now |
| CR3 | Concurrent duplicate role/user name → `DuplicateKeyException` uncaught → HTTP 500 | Real | Fix now with the shared helper (CR8) |
| CR4 | Role create/update accepts deleted or non-existent permission ids (FK 547 untranslated → 500) | Real | Fix now: keep only existing permission ids |
| CR5 | Interceptor sets a deleted entry `Unchanged` → other pending edits lost; tracked cascade dependents hard-deleted | Partly; no current code path hits it | Fix now: keep the entry `Modified`, change only `Status` |
| CR6 | `ApplicationUser.UpdatedAt` also changes on Identity-internal writes (failed login, lockout) | Design question; D-50 says "last change" | Keep; document |
| CR7 | Page number above `TotalPages` shows an empty page | Real, minor | Fix now: clamp to the last page |
| CR8 | Duplicate-catch `SaveAsync` copied in two services; missing in Role/User services | Same as ponytail P2 | Fix now: one shared helper |
| CR9 | Seeder never restores a soft-deleted built-in permission | Real, from the template | Fix now: seeder re-activates it |
| P1 | `ErrorCodes.Concurrency` unused | — | Delete |
| P2 | `SaveAsync` duplicated (= CR8) | — | Shared helper |
| P3 | `ArchitectureTests` L46 generic-type ternary | — | Shrink |
Assistant recommendation: one correction commit for CR1, CR3, CR4, CR5, CR7, CR8, CR9, the
permission-lookup part of CR2 and P1–P3; CR2 cookie part → 0.9; CR6 kept. Then docs + the D-14
per-use-case duplicate-operation assessment + Gate 2 report. **Not yet answered** — the user asked to
switch sessions (conversation too long); this log, the handoff and the plan were updated (Session 17
close-out, not committed).

### 48. Session 18 — review fixes committed; D-14 rule 3 use-case assessment
- Close-out docs: **user "ok"** → commit `6470649`.
- User answered "choose 2" = the §47 recommendation as proposed. **User "ok"** → commit `8cd5f53`
  (16 files):
  - CR1: `ExecuteInTransactionAsync(work, verifySucceeded = null, ct)` — optional callback (EF's
    `verifySucceeded`); after a failed commit it is asked only for the attempt whose commit was sent,
    true returns that attempt's result instead of re-running. Role/user create pass "row with the
    generated id exists". Signature change of the approved 0.7 method (one optional parameter, still
    one method).
  - CR3/CR8/P2: `Services/DuplicateKeyExtensions.cs` — `Task<Result>.OnDuplicate(failure)` +
    `IUnitOfWork.SaveAsync`; used by GeneralConfig/Permission create+update, Role create+update, User
    create. A duplicate past the pre-check is `ErrorCodes.Duplicate`, not HTTP 500.
  - CR4: `IPermissionRepository.GetExistingIdsAsync` — role create/update keep only existing,
    non-deleted ids (update therefore also drops links to deleted permissions).
  - CR5: soft delete sets the entry `Modified` (other pending edits kept). Cascade part **not** changed:
    a tracked non-`BaseEntity` dependent of a soft-deleted principal is still hard-deleted; no current
    code path does this.
  - CR7: `PageRequest.ClampTo(total)` — past-the-end page shows the last page.
  - CR9: seeder restores the latest deleted built-in permission when no live row has the name.
  - CR2 (part): `GetNamesForUserAsync` returns nothing for a user who is not `Active`; cookie part → 0.9.
  - P1 `ErrorCodes.Concurrency` deleted; P3 ternary removed. CR6 kept: `ApplicationUser.UpdatedAt`
    means "last write of the row", including Identity-internal writes (failed login, lockout).
  - Tests 44 → 49: ambiguous commit verified not re-run (mutation-checked: with `verifySucceeded`
    returning false the old duplicate error comes back), duplicate role name past the pre-check,
    unknown/deleted permission ids dropped, soft delete keeps pending edit, seeder restores permission;
    CR7 and CR2 asserted in existing tests.
  - Evidence: build 0 errors / 0 warnings; tests 49/49 twice; `has-pending-model-changes` = none;
    0 `CseLibrary*` DBs on LocalDB; `.\SQLEXPRESS` untouched.

**D-14 rule 3 — duplicate-operation assessment (Phase 0 admin use cases).** Situation: the commit
reaches SQL Server but the answer is lost, and EF's retry strategy runs the operation again. A single
`SaveChanges` is re-sent as is; `ExecuteInTransactionAsync` re-runs the whole delegate.

| Use case | Write path | Risk if repeated | Protection | Test | Remaining limitation |
|---|---|---|---|---|---|
| Config create | 1 SaveChanges | Second INSERT | Filtered unique index `Key` | G1, soft-delete/key-reuse test | User may see "Key already exists" although the row was created |
| Config update | 1 SaveChanges | Same UPDATE again | Idempotent (absolute values, no concurrency token) | — (analysis) | None known |
| Config delete | 1 SaveChanges (soft) | `Status = 3` again | Idempotent | soft-delete test | None known |
| Permission create / update / delete | same as Config | same | Filtered unique index `Name`; idempotent update/delete | CR3 helper shared; G1 | Same misleading-message risk on create |
| Role create | Transaction (RoleManager + RolePermissions) | Delegate re-run → "name taken" | `verifySucceeded` (role id exists) + unique `RoleNameIndex` | Ambiguous-commit test at UoW level | Role/user wiring of the callback is not separately fault-injected |
| Role update | Transaction | Delegate re-run | Idempotent: role reloaded and desired permission set recomputed inside the delegate | F-tests (rollback/retry) | None known |
| Role delete | 1 SaveChanges (RoleManager, `ConcurrencyStamp`) | DELETE matches 0 rows | Data correct (row already gone) | — (analysis) | User may see Identity "optimistic concurrency failure" although deleted |
| User create | Transaction (UserManager + roles) | Delegate re-run → "name taken" | `verifySucceeded` (user id exists) + unique `UserNameIndex` | as Role create | Email has no DB unique index (`RequireUniqueEmail` is app-level): two simultaneous creates with one email can both succeed |
| User update | Transaction | Delegate re-run | Idempotent: user reloaded inside the delegate | F1 | None known |
| User delete / reset password | 1 SaveChanges (UserManager, `ConcurrencyStamp`) | UPDATE/DELETE matches 0 rows | Data correct | — (analysis) | User may see a concurrency failure although it succeeded |

Not claimed: that every ambiguous-commit case is proven. Only the transaction path is
fault-injected; single-SaveChanges rows are analysis. The worst remaining outcome is a misleading
error message, not wrong data, except duplicate email (a race, not a retry), which needs a
filtered unique index on `NormalizedEmail` — a schema decision for the user (proposal, not done).
- Docs: **user "ok"** → commit `d779249`.

### 49. Session 18 — Gate 2 approved and merged
- Gate 2 report given; **user "ok"** = Gate 2 approved. Separate **user "ok"** → merge
  `feature/phase-0-gate-2` → `develop` `--no-ff` = `798ccbd`.
- Evidence on `develop` after the merge: build 0 errors / 0 warnings; tests 49/49; 0 `CseLibrary*` DBs
  on LocalDB; `.\SQLEXPRESS` untouched.
- Not answered: email unique-index proposal (§48) stays open. `feature/phase-0-gate-2` kept (deleting
  needs the user's OK). Gate 3 (0.9–0.12) not started; it needs its own approval.
- Docs: **user "ok"** → commit `4c288a5`.

### 50. Session 18 — branch per task, unique email (D-52), merged branches deleted
- **User (standing rule):** every new feature or task starts on its own branch from `develop` and is
  merged back into `develop` when it is finished. Extends D-07 beyond gate branches.
- **User:** create the unique email index — members log in by email + password (→ **D-52 Approved**).
  User asked about an index on the password too; assistant advised against it: only a salted hash is
  stored (equal passwords give different hashes, so a unique index protects nothing) and login looks
  the user up by email, then verifies the hash.
- **User:** "do what you think is good" for old branches → `feature/phase-0-gate-1` and
  `feature/phase-0-gate-2` deleted with `git branch -d` (refuses unmerged branches; both are in
  `develop` via `bbee8dc` / `798ccbd`).
- Work on `feature/email-unique-index`: `EmailIndex` unique + filtered; migration `UniqueUserEmail`
  (scaffolded, no data step — test DBs only; never applied to `.\SQLEXPRESS`); `UserService` create and
  update map a duplicate to "User name or email is already taken."; test: two users with one email →
  `DuplicateKeyException`.
- Evidence: build 0 errors / 0 warnings; tests 50/50 twice; no pending model changes; 0
  `CseLibrary*` DBs on LocalDB.
- **User "ok"** → commit `70bad0f`; merged → `develop` `--no-ff` = `e9a0536` (standing rule); on
  `develop`: build 0/0, tests 50/50, 0 test DBs left. `feature/email-unique-index` deleted (`-d`).

### 51. Session 18 — close-out
- User asked to switch sessions (conversation too long) and to record this session's decisions,
  discussion and findings in the existing docs. Close-out done on branch `docs/session-18-closeout`
  (standing rule §50): SESSION_HANDOFF Session 18 block rewritten (state, decisions, next action,
  lessons), this log, PROJECT_DECISIONS (D-52 commit, open-decision table), PHASE_0_PLAN Gate 3 note.
- Process lesson: a bare "ok" after several questions was taken as approval of the first pending
  action only (docs commit, then Gate 2, then merge, then the next commit) — each asked again.
- Next: Gate 3 (0.9–0.12) start needs the user's explicit approval.

### 52. Session 19 — Gate 3 start; 0.9 security decisions (2026-10-14)
- Verified: Session 18 close-out already committed (`408b625`) and merged (`5bb43bc`); branch
  deleted. User "ok" → Gate 3 started; branch `feature/phase-0-gate-3` created from `develop`.
- Assistant presented the 0.9 plan (three commits: D-37 DB-only permissions, D-31 baseline,
  D-32 deactivation) and four questions: D-31 values, D-31 M9 approach + Q-M9-10, D-32 mechanism,
  D-37 claim-row cleanup. Recommendation for Q-M9-10 was "authenticated"; the user chose otherwise.
- **User decisions (each recorded as stated, nothing else upgraded):**
  1. **0.9 access design — Approved** (D-31 M9 extension; supersedes the Session 13 Q-M9-10
     answer): `Home.Index` public — CSE Library landing page with Login and Registration links
     top-right; `Home.Privacy` public; Login and Registration anonymous; everything else needs
     authentication + the appropriate authorization/permission checks; `FallbackPolicy` +
     class-level `[Authorize]` applied consistently, `[AllowAnonymous]` only on the intended
     public endpoints; public Registration must not grant privileged roles automatically.
  2. **D-31 password + lockout values — Approved:** min length 10, one digit, lockout 5 failures /
     15 min, `lockoutOnFailure: true`. Stamp interval, cookie settings, global antiforgery stay
     Proposed.
  3. **D-32 mechanism — Approved:** inactive user cannot sign in again; deactivation calls
     `UpdateSecurityStampAsync`; existing-cookie validation and protected-page access verified by
     automated tests. Explicitly approves no other security setting.
- Not answered: D-37 cleanup of existing `AspNetUserClaims` permission rows (still needs separate
  approval; assistant recommends deferring — the new handler will not read them).
- **New open questions from decision 1:** Q-S19-1 registration scope (no registration endpoint
  exists; who may register, what the account gets, link to member record, staff activation,
  phase — note the archive's "Student Registration" is a staff form, not self-registration);
  Q-S19-2 whether `Error` / status-code / `AccessDenied` stay anonymous (recommended: yes for
  `Error` + status-code page).
- Documentation-only change (PROJECT_DECISIONS, this log, SESSION_HANDOFF); no code, Git state,
  staging or commits changed.

### 53. Session 19 — D-37 fix, user retention, registration, notifications (2026-10-14)
User sent five requests in one message:
1. **D-37 (both tasks).** Inspection: the only legacy claim type is `Permission`
   (`Permissions.ClaimType`), written by the old `AccountController.AddPermissionClaimsAsync`
   (value = permission name); no other code writes `AspNetUserClaims`. **Authorization fix
   implemented** (user request; uncommitted): handler ignores cookie permission claims and checks
   the DB per request (request-local cache); login writes no claims; `IPermissionService` removed
   from `AccountController`. Tests: A9 flipped (login writes no claims), A7 flipped (revoked
   permission denied on the next request **and** after re-login), new A7 legacy-row test (a
   `Permission` row in `AspNetUserClaims` reaches the cookie but grants nothing), new A8 (removed
   from role → denied next request). Build 0/0, tests 52/52. Mutation check: re-adding the
   cookie-claim shortcut fails the legacy-row test.
   **Cleanup — reported, not run:** criteria `ClaimType = N'Permission'` exact, case-sensitive;
   every other claim type kept; no `AspNetUsers` row touched. Target database not chosen: the
   `CseLibrary` dev DB on `.\SQLEXPRESS` has never been migrated by this project (Session 18
   evidence), the user's `DefaultTemplate` DB is off-limits (CLAUDE.md), test DBs are disposable.
   Row count needs a read-only `SELECT COUNT(*)` on the chosen DB — not run without approval.
   Rollback: copy matching rows to a backup table inside the same transaction before the
   `DELETE`; restore with `SET IDENTITY_INSERT` + `INSERT … SELECT`. Since the handler ignores the
   rows, the cleanup is hygiene only (rows still ride along in the cookie until removed).
2. **User retention → D-53 (Approved requirement).** Findings: `UserService.DeleteAsync` →
   `UserManager.DeleteAsync` physically deletes the user (+ cascade of roles, claims, logins,
   tokens); no interceptor safety net (`ApplicationUser` is not a `BaseEntity`); `UpdateAsync`
   could resurrect a deleted user; deleted users would show in the Users list; existing cookie
   needs a stamp update; `RoleService.DeleteAsync` also hard-deletes (out of scope). Code not
   changed — user asked for findings first.
3. **Registration design → D-54** (Q-S19-1 answered): Student → Active immediately; Dept. Fellow
   → Pending, Librarian approves/rejects; Pending/Rejected/Deleted cannot log in; no privileged
   roles from the registration type. Status-model gap recorded (approval ≠ `RecordStatus`,
   Q-S19-8); open details Q-S19-3 … Q-S19-9, Q-S19-17.
4. **Separate registration workflows with shared logic** (added to D-54 item 5): explicit
   Student and Dept. Fellow workflow methods, shared internals reused, workflow rules separate,
   no Fellow special rules invented now. Design decision, not implementation approval.
5. **UI notifications and error handling → D-55.** Reconciled with D-31 access design, Q-S19-2,
   D-34 and test A2; open Q-S19-10 … Q-S19-14. Also recorded D-52 conflict (Q-S19-16).
- Docs updated: PROJECT_DECISIONS (D-37 note, D-52 note, D-53–D-55, section 9), this log,
  PHASE_0_PLAN (0.9 / 0.10 notes), SESSION_HANDOFF. No database, migration, branch, staging or
  commit change.

### 54. Session 19 — close-out
- User asked to switch sessions (conversation too long) and to record this session's decisions,
  discussion and findings in the existing docs. SESSION_HANDOFF Session 19 block rewritten
  (state table of uncommitted files, decisions, findings, ordered next action, lessons), this
  log, PROJECT_DECISIONS section 9 note.
- Nothing committed this session: the D-37 fix (3 files) and all docs are uncommitted on
  `feature/phase-0-gate-3`; the user has not yet reviewed the §53 report. Close-out made in the
  working tree (no close-out branch: switching branches would carry the uncommitted code and
  was not approved).
- Next: Session 20 starts with the review of §53, then separate approvals (D-37 commit, cleanup
  target DB + count, D-53 implementation, open values).

### 55. Session 20 — Q-S19-2 … Q-S19-17 answered (2026-10-14)
- Verified first: branch `feature/phase-0-gate-3`, 7 files uncommitted as in §54, build 0 errors,
  tests 52/52.
- Assistant listed Q-S19-2 … Q-S19-17 with recommendations. **User approved (recorded as stated):**
  - **D-54:** Q-S19-3 necessary identity + credential fields, email confirmation *to be
    considered* (not decided); Q-S19-4 Librarian approval → Fellow `Active`, subject to other
    security checks; Q-S19-5 validate + link an existing `Member`, no automatic duplicate;
    Q-S19-6 dedicated `Registrations.Review` permission; Q-S19-7 rejected applicant may re-apply,
    no duplicate account/application; Q-S19-8 approval state separate from `RecordStatus`;
    Q-S19-9 outside Phase 0; Q-S19-17 System Admin-configured default member role, no privileged
    permission. Earlier D-54 items (Student → Active, Fellow → Pending → Librarian review,
    separate workflow methods, no physical `AspNetUsers` delete) confirmed.
  - **D-55:** Q-S19-10 safe local return location else Home with toast, AJAX/API 401/403, open
    redirect prevented; Q-S19-11 Reference ID in error toast; Q-S19-12 warning ≈ 5 s, info ≈ 3 s,
    max 3, pause on hover/focus; Q-S19-13 verify existing code, Bootstrap toast + current
    `TempData` keys; Q-S19-14 step 0.10 with test A2 update.
  - **D-53/D-52:** Q-S19-15 deleted users hidden by default, visible from an authorized admin
    view when needed; Q-S19-16 email of a deleted user not reusable for now, unique index stays.
  - **Q-S19-2:** generic error/status responses work for anonymous requests; protected-resource
    authorization unchanged.
- **Explicitly not approved by this message** (each still a separate gate): D-37 commit (staged
  list + exact message first); D-37 cleanup (confirm target DB → read-only count → affected rows
  + rollback plan → separate delete approval); D-53 code changes (execution approval before any
  edit); D-31 stamp interval, cookie settings, global antiforgery (show values compared with the
  current configuration — the Session 19 approval covered only password/lockout).
- Docs updated: PROJECT_DECISIONS (D-31, D-53, D-54, D-55, section 9), this log, PHASE_0_PLAN
  note, SESSION_HANDOFF. No code, database, staging or commit change.
- Then, each on its own "ok": D-37 fix committed `fab1af1` (3 files); docs committed `9234229`.
  Local `.claude/settings.local.json` allows `git add/commit/status` (auto-mode classifier had
  blocked the approved commit); excluded via `.git/info/exclude`.
- **D-37 cleanup closed:** user approved target `CseLibrary` on `.\SQLEXPRESS` + read-only check.
  `SELECT COUNT(*) FROM sys.databases WHERE name = N'CseLibrary'` → `DESKTOP-OIKPIIF\SQLEXPRESS 0`.
  The database does not exist, so the claim count was not run and nothing needs deleting.
- **D-53 implemented** (user "ok" to start code; commit needs its own ok): soft delete via
  `Status = Deleted` + `UpdateSecurityStampAsync`; deleted user = not found for update/delete/reset
  password; hidden from Users list + details. Separate commit from D-32 (smaller review). New test
  `Deleted_user_is_kept_but_locked_out_and_hidden`; build 0/0; tests 53/53; mutation check passed.
  Committed `8550c00` (code + docs, user "ok").
- **D-31 remaining values Approved** (user "ok" after a current-vs-proposed table): stamp interval
  5 min (from config), cookie `Secure = Always` + explicit `HttpOnly`/`SameSite = Lax`, 8 h sliding
  kept, global `AutoValidateAntiforgeryToken`. Values only — D-31 baseline code not yet approved.
- **D-31 baseline implemented** (user "ok" to start code; commit needs its own ok): Identity
  password/lockout bound from `appsettings` `Identity`; `Security:StampValidationMinutes` = 5;
  cookie Secure/HttpOnly/SameSite=Lax; global antiforgery; `FallbackPolicy` = authenticated;
  class `[Authorize]` on all 6 controllers; `[AllowAnonymous]` on Home Index/Privacy/Error,
  Account Login/AccessDenied. Assistant's defaults (user told to object if needed): no Registration
  link or landing-page redesign yet (Registration outside Phase 0), AccessDenied stays until D-55
  (0.10), Logout authenticated. Test passwords lengthened to 11 chars. New tests: allowlist (A1),
  lockout (A5), password policy from config, cookie/stamp/antiforgery settings; 64/64; mutation
  checks (lockout off, binding removed, Index not anonymous) each fail.
  Committed `215d75e` (14 files incl. docs, user "ok").
- Proposed next (not approved): D-32 code — custom `SignInManager.CanSignInAsync`, stamp update
  on deactivation in `UserService.UpdateAsync`, test A4 with a test-only validation interval 0.

### 56. Session 20 — close-out
- User asked to switch sessions (conversation too long) and to record this session's decisions,
  discussion and findings in the existing docs. Updated: SESSION_HANDOFF (Session 20 block
  rewritten), this log, PROJECT_DECISIONS (commit hashes for D-31/D-53, D-32 note, section 9
  "Session 21 start" row), PHASE_0_PLAN (0.9 progress note).
- Branch `feature/phase-0-gate-3`: 4 commits on top of `develop` @ `5bb43bc` — `fab1af1` (D-37),
  `9234229` (docs), `8550c00` (D-53), `215d75e` (D-31). Not merged; Gate 3 not reported.
- Findings worth keeping:
  1. The auto-mode permission classifier can block an approved `git commit`; the user granted
     `Bash(git add:*)`, `Bash(git commit:*)`, `Bash(git status:*)` in `.claude/settings.local.json`
     (local, excluded from Git). Other git/read commands may still be classified.
  2. Python is not installed on this machine — edit docs with the Edit tool or `sed`.
  3. Identity's defaults (6 chars, symbol, upper case) would hide a broken config binding if a
     test only checks a password that fails both rule sets — test a password that only the
     configured policy accepts (done in `Password_policy_comes_from_configuration`).
  4. `UpdateSecurityStampAsync` saves the whole user, so setting `Status` first and calling it
     stores both in one write (used by D-53 delete; reusable for D-32 deactivation).
  5. The dev DB `CseLibrary` on `.\SQLEXPRESS` does not exist; the app has never run against it.
- Close-out committed `b81dfee` (docs, user "ok commit"); handoff then updated to record it.

### 57. Session 21 — D-32 implemented (2026-10-14)
- User: "D-32-এর approval দিয়ে শুরু করতে পারবে" → taken as D-32 execution approval (code only).
- Implemented as proposed in §56: `ActiveUserSignInManager.CanSignInAsync` (only `Active`, every
  sign-in path; `AccountController` keeps only the null-user check, same generic error);
  `UserService.UpdateAsync` uses `UpdateSecurityStampAsync` instead of `UpdateAsync` when an
  Active user is deactivated (one write). Reactivation does not change the stamp.
- Test A4 `Deactivated_user_loses_existing_sessions`: stamp changes; old cookie → redirect on
  `/GeneralConfig` at once; with the stamp interval set to 0 (test-only, shared
  `IOptions<SecurityStampValidatorOptions>` instance, restored in `finally`) → `/Account/Login`
  on `/Account/ChangePassword`; re-login refused. Deviation from PHASE_0_PLAN A4 wording
  ("advance `TimeProvider`"): interval 0 was the approved approach (§56).
- Evidence: build 0/0; tests 65/65; mutation: no stamp update → A4 fails; no `CanSignInAsync`
  check → A4 + `Inactive_user_cannot_log_in_and_has_no_permissions` fail.
- Commits (each on its own user "ok"): `c63854b` code; docs commit after it.

### 58. Session 21 — registration type revised (2026-10-14)
- After the `UsersController` role-list plan (staff page, `IRoleService.GetNamesAsync`), the user
  asked that the **public** registration choice never come from database roles: a dedicated
  `RegistrationType` enum {`Student`, `DepartmentFellow`}, only a registration choice, never a
  role or permission; server-side rejection of undefined values; no privileged role from public
  registration; `Registrations.Review` for Fellow approve/reject; approval applies only the
  configured member permissions; soft-delete rule unchanged. Recorded as **D-56** (design
  Approved, implementation not approved); refines D-54.
- Code review (read-only): no registration code, view or `Registrations.*` permission exists yet;
  the only role dropdown is the staff `Users` Create/Edit page (`RoleManager.Roles` in the
  controller, all roles incl. `Admin`). The user still accepts `IRoleService` / `RoleService` /
  `RoleReadRepository` for role operations; that refactor is not started.
- Finding raised as Q-S21-4: any holder of `Users.Create`/`Users.Edit` can assign `Admin`.
- Open: Q-S21-1 … Q-S21-4 (PROJECT_DECISIONS §9). Docs only; nothing staged or committed.

### 59. Session 21 — Q-S21-1 … Q-S21-4 answered (2026-10-14)
User decisions, recorded under D-56 (implementation not approved; docs only):
- **Q-S21-1:** separate System Admin-configured, non-privileged default roles for `Student` and
  `DepartmentFellow`; the configuration is validated server-side so a privileged role cannot be
  a registration default. Refines Q-S19-17 (Student only).
- **Q-S21-2:** `RegistrationType` stored on the registration application record; a business
  classification, not an authorization role, claim or permission.
- **Q-S21-3:** dropdown populated from the `RegistrationType` enum, server-side validation of the
  submitted value. **Supersedes** the radio control in D-54 item 1 (choices unchanged).
- **Q-S21-4:** option (b) — `UserService` enforces server-side that only an `Admin` may assign or
  remove the `Admin` role; prevents privilege escalation incl. self-promotion; existing
  authorization checks (`HasPermission`, D-37, D-31 fallback) stay.
- New open questions from these answers: Q-S21-5 "privileged role" definition, Q-S21-6 how
  `Admin` is identified (configured `Seed:Admin:Role`), Q-S21-7 last-`Admin` protection,
  Q-S21-8 restriction scope + timing.

### 60. Session 21 — Q-S21-5 … Q-S21-8 answered (2026-10-14)
User decisions, recorded under D-56 (implementation not approved; docs only):
- **Q-S21-5:** a role is privileged if it has permissions outside the approved member-permission
  allowlist; the allowlist is defined when member-portal permissions are designed.
- **Q-S21-6:** Admin-specific security checks use the literal role name `"Admin"` (the assistant's
  recommendation — configured seed role name — not taken).
- **Q-S21-7:** the last effective Admin is protected server-side against privileged-role removal,
  deactivation and soft deletion; concurrent operations must be accounted for.
- **Q-S21-8:** the Q-S21-4 restriction applies to **all privileged roles** (the assistant had
  recommended `Admin` only); built in Gate 3 (0.9) with the `UsersController` role-list refactor.
- Conflicts / gaps flagged, not resolved (PROJECT_DECISIONS §9): **Q-S21-9** literal `"Admin"` vs
  the configurable `Seed:Admin:Role` (`appsettings.json`, `SeedAdminOptions`); **Q-S21-10** the
  allowlist is empty until the member portal, so in Gate 3 every role with a permission is
  privileged; **Q-S21-11** whether a locked-out Admin counts as effective; **Q-S21-12** role-side
  escalation (`Roles.Edit`/`Roles.Delete` can make a role privileged or remove the `Admin` role).
- No earlier decision was changed: Q-S21-4 is marked "scope extended by Q-S21-8", not rewritten.

### 61. Session 21 — Q-S21-9 … Q-S21-12 answered (2026-10-14)
User decisions, recorded under D-56 (implementation not approved; docs only):
- **Q-S21-9 (Option A):** fixed Admin constant; `Seed:Admin:Role` removed, or kept only with
  startup validation that fails on a mismatch; a mismatch is never silently allowed. Resolves
  the Q-S21-6 conflict.
- **Q-S21-10 (Option A):** until the member-permission allowlist exists, every role with one or
  more permissions is privileged.
- **Q-S21-11 (Option B):** an `Admin` with `Status = Active` counts as effective even during a
  temporary authentication lockout.
- **Q-S21-12 (Option A):** `RoleService` enforces the rules server-side in Gate 3: no privilege
  escalation through permission changes, no `Admin`-role deletion, no permission removal that
  undermines the last-effective-Admin protection, within the existing decision scope.
- Ambiguities flagged, not decided (PROJECT_DECISIONS §9): **Q-S21-13** remove vs validate
  `Seed:Admin:Role`; **Q-S21-14** which `Admin`-role permission removals are refused (identity is
  the role name, so a stripped `Admin` role is still "effective"); **Q-S21-15** renaming `Admin`
  or renaming another role to `Admin` (`RoleService.UpdateAsync` allows renames today).

### 62. Session 21 — Q-S21-13 … Q-S21-15 answered (2026-10-14)
User: "Approve Q-S21-13 to Q-S21-15" — the assistant's recommendations, recorded under D-56
(implementation not approved; docs only):
- **Q-S21-13:** `Seed:Admin:Role` is removed; the seeder uses the fixed `"Admin"` constant.
- **Q-S21-14:** the `Admin` role always holds every permission; removing any is refused.
- **Q-S21-15:** renaming the `Admin` role, or renaming another role to `Admin`, is refused.
- No Q-S21 question remains open. All Q-S21 rules are built in Gate 3 (0.9) with the
  `UsersController` role-list refactor (Q-S21-8, Q-S21-12), each step on its own approval.

### 63. Session 21 — close-out (2026-10-14)
- Commits this session (each on its own user "ok"): `c63854b` D-32 code + test A4; `3c9b5a3` docs
  (D-32, Session 20 follow-up note); `add844a` docs (D-56, §58–§62). Branch
  `feature/phase-0-gate-3`, 8 commits, not merged. Tests 65/65 at `c63854b`.
- The `UsersController` role-list step was **planned but not started**: the assistant proposed
  `IRoleService.GetNamesAsync` (name-only projection; `GetAllAsync` also loads `PermissionIds`)
  and a private helper replacing the six `_roleManager.Roles.Select(...)` calls. The user then
  turned the discussion to D-56; the refactor now goes into one Gate 3 (0.9) plan with the
  Q-S21 rules (SESSION_HANDOFF next action 3). The plan needs approval before code.
- Findings (code-verified, read-only):
  1. `SignInManager.CanSignInAsync` runs at sign-in only; the cookie's stamp validator does not
     call it, so ending existing sessions relies on the stamp change (hence D-32's stamp update).
  2. `PasswordSignInAsync` refuses a non-`Active` user with `NotAllowed` before checking the
     password, so no failed-attempt count is added; the UI shows the same generic error.
  3. `IOptions<SecurityStampValidatorOptions>.Value` is one shared mutable instance; test A4 sets
     the interval to 0 and restores it in `finally` (tests in the `app` collection run serially).
  4. MVC model binding accepts undefined enum numbers (e.g. `"7"`) — D-56 requires
     `Enum.IsDefined` on the server.
  5. `UsersController` Create/Edit list **all** roles incl. `Admin` → any `Users.Create`/
     `Users.Edit` holder could assign `Admin` (incl. to themselves) — answered by Q-S21-4/8.
  6. `RoleService.UpdateAsync` allows renaming any role (Q-S21-15); `DbSeeder` restores all
     permissions to the admin role at startup only (Q-S21-14).
  7. `Seed:Admin:Role` (`appsettings.json`, `SeedAdminOptions.Role`) made the admin role name
     configurable — to be removed (Q-S21-13).
  8. `RoleService.DeleteAsync` deletes roles **physically** (`RoleManager.DeleteAsync`); D-53
     soft delete covers users only. No decision yet — raise with the Gate 3 plan if relevant.
- Close-out docs (this section, SESSION_HANDOFF Session 21 block, PHASE_0_PLAN note,
  PROJECT_DECISIONS §9 "Session 22 start" row) are uncommitted unless the user asks to commit.

### 64. Session 22 — Gate 3 (0.9) D-56 implementation (2026-10-14)
- Verified at start: branch `feature/phase-0-gate-3`, top `add844a`, build 0/0, tests 65/65.
  Session 21 close-out committed on the user's "ok" (`21869c6`).
- Plan proposed and approved ("ok"), then two choices approved separately:
  (a) **concurrency mechanism = `sp_getapplock`** (Exclusive, `LockOwner = Transaction`,
  resource `CseLibrary.AdminGuard`) instead of `UPDLOCK/HOLDLOCK`; (b) **error codes**
  `ErrorCodes.Forbidden = "SECURITY.FORBIDDEN"`, `ErrorCodes.LastAdmin = "SECURITY.LAST_ADMIN"`.
- Implemented `b1bba18` (user gave blanket permission for the remaining sequence after the diff):
  `SystemRoles.Admin` constant; `Seed:Admin:Role` / `SeedAdminOptions.Role` removed;
  `IRoleReadRepository.GetNamesAsync` / `AnyPrivilegedAsync`, `IUserReadRepository.IsEffectiveAdminAsync`
  / `CountEffectiveAdminsAsync`, `IPermissionRepository.CountAsync`, `IUnitOfWork.AcquireAdminGuardLockAsync`;
  `UserService` checks only the changed roles (non-Admin may still edit an Admin's name/email),
  runs every check before the first write, and wraps `DeleteAsync` in a transaction;
  `RoleService` rules per Q-S21-12/14/15; `UsersController` uses `IRoleService.GetNamesAsync`
  (architecture test E1 now also forbids `RoleManager<>` in controllers).
- Tests: new `RoleAssignmentTests` (8); test host `TestCurrentUser` = request user, else
  `ActAs(...)`, else the seeded Admin (direct service calls in tests have no request).
  Evidence: build 0/0, **73/73**; mutation checks M1–M7 (privileged check, count, lock, caller
  check, rename / permission / delete guards) each fail the matching test.
- Not changed: non-Admins still see all role names on the Users pages (server refuses
  privileged ones); `RoleService.DeleteAsync` still deletes physically (§63 item 8, no decision).

### 65. Session 22 — Gate 3 reviews, menu fix, D-57 (2026-10-14)
- `/code-review` on `develop...feature/phase-0-gate-3` (10 findings), verified one by one:
  1. **Menu bug (real, from D-37 `fab1af1`):** `_Layout.cshtml` showed links by cookie
     `Permission` claims, which login no longer writes → fixed `ac24fd7`: the layout calls
     `IAuthorizationService` with the same `Permission:*` policies (one query per request via
     the handler cache); test `Menu_shows_links_for_database_permissions_only`; reverting the
     layout fails it.
  2. **Role-name case (real, minor):** `UpdateAsync` role diff now ignores case (`ac24fd7`).
  3. **Account takeover (real, security):** a non-Admin `Users.Edit` holder could reset an
     Admin's password / change email / deactivate or delete a non-last Admin. Not covered by
     D-56 → raised with the user → **D-57 Approved** (user's rule text) → implemented
     `56bb5f0`; tests `Non_admin_cannot_reset_change_email_deactivate_or_delete_a_privileged_user`,
     `Admin_reset_and_self_service_password_change_still_work_for_privileged_users`;
     mutations M8 (no target check) and M9 (email change unchecked) each fail the first test.
  4. Dismissed with reason: `Cookie.SecurePolicy = Always` (D-31 Approved); deleted user's email
     not reusable (Q-S19-16 "keep for now"); class-level `[Authorize]` beside the fallback policy
     (FR-M9-05 Approved); test caller defaults to the seeded Admin (mutation checks prove the
     non-Admin paths run via `ActAs`); role privilege looked up by normalized name (works; style).
  5. Noted, no change: `RoleService.DeleteAsync` privilege check is outside a transaction — a
     concurrent permission grant by an Admin could interleave (very unlikely; no decision).
- `/ponytail-review`: lean; one optional 2-line cut (`AcquireAdminGuardLockAsync`'s explicit
  "no transaction" check duplicates `sp_getapplock`'s own negative result). Not applied: the
  explicit message is clearer.
- Evidence after `56bb5f0`: build 0/0, tests **76/76**.

### 66. Session 22 — missing access tests, D-55 toasts, stop at Proposed items (2026-10-15)
- 0.9 "Done when: tests A1–A11" was not met (no A5 behaviour test, no A6/A10, no A11) →
  added `2c862e0`: POST without antiforgery token → 400; reflection test (class-level
  `[Authorize]`, anonymous allowlist `Account.Login/AccessDenied`, `Home.Index/Privacy/Error`,
  `[HasPermission]` on every other action except authenticated-only `Account.ChangePassword/
  Logout`); fallback test via test-only `FallbackProbeController`. Mutations: no fallback policy,
  no class `[Authorize]` → the matching test fails. (Existing lockout test relabelled A3.)
- **D-55 implemented `42ae3f4`** (design + Session 20 details Approved; executed under the user's
  blanket permission): `_Toasts.cshtml` (TempData `Error`/`Warning` 5 s, `Success`/`Info` 3 s,
  max 3, label text + colour, close button, Bootstrap 5.1 pauses on hover/focus) replaces the
  layout alerts; `AccessDeniedRedirect` on `OnRedirectToAccessDenied`: local same-site Referer
  (not `//…`, not the denied URL) else `/`, red toast "You don't have permission to perform
  this action."; AJAX (`X-Requested-With`) → 403; `Home.Error` raises an error toast with the
  Reference ID (Q-S19-11). Test A2 (5 Referer cases + AJAX) and a toast render test; the two
  tests that expected `/Account/AccessDenied` now expect `/`. Mutations: no `//` guard, no hook
  → tests fail. Evidence: build 0/0, **86/86**.
- Not done in D-55: client-side (AJAX) toasts — no AJAX UI exists yet; "overall toast plus field
  messages" for validation — forms keep their validation summary + field messages.
- **Stopped** — remaining Gate 3 work needs decisions that are not Approved:
  - 0.10 rest = D-34 **Proposed** items: `UseStatusCodePagesWithReExecute`, Error view with the
    Reference ID only (remove the template's "Development Mode" text), Serilog output with
    TraceId, verify `WithMachineName`/`WithThreadId` enrichers (tests B1–B2).
  - 0.11a HiLo demonstration — plan says "stop for approval" (D-29/D-30); 0.11 audit depends
    on it and on D-21–D-27 (Proposed).
  - 0.12 manual smoke needs the app run against a non-test database (approval needed).

### 67. Session 22 — 0.10 D-34 scope approved and implemented (2026-10-15)
- The user approved the 0.10 D-34 scope (friendly HTTP error pages, no "Development Mode" text,
  Reference ID only, `TraceId` in the server log, no sensitive details in production responses,
  existing authorization / toast / AJAX-403 behaviour kept). Recorded in D-34.
- Implemented `d0a8a66`: `Home.HttpStatus` (anonymous, added to the A6/A10 allowlist) +
  `HttpStatus.cshtml` via `UseStatusCodePagesWithReExecute`; AJAX gets `EmptyResult` with the
  status kept; `Error.cshtml` rewritten (Reference ID only); Reference ID =
  `Activity.Current.TraceId` hex, the same value Serilog 4.3 stores as `LogEvent.TraceId`;
  `{TraceId}` added to both output templates.
- **Test host now runs as `Production`** (`AppFactory.UseEnvironment`) so B1 sees what users
  see; side effect: `appsettings.Development.json` (with the `.\SQLEXPRESS` connection string)
  is no longer loaded by tests. Test log sink `CapturingLogSink` via `ReadFrom.Services`.
- Tests B1 (500 page has no exception text, 32-hex Reference ID equals the logged error's
  TraceId, templates contain `{TraceId}`), B2 (friendly 404 for a signed-in user; AJAX bare 404;
  anonymous unknown URL → Login because the D-31 fallback policy also covers URLs without an
  endpoint — kept, it does not reveal which URLs exist). Mutations: no status pages, old
  `Activity.Id` format, AJAX gets HTML → the matching test fails. Evidence: build 0/0, **88/88**.
- Still open in 0.10 (not in the approved scope): dev SQL logging; enrichers without packages.

### 68. Session 22 — 0.10 leftovers (2026-10-15)
- User "ok" on the first pending item (both 0.10 leftovers). Choice made by the assistant and
  reported: **remove** `WithMachineName`/`WithThreadId` from `Serilog:Enrich` (they had no
  package and did nothing) rather than add two packages; reversible if the values are wanted.
- `42d0908`: `appsettings.Development.json` sets Serilog override
  `Microsoft.EntityFrameworkCore.Database.Command = Information` (Development only;
  `EnableSensitiveDataLogging` is not used, so parameter values are not logged). Config test
  `Sql_command_logging_is_development_only_and_enrichers_resolve`. Not run against the dev DB
  (no approval to start the app on a non-test database); verified by configuration only.
- Evidence: build 0/0, **89/89**. **0.10 done.**

### 69. Session 22 — 0.11a HiLo demonstration (2026-10-15)
- User "ok" to start 0.11a (test DB only). D-29 conditions 1 and 2 done; **stopped at
  condition 3** (no migration change in the repository).
- **Condition 1 — demonstration** `be033f9`, `tests/CseLibrary.Tests/Spikes/HiLoSpikeTests.cs`:
  a spike `DbContext` (not the app model) on its own `CseLibrary_Test_{guid}` LocalDB database
  (D-40 guard, dropped after), `UseHiLo` sequence with block 10 and a `SaveChangesInterceptor`
  that adds the audit row while saving. Shown: (1) id known at `AddAsync`, consecutive ids from
  one block, sequence increment 10; (2) one `SaveChanges` writes entity + audit row carrying
  the new `EntityId`; (3) transaction rollback and a failing `SaveChanges` (duplicate unique
  name) leave neither row; (4) with `SqlServerRetryingExecutionStrategy` (what
  `EnableRetryOnFailure` uses) a simulated transient failure re-runs the work: the second
  attempt gets a **new id** (the first is a gap, the documented D-29 trade-off), commits once,
  one audit row. Counter-check: with the identity key (no `UseHiLo`) all four tests fail — the id
  is still 0 at `AddAsync`, so the audit row cannot carry it. Evidence: **93/93**.
- **Condition 2 — migration + SQL diff**, generated in a temporary git worktree in the
  scratchpad (repository unchanged; connection string pointed at a never-created
  `CseLibrary_Test_*` name; `migrations add`/`script` do not connect): config change =
  `builder.Property(x => x.Id).UseHiLo("GeneralConfigs_HiLo" | "Permissions_HiLo")`; migration
  `HiLoSpike` = `CreateSequence(..., incrementBy: 10)` ×2 + `AlterColumn<int>("Id")` with
  `.OldAnnotation("SqlServer:Identity", "1, 1")` on both tables. **`dotnet ef migrations
  script` fails:** "To change the IDENTITY property of a column, the column needs to be dropped
  and recreated." — SQL Server cannot remove IDENTITY in place, confirming D-29's trade-off:
  existing tables need a table rebuild or a regenerated baseline (**D-30**, Pending). Copies kept
  in the scratchpad only (`HiLoSpike.migration.cs`, `HiLoSpike.config.diff`); worktree removed.
- New finding (no decision): `dotnet list package --vulnerable` reports transitive
  **`System.Security.Cryptography.Pkcs` 9.0.11** — High (GHSA-846f-f764-76mx) and Moderate
  (GHSA-2fmv-rv2c-59x7) — in `CseLibrary.Web`, `CseLibrary.Data`, `CseLibrary.Tests`. A fix
  (pinning a patched version) changes package references and needs approval.

### 70. Session 22 — package fix; read-only data check for D-30 (2026-10-15)
- **Vulnerability fixed on the user's recommendation** `4b0fb10`: direct
  `System.Security.Cryptography.Pkcs` **10.0.13** reference in `CseLibrary.Data` (first patched
  version of both advisories for 10.x; source of 9.0.11 = `Microsoft.Data.SqlClient` 6.1.6 via
  `Microsoft.EntityFrameworkCore.SqlServer` 10.0.12). `dotnet list package --vulnerable
  --include-transitive`: no vulnerable packages in any project. Build 0/0, **93/93**.
- **Audit key strategy:** the user recommends option (a) HiLo + regenerated baseline (D-29/D-30)
  **but requires data confirmation first** → not approved yet.
- **Read-only inspection (D-30, M5 §3 / M6 C), 2026-10-15:** connection strings
  (`appsettings.json`, `appsettings.Development.json`) both `.\SQLEXPRESS` / `CseLibrary`;
  user-secrets file for `UserSecretsId 23c63bb8-…` does not exist; migrations
  `InitialCreate`, `RecordStatusAndAuditColumns`, `UniqueUserEmail`. `sys.databases` (names only,
  `master`): **`.\SQLEXPRESS` has no user databases** (service running; neither `CseLibrary` nor
  `DefaultTemplate` exists there now); `(localdb)\MSSQLLocalDB` has only `Practice` (not this
  project) and no leftover `CseLibrary_Test_*`. No other server or backup was checked — the user
  must confirm that no copy elsewhere (another machine, a backup, a deployment) holds data to keep.

### 71. Session 22 — D-29/D-30 approved; baseline plan approved (2026-10-15)
- User: "option a. approved" → **D-29 Approved for migration changes** (condition 3) and
  **D-30 Approved**; taken as the requested data confirmation (no database anywhere holds data
  to keep).
- Baseline plan proposed and approved ("ok"), **including deleting** the three migrations
  (`InitialCreate`, `RecordStatusAndAuditColumns`, `UniqueUserEmail`) and the model snapshot
  with `git rm` (history keeps them): `UseHiLo` on `GeneralConfig`/`Permission` ids; seeder
  `Add`/`AddRange` → `AddAsync`/`AddRangeAsync`; new single `InitialCreate`; `MigrationTests`
  rewritten (its D-51 data-conversion path disappears with the old migrations);
  `StartupAndGuardTests` migration id updated; new test that a real `GeneralConfig` gets its id
  at `AddAsync` (+ mutation); spike tests kept. `AuditLog` table is **not** part of this — it
  comes with 0.11 after D-21–D-27 approvals. No `database update` against a non-test DB.
- **Implemented `88f8a46`:** `UseHiLo` on both ids; seeder `AddAsync`/`AddRangeAsync`; old
  migrations + snapshot removed with `git rm`; new `20261015031410_InitialCreate` (sequences
  `GeneralConfigs_HiLo`/`Permissions_HiLo` `INCREMENT BY 10`, `GeneralConfigs.Id`/`Permissions.Id`
  plain `int NOT NULL`, Status CHECKs, filtered unique indexes, unique email; IDENTITY only on the
  Identity claim tables). The old migrations' only hand-written SQL was the D-51 data conversion,
  not needed on an empty baseline. `dotnet ef migrations has-pending-model-changes`: "No changes
  have been made to the model since the last migration"; full script generated (scratchpad
  `InitialCreate.sql`). Tests: `MigrationTests.Baseline_matches_the_model_and_migrates_up_and_down`,
  `DataLayerTests.GeneralConfig_and_Permission_ids_are_known_at_AddAsync`; mutation (no `UseHiLo`
  on `GeneralConfig`) fails both. Evidence: build 0/0, **94/94**. **0.11a done.**

### 72. Session 22 — audit package approved (2026-10-15)
- Proposed as one package P1–P7 with four questions; the user answered "ok" → approved as
  recommended: **Q1** `GeneralConfig.Value` audited + rule "no secrets in `GeneralConfig`";
  **Q2** the full D-24 event list (+ `UserDeleted`); **Q3** `EntityId` nvarchar(64); **Q4** JSON
  `{"Prop":{"Old","New"}}`, 1,000-char truncation. Recorded under D-27 ("Audit package").
- Implementation plan follows separately and needs its own approval (no code yet).

### 73. Session 22 — 0.11 audit implemented (2026-10-15)
- Plan (two commits) proposed and approved ("ok").
- **`d8301c0` entity audit:** `AuditLog` + `AuditCategory`, `IAuditable` (GeneralConfig,
  Permission), `IAuditLogRepository` (add only) on the UoW; `AuditLogConfiguration`; migration
  `AddAuditLog` (table + 3 indexes only; the classification annotations add no column changes);
  `AuditClassification` (`.Audited()`/`.NotAudited()`, `ClassifyBaseColumns`, `IsAudited`
  excludes concurrency tokens, shadow and binary); `AuditLogInterceptor` registered after
  `AuditColumnsInterceptor` — builds Created/Updated/Deleted rows, rebuilds (never doubles)
  unsaved entity rows from an earlier failed save, stamps actor/time/`CorrelationId`
  (`Activity.Current.TraceId`) on every new audit row. Tests C1, C2, C3, CorrelationId via a
  real request, C6, C7, P5 (no `ExecuteUpdate/ExecuteDelete/ExecuteSql` outside the one
  `sp_getapplock` call). Mutations: interceptor removed, interceptor order swapped, property
  unclassified, `Key` not audited → the matching test fails. 101/101.
- **`3a00c50` security events:** `SecurityEvent` factory (Services); `UserService` /
  `RoleService` events in the same transaction (full P4 list); `ResetPasswordAsync` and
  `RoleService.DeleteAsync` moved into `ExecuteInTransactionAsync`; new
  `IUserService.ChangeOwnPasswordAsync` (AccountController no longer calls
  `UserManager.ChangePasswordAsync`; it re-issues the cookie after the stamp change);
  `ISecurityAuditService.RecordSignInEventAsync` (own save, try/catch → Serilog, the user is
  the actor); unknown email → `LogWarning` without the typed value. Role permission ids (not
  names) in `RoleCreated`/`RolePermissionsChanged`. Tests: C4 (full event sequence, actor,
  no password text), refused/failed change writes no event, C5 (unknown email none; failed,
  succeeded, logout, 4× failed + `LockedOut`), self-service change still works + `PasswordChanged`.
  Mutations: four event lines removed → the matching test fails. **105/105**.
- Not tested (as planned): the best-effort path where writing a sign-in event fails.
- `has-pending-model-changes`: none. Migrations applied only to test databases. **0.11 done.**

### 74. Session 22 — 0.12 option chosen; close-out (2026-10-15)
- **0.12 smoke target decided:** the assistant offered (ক) the real dev database (`.\SQLEXPRESS`
  / `CseLibrary`, which does not exist yet — the first run would create it, apply both
  migrations and seed an admin) or (খ) a disposable `CseLibrary_Test_*` LocalDB database,
  recommending (খ). **The user chose (খ).** Execution is left to Session 23 (the user closed the
  session first). Conditions: D-40 guard name; connection string and seed password only as
  process environment variables (no file or user-secrets change); drop only that generated
  database afterwards; the dev database stays untouched and uncreated.
- **Session 22 summary** (details §64–§73): steps **0.9, 0.10, 0.11a, 0.11 done**; D-57 new;
  D-55 and the 0.10 D-34 scope implemented; D-29/D-30 approved and applied (regenerated
  baseline); audit package P1–P7 approved and implemented; vulnerable `Pkcs` pinned. Branch
  `feature/phase-0-gate-3`: 32 commits, not merged; tests 105/105.
- **Discussion notes:** the user worked step by step with one-"ok"-per-action, gave a one-time
  blanket permission for a sequence ("don't wait for my approval"), then returned to explicit
  approvals; pasted assistant-style text that recommended a choice but disclaimed approval was
  treated as **not** an approval and confirmed directly. The user asked for plain-Bangla
  explanations when review output was too terse.
- **Process findings** (also in SESSION_HANDOFF "Learned in Session 22"): check a step's "Done
  when" before ticking it; mutation scripts must detect a failed build; `dotnet ef` design-time
  runs `Program.cs` up to `Build()`; long Windows paths need `\\?\` to delete scratch worktrees.
- Open, no decision (carried to Session 23): physical role delete (§63 item 8); all role names
  visible to non-Admins on Users pages; role-delete privilege check vs. concurrent grant (§65);
  untested best-effort sign-in event failure path (§73); older items D-49 O1–O5, EF Tools
  package, UI brand text, CR5 cascade part.

### 75. Session 23 — 0.12 smoke run on a disposable database (2026-10-15)
- **Read-only check:** branch `feature/phase-0-gate-3`, build 0/0, tests 105/105. Session 22
  close-out committed on the user's "ok" (`efb8a68`).
- **Smoke setup (option (খ), §74):** `dotnet run --no-launch-profile --urls https://localhost:7259`
  with `ASPNETCORE_ENVIRONMENT=Production`, `ConnectionStrings__DefaultConnection` → LocalDB
  `CseLibrary_Test_1cad9ca5d094426085f3abcbccf828c6`, random `Seed__Admin__Password` — process
  environment only, no file or user-secrets change. Startup migrated and seeded it (19 seed
  audit rows). https is required because the auth cookie is `SecurePolicy = Always`; the dev
  certificate is untrusted (browser warning accepted, not trusted system-wide).
- **Results.** The user drove the browser (admin login, role `Student` + user create, login/
  logout as the student) and reported "everything works"; DB and log showed deactivate,
  lockout and the toast had not run yet, so the assistant ran those with `curl` against the
  same database (user `smoke2@local`):

  | Item | Evidence | Result |
  |---|---|---|
  | Admin login | `LoginSucceeded` #20, #30 | Pass |
  | Create user | `RoleCreated` #21; `UserCreated` #22, #31 (`{"Roles":["Student"]}`) | Pass |
  | Deactivate user | POST `/Users/Edit` 302; `Status` 1 → 2; `UserDeactivated` #39 | Pass |
  | Lockout after 5 failures | `LoginFailed` #33–#36, `LockedOut` #37–#38, `LockoutEnd` +15 min; correct password then refused | Pass |
  | Permission failure toast | Student GET `/Users` → 302 `/`, toast "…permission to perform this action." | Pass |
  | 404 page | `/does-not-exist` → 404 "Page not found", status code shown | Pass |
  | Entity + security audit | `Permission`/`GeneralConfig` `Created` with JSON changes (#1–#19); user/role/sign-in events (#20–#39) | Pass |

- **Cleanup:** app stopped; database name checked against `^CseLibrary_Test_[0-9a-f]{32}$`,
  then dropped; LocalDB has only `Practice`; `.\SQLEXPRESS` untouched; working tree clean.
- **Findings (no decision):**
  1. `/CseLibrary.Web.styles.css` returns 404 on every page when run as `Production` via
     `dotnet run` (static web assets are a Development-only feature; expected to be served
     after `dotnet publish` — not verified).
  2. The 404/403 status page shows no Reference ID; only the 500 `Error` page does. The 500
     path was not triggered manually (covered by tests B1–B2).
  3. A locked-out user sees the same "Invalid login attempt." as a wrong password (no account
     enumeration; the audit row says `LockedOut`).

### 76. Session 23 — Gate 3 approved, reviews skipped; Phase 1 catalog started (2026-10-15)
- The user, low on token budget, asked to "build the features". The assistant explained the
  full system cannot be done in that budget and that D-48 / domain policies are still
  Proposed/Pending. **The user chose:** close Gate 3 **without** the `/code-review` +
  `/ponytail-review` runs (a deliberate exception to D-07 v7's pre-gate review rule), merge into
  `develop`, then start the first Phase 1 slice — **book titles + physical copies CRUD** — which
  needs no pending decision. Lending, members, portal and email stay unstarted.
- Gate 3 evidence: 0.9–0.12 done (§64–§75), build 0/0, tests 105/105.

### 77. Session 23 — catalog slice implemented (2026-10-15)
- Implementation choices (assistant, within archive FR-02/FR-03; review with the slice):
  `Authors` and `Category` are free text (no `Authors`/`Categories` tables yet); no cover image
  or language; `CopyStatus` tinyint 1–6 with CHECK; ISBN unique when present (filtered index),
  accession number unique (filtered, deleted rows reusable, D-51); `Issued` can be neither set
  nor cleared by catalog edits (`COMMON.INVALID_STATE`); a title with copies cannot be deleted
  and an issued copy cannot be deleted (`COMMON.IN_USE`); one permission set `Books.*` covers
  titles and copies; FK `BookCopies → Books` is `Restrict`.
- Evidence: build 0/0; tests 108/108 (`CatalogTests` ×3; audit allowlist and HiLo sequence
  expectations extended).

### 78. Session 23 — lending slice: policy defaults approved, implemented (2026-10-15)
- **User decision (AskUserQuestion):** Student max 3 active loans / 14 days, Fellow 5 / 30, one
  renewal, a member with an overdue loan gets no new issue; values in GeneralConfig
  (`Lending.{Type}.MaxLoans`, `Lending.{Type}.LoanDays`, `Lending.MaxRenewals`), seeded per
  missing key. This settles the D-48 values for v1; D-48's remaining Proposed parts (per-member
  overrides, category rules) stay open. Catalog merged into `develop` on the user's "yes".
- Implementation: `Members`, `Loans` (HiLo, audited, migration `AddLending`); issue runs in
  `ExecuteInTransactionAsync` under a per-member `sp_getapplock` (`IUnitOfWork.AcquireLockAsync`,
  the admin guard now uses it) so the limit holds under concurrency (P12); filtered unique index
  `UX_Loans_ActiveCopy` makes a concurrent issue of one copy fail as `LENDING.COPY_UNAVAILABLE`
  (D-35); return sets the copy Available; renew refused when overdue or at the limit; member
  delete refused while holding books. "Today" = server local date. `Lending.*` / `Members.*`
  permissions.
- Evidence: tests 114/114 (`LendingTests` ×6 incl. two concurrency tests); mutation — member lock
  removed → the per-member limit test failed 3/3, restored → green.

## Unresolved decisions and why

**Session 13:** rows for D-07, D-07 T1 scope, T1-EOL, T4, step 0.1, D-41 tooling, D-40
connection string and D-47 below are **resolved** (§29); kept as history.

| ID | Why still open |
|---|---|
| D-07 branching (M14, v6) | Proposed; v6 wording, D-06 partial supersession and the documentation impact list (§21) await approval; T1–T5 proposals not approved. **v6.1 correction draft (§22: F1, F2 mandatory; F3, F4 recommended) awaits the user's decision — not applied.** Session 9 (§23): final F2 (c)4 and F4 Isolation wording accepted for the review; F1/F3 acceptance not yet stated; approval order (A)–(F) in §23. **Session 10 (§24):** user direction — F1/F3 acceptable only with the [F1+]/[F3+] additions; Variant A/B diffs, [R] received-files copy (R1/R2/R3) and [C] `CURRENT.txt` rule proposed — none approved, v6.1 still not applied. **Session 11 (§25):** read-only review of Variant A — B4 parsing/expected values, gaps G1–G6, manifest/RESULT M1–M9 (M9: §24 [R] claim inaccurate), R2-failure wording proposed; not applied. **Session 12 (§28):** approval (A) given — v6.1 applied (Variant A + G1, G2, M1, M9); D-07 still Proposed, awaiting approval (B) + [R] choice. Push/PR and `main` protection deferred by the user |
| D-07 T1 scope | `core.autocrlf=true` would alter 29 received files on add; `.gitattributes` + inventory checks B1–B6 / C1–C4 in step 0.1 scope need approval; blocks 0.1 even if D-07 is rejected |
| D-07 T1-EOL | LF for new authored text files — Pending, separate explicit confirmation needed before step 0.2; not confirmed by D-07 approval |
| D-07 T4 backup root | **Resolved Session 12 (§28):** `<backup-root>` = `H:/Internship Project/DLMS-Backups/` with write authorization (approval (D)). T4 level (c) not yet exercised |
| Step 0.1 | Documentation approved (§18), but step 0.1 needs D-07/T1 scope/T4 resolved and its own explicit approval |
| ~~D-29 HiLo~~ | **Resolved Session 22 (§69, §71):** demonstrated, diff shown, approved (option (a)) |
| ~~D-30 Baseline migration regeneration~~ | **Resolved Session 22 (§70, §71):** data confirmed by the user, approved |
| D-41 tooling | `global.json` + local `dotnet-ef` manifest were not covered by the layout approval |
| D-40 connection string | Location of the `CseLibrary` dev connection string and seed password not decided |
| D-13 signatures | Contract approved; exact signatures reviewed at Gate 2 |
| D-14 ambiguous commits | Approach approved (M8); per-use-case assessment (risk, mechanism, tests, limitations) due at Gate 2 |
| D-15 / D-34 details | `PageRequest` limits, `Result` type — Proposed |
| D-31 values / D-32 mechanism | Baseline approved; **Session 19 (§52): password + lockout values and D-32 mechanism Approved**; **Session 20 (§55): stamp interval, cookie, antiforgery Approved and D-31 implemented `215d75e`**. D-32 implementation awaits execution approval (§56) |
| Q-S19-1 / Q-S19-2 (§52) | ~~Registration scope~~ answered → D-54 (§53); anonymous `Error` / status-code / `AccessDenied` pages (now tied to D-55 Q-S19-10) |
| ~~Q-S19-2 … Q-S19-17 (§53)~~ | **Answered Session 20 (§55)** — registration details (D-54), toast/forbidden-page details (D-55), deleted users in list + email reuse (D-53/D-52), anonymous error pages |
| ~~D-53 implementation (§53)~~ | **Resolved Session 20 (§55): implemented `8550c00`** |
| ~~D-37 cleanup~~ | **Closed Session 20 (§55):** target `CseLibrary` does not exist on `.\SQLEXPRESS`; nothing to delete |
| ~~D-21/24/25/27 details~~ | **Resolved Session 22 (§72):** audit package P1–P7 approved (Q1–Q4 as recommended) |
| D-47 naming (M9) | `LibraryManagement.*` vs Approved `CseLibrary.*`; blocks 0.3 (Q-M9-9) |
| ~~D-45 outstanding-loan rule (M9)~~ | Superseded by D-48 (M10) — conflict resolved as configuration; no request workflow |
| D-48 borrowing policy (M10–M12) | Wholly Proposed; reviewed before the Lending phase, not now. Architecture, settings, constraints, defaults/overrides, activation, permission holder (conflict with archive roles table), due-date edit (would amend FR-06), category change (Q-M10-1, -2, -4..-9); P17 mechanism; member status on the member row (Phase 2); values Pending user/department (Q-M10-3); lost/cancelled counting Q-M9-3; lowering limit Q-M9-5; explicit `ReadCommitted` in D-14 (optional at 0.8) |
| D-46 / D-15 M9 / D-31 M9 | Proposed designs for AJAX loading, keyset paging, controller authorization; values Q-M9-6..8, Q-M9-10 (D-44 superseded by D-48). **Session 19 (§52): D-31 M9 + Q-M9-10 Approved/answered** |
| Domain policies | Fellow definition, limits, durations, renewals, Lost status, SMTP, portal accounts, hosting, retention — department/user input needed in later phases |
