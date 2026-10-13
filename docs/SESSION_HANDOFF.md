# Session Handoff

## ▶ Session 16 (2026-10-13) — read first; overrides older blocks where they differ
Session 16 closed at the user's request (conversation too long) — **Session 17 starts here.**

### State (verified 2026-10-13)
- Branch **`develop`** (local only; `main` @ `af89144` unchanged). Session 16 commits, each on the
  user's "ok":
  | Commit | Content | Log |
  |---|---|---|
  | `8a6df00` | Session 15 close-out docs (on `feature/phase-0-gate-1`) | §38 |
  | `bbee8dc` | **Gate 1 approved → merged into `develop` `--no-ff`** | §39 |
  | `fe960d9` | Gate 1 approval docs | §39 |
  | `bdd5261` | M15: archify target-architecture + database-model diagrams, `database.md`, D-49 | §40 |
  | _(staged, uncommitted)_ | ER diagram `database-tables.html`, D-50, D-51, overview sublabels, Session 16 close-out (§41–§42) | §41–§42 |
- Evidence on `develop` after the merge: build 0 errors (NU1903 AutoMapper only), tests 16/16,
  0 `CseLibrary*` DBs on LocalDB, `.\SQLEXPRESS` untouched. No code changed after the merge.
- `feature/phase-0-gate-1` still exists (merged); deleting it needs the user's OK.

### Decisions this session (details: PROJECT_DECISIONS)
- **D-49 Proposed:** library DB design (`docs/database.md`) — derived available count (no counter),
  open-loan invariant (filtered unique index + copy `RowVersion`), append-only lending history.
  Open choices **O1–O5** (database.md §6) wait for the user, latest before the phase that builds them.
- **D-50 Approved:** PK `Id`; FK = singular entity + `Id` (`BookId`), role prefix for two FKs to one
  table (`IssuedByUserId`); `CreatedAt/By`, `UpdatedAt/By` on every `BaseEntity` (template names,
  "option A"), set by a `SaveChanges` interceptor; log tables exempt.
- **D-51 Approved:** `enum RecordStatus : byte { Active = 1, Inactive = 2, Deleted = 3 }` replaces
  `BaseEntity.IsDeleted` and `ApplicationUser.IsActive`; one global filter; central soft delete;
  unique indexes `WHERE Status <> 3`; `CHECK`; business statuses stay separate.
- D-50 + D-51 are implemented in **Gate 2 step 0.8** (PHASE_0_PLAN lists every touched file).

### Docs and diagrams (open `.html` files in a browser, not the IDE)
- `docs/diagrams/database-tables.html` — **ER diagram** (hand-built SVG, all columns, crow's-foot
  FK lines, click to highlight). Owns the column list; `database.md` owns the reasoning.
- `docs/diagrams/database-model.html` — archify table overview; `target-architecture.html` — archify
  whole-system view. Both re-pass `finalize`.

### ⛔ Next action (Session 17), in order
1. Verify read-only: `git branch --show-current` (= `develop`), `git log --oneline -6`, `git status`.
2. If the Session 16 close-out is still staged/uncommitted: show the staged list + message, commit on
   the user's OK. Suggested message:
   `Add ER diagram and approve naming and record-status conventions (docs)`.
3. Ask explicitly: **start Gate 2 (steps 0.6–0.8)** on a new `feature/phase-0-gate-2` from
   `develop`? The user's "go for it" approved D-50/D-51 only, **not** the Gate 2 start.
4. Gate 2 content: 0.6 remove AutoMapper, 0.7 core contracts, 0.8 repositories/UoW + D-16 fixes +
   **D-50/D-51** (RecordStatus, interceptor for audit columns and soft delete, migration with data
   conversion). Show staged list + message before each commit; `/code-review` + `/ponytail-review`
   before the Gate 2 report; stop at the report.
- Still open: O1–O5 (database.md), `EF Tools` package, UI brand text, deleting the merged
  `feature/phase-0-gate-1` branch.
- Session 17: old `DefaultTemplate/` folder (ignored build output only) deleted on the user's
  request (DECISION_LOG §43).

### Learned in Session 16
- The user reviews designs **visually**. Plain archify boxes without columns were not enough; a real
  ER diagram (tables + columns + crow's-foot lines) was. Prefer readable pictures + a short "in
  words" list over long prose.
- Opening an `.html` file in VS Code shows source; open it in the browser (`Start-Process <file>`).
- Headless Edge screenshot works for self-checking pages:
  `msedge.exe --headless=new --disable-gpu --window-size=1900,1500 --screenshot=<png> file:///<path>`.
- The user brings their own conventions (naming, audit columns, status enum); evaluate, refine
  with concrete reasons (e.g. 1/2/3 instead of HTTP codes), give one recommendation, then act on
  their answer. Record user-approved conventions as Approved decisions.
- Bash heredocs with mixed quotes break; write data/scripts to the scratchpad with the Write tool
  and run them with `node <file>`.

## Session 15 (2026-10-13) — history; the Session 16 block above takes precedence
Session 15 closed at the user's request (conversation too long) — **Session 16 starts here.**

### State (verified 2026-10-13)
- Branch **`feature/phase-0-gate-1`** (local only), 8 commits ahead of `develop` (`develop` = `main`):
  the 5 Gate 1 step commits (Session 14 block below) + Session 15:
  | Commit | Content | Log |
  |---|---|---|
  | `346496a` | Session 14 close-out docs (+ README net8.0 → .NET 10, finding 9) | §36 |
  | `8504115` | Gate 1 review-finding correction (test harness, `.sln`, 2 package refs) | §37 |
  | `97891e8` | Gate 1 correction evidence in docs | §37 |
  | _(uncommitted)_ | Session 15 close-out docs (this block, §38) — commit on the user's OK | §38 |
- Evidence: `dotnet build CseLibrary.sln` 0 errors (warnings: NU1903 AutoMapper only; NU1510 gone);
  `dotnet test CseLibrary.sln` 16/16; 0 `CseLibrary*` DBs on LocalDB; `.\SQLEXPRESS` untouched.

### ⛔ Gate 1 corrected — STOPPED, approval not yet given. Next action (Session 16), in order
1. Verify read-only: branch, `git log --oneline develop..HEAD` (8–9 commits), `git status`.
2. If the close-out docs are still uncommitted: show staged list + message, commit on OK.
3. Ask the user explicitly: **approve Gate 1 and merge `feature/phase-0-gate-1` → `develop`
   `--no-ff`?** The user's last "ok" (Session 15) covered only the doc commit `97891e8` — do not treat
   it as merge approval. After merge: build + test on `develop`, report.
4. Then **M15** (DECISION_LOG §33, `requirements.md` FR-M15-01..04) before Gate 2 — archify overall
   architecture + DB/ER diagrams + DB design proposal; everything **Proposed**, no code/migration.
5. Gate 2 (0.6–0.8) only after explicit approval.
- Still open (PROJECT_DECISIONS §9 "Session 14"): `EF Tools` package (kept — VS PMC), old
  `DefaultTemplate/` folder deletion, UI brand text.

### Learned in Session 15
- The user may paste advice from another assistant (e.g. a findings-handling table); treat it as
  input to evaluate, give one consolidated recommendation, then act on the user's own "YES/ok".
- One "ok" = one approval: when a message asks for two approvals (commit + merge), a bare "ok"
  approves only the first; re-ask for the gate merge.
- Verify review/ponytail suggestions before applying: e.g. removing the sync interceptor overrides
  would have hidden `UserService`'s sync `Users.ToList()` from the N+1 counter.
- Mutation check for a safety test: comment out the guard line, run the single test (must fail),
  restore the file, rerun the full suite.
- `sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT name FROM sys.databases WHERE name LIKE 'CseLibrary%'"`
  works for the leftover-test-DB check.

## ▶ Session 14 (2026-10-13) — read first; overrides older blocks where they differ
Session 14 closed at the user's request (conversation too long) — **Session 15 starts here.**

### State (verified 2026-10-13)
- Branch **`feature/phase-0-gate-1`** (local only; `main` @ `af89144`, `develop` = `main`).
- Gate 1 commits, each after the user's OK:
  | Step | Commit | Content | Log |
  |---|---|---|---|
  | 0.1 | `1fdbe5e` | received template + requirements byte-identical (151/151 SHA-256) | §30 |
  | 0.2 | `9cf511a` | docs, README, `architecture.md`, archify diagram, archive → `docs/archive/` | §30 |
  | 0.3 | `3c8d98a` | rename → `src/CseLibrary.*`, root `CseLibrary.sln`, `global.json`, local `dotnet-ef` 10.0.12 | §31 |
  | 0.4 | `468a82c` | connection strings → `CseLibrary` (Windows auth), net10.0, EF/Identity 10.0.12 | §32–§33 |
  | 0.5 | `22d27ae` | `tests/CseLibrary.Tests`: LocalDB guard, shared factory, SQL counter, 15 tests | §34 |
- Evidence: `dotnet build CseLibrary.sln` 0 errors (warnings NU1903 AutoMapper, NU1510);
  `dotnet test CseLibrary.sln` 15/15 (twice); no `CseLibrary*` DB left on LocalDB; `.\SQLEXPRESS`
  holds only the 4 system DBs (nothing created there).
- **Uncommitted:** Session 14 close-out doc updates (`docs/DECISION_LOG.md` §33–§35,
  `PROJECT_DECISIONS.md` (header, D-37, D-40, D-41, D-42, D-43, §9 Session 14 table),
  `PHASE_0_PLAN.md`, `requirements.md` (FR-M15-01..04), `README.md`, `CLAUDE.md`, this file).
  Show the staged list + message and commit only on the user's OK (D-07 v7).

### ⛔ Gate 1 reported — STOPPED. Next action (Session 15), in order
1. Verify read-only: branch, `git log --oneline main..HEAD` (5 commits above), `git status`.
2. Ask the user (open table: `PROJECT_DECISIONS.md` §9 "Session 14"):
   a. which Gate 1 review findings to fix (DECISION_LOG §35; recommendation: one correction commit
      — code-review 2–5, 8–10 + all ponytail items; `Program.cs` catch-all → 0.10);
   b. Gate 1 approval → merge `feature/phase-0-gate-1` → `develop` `--no-ff` (separate approval);
   c. delete the old `DefaultTemplate/` folder (only ignored `bin/`, `obj/`, `*.csproj.user`)?
3. Then **M15** (DECISION_LOG §33, `requirements.md` FR-M15-01..04) **before Gate 2**:
   archify overall target-architecture diagram + DB/ER model diagram + DB design proposal
   (multi-copy titles, copy holder, immutable lending history; derived available count vs counter
   with trade-offs). Everything **Proposed** until the user approves; no code/migration.
4. Gate 2 (0.6–0.8) only after explicit approval.

### User preferences learned in Session 14
- No DB username/password (Windows auth) and **no seed admin password** set; app-start check lives
  in the tests. Running the app on `.\SQLEXPRESS` would need `Seed:Admin:Password` in user-secrets.
- Wants visual (archify) architecture and DB diagrams to understand the project before code.
- Commits one step at a time with "ok"; review tools + report at every gate.

### Environment gotchas (Session 14)
- **VS Code C# Dev Kit** locks project folders (directory `git mv` → "Permission denied") and
  **rewrites `.csproj` files** when referenced projects look missing → move files individually,
  always `git diff` `.csproj` before staging.
- **Git Bash `sed -i` strips CR** from CRLF files → use `sed -b -i`, only on matching files; the
  Edit tool preserves line endings. Python is not installed.
- `dotnet sln add` rewrites the solution (x64/x86 configs, solution folders; left an empty `src`
  folder — review finding 8). SDK 10 `dotnet new tool-manifest` writes `dotnet-tools.json` at the
  root → move it to `.config/`.
- `dotnet ef` design-time logs `HostAbortedException` as Fatal via the `Program.cs` catch-all —
  harmless now, fixed in 0.10. `dotnet ef migrations list`/`database update` would connect to the
  configured DB — do not run against `.\SQLEXPRESS` without approval.
- archify: `node C:/Users/Remon/.claude/skills/archify/bin/archify.mjs finalize architecture <json>
  <html> --repo-root . --quality showcase --out-dir <scratch> --json` from the repo root; evidence
  must cite a **committed** revision. archify has no ER type → model tables as `database` nodes with
  labelled "1 — N" relationships (architecture type).

## Session 13 (2026-10-13) — history; the Session 14 block above takes precedence
- **R1 backup verified** (read-only, DECISION_LOG §29): `received_20261013-152404/` is the
  recovery source; all hashes, file set, sizes and current originals match.
- **User approved the senior review package (items 1–13, §29):** D-07 **v7 simplified Approved**
  (v6.1 Superseded); `.gitattributes` `* -text`; LF for new files; `CseLibrary.*` kept (D-47
  Rejected); `global.json` + local `dotnet-ef`; dev DB `CseLibrary` on `.\SQLEXPRESS`, **base and
  Development `appsettings` both fixed** (base still says `DefaultTemplate` today — do not run the
  app before 0.4); Q-M9-8/10/11 answered; step 0.2 slimmed; D-16 Approved.
- **Authorized:** step 0.1 and Gate 1 (0.1–0.5), with staged list + message shown before every
  commit; `/code-review` + `/ponytail-review` before the Gate 1 report; stop at the Gate 1 report.
- **Workflow (D-07 v7):** `main` → `develop` → `feature/phase-0-gate-1`; local only; `--no-ff`.
- **Review tooling (user request, in D-07 v7):** `/code-review` + `/ponytail-review` on every gate
  diff before the gate report; `archify` architecture/dependency diagram in step 0.2.

### Repository state at Session 13 close (verified 2026-10-13)
- Current branch **`feature/phase-0-gate-1`** (from `develop`, from `main` @ `af89144`); no new
  commit yet; local only.
- Working tree: new `.gitattributes` (LF, 5 lines: `* -text`, `/.gitignore text`,
  `/LICENSE text` + 2 comments); `.gitignore` + `\r\n# IDE-local user settings (D-06 step 0.1)\r\n*.user\r\n`
  (working copy CRLF, stored LF by `text`).
- **Index (staged, not committed): 153 paths** = 151 files of `DefaultTemplate/` + `knowledge Docs/`
  + `.gitattributes` + `.gitignore`. Staged tree hash (`git write-tree`):
  **`b48a82b62f167454f77f2ae567ef8263414ea3b7`**.
- **Hash check PASS:** SHA-256 of every staged blob = R1 manifest (`backup.sha256` minus
  `bin/`, `obj/`, `*.user`) — 151/151, empty diff → no line-ending change. `.csproj.user` ignored
  (`.gitignore:60`).
- Untracked (by design, step 0.2): `CLAUDE.md`, `docs/`. Docs changed in Session 13 are LF;
  pre-Session-13 snapshot only in the (non-persistent) scratchpad.

### ▶ Next action (Session 14) — single
1. Verify read-only: `git branch --show-current` = `feature/phase-0-gate-1`; `git write-tree` =
   `b48a82b6…a3b7`; `git status --short` = 153 `A`/`M` entries + `?? CLAUDE.md`, `?? docs/`.
   If the tree hash differs, **stop and report** (do not re-stage silently).
2. Ask the user to approve the commit (exact staged tree above) with this message, then commit:
   ```
   Add received template and requirements baseline (step 0.1)

   Commit DefaultTemplate/ and knowledge Docs/ byte-for-byte as received
   (.gitattributes "* -text"; verified against the R1 backup manifest,
   151/151 SHA-256 match). Ignore IDE-local *.user files.

   Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>
   ```
   (The user saw this exact message in Session 13; the commit was not yet approved.)
3. After commit: `git status` shows only `CLAUDE.md`, `docs/` untracked; `git ls-files --eol`
   spot check; mark 0.1 `[x]` with the hash in PHASE_0_PLAN + here.
4. Continue Gate 1 (already authorized): 0.2 slimmed → 0.3 rename `CseLibrary.*` + `global.json`
   + `dotnet-ef` manifest → 0.4 connection strings (base **and** Development) → `CseLibrary`,
   verify user-secrets, then net10.0 → 0.5 test harness. Show staged list + message before each
   commit. Then `/code-review` + `/ponytail-review` on the gate diff, Gate 1 report, **stop**.
- **Do not run the app** before step 0.4's connection-string check (base `appsettings.json` still
  says `Database=DefaultTemplate`).

_Earlier — last updated: 2026-10-13, **Session 12** — user approved **(A)** documentation (v6.1 applied to
D-07: Variant A + §25 G1, G2, M1, M9) and **(D)** `<backup-root>` = `H:/Internship Project/DLMS-Backups/`
with write authorization. **D-07 still Proposed** — approval (B) not given; step 0.1 not
authorized; nothing executed, no Git change (DECISION_LOG §28). Session 12 closed at the user's
request (conversation too long) — **Session 13 starts from this file**.
Earlier: Session 11 — (1) read-only technical review of Variant A: B4
`check-attr -z --stdin` parsing and expected values (correct; wording gaps B1–B3), multi-state
reconstruction gaps G1–G6 (G1 assume-unchanged/skip-worktree and G2 excludesFile = silent loss),
manifest/`RESULT.txt` rules M1–M9 (M9: §24 [R] claim inaccurate), R2-failure wording — **not
applied, D-07 still v6 and Proposed** (DECISION_LOG §25). (2) On the user's instruction an
**independent verified backup** of `DefaultTemplate/` + `knowledge Docs/` was made at
`H:/Internship Project/DLMS-Backups/received_20261013-152404/` (409 files, SHA-256/size/path
identical, originals and Git unchanged; drive `H:` accepted by the user; DECISION_LOG §26).
(3) Progress check "how far from coding" answered; assistant recommended shrinking the D-07
review (apply only G1, G2, M1, M9 with (A)+(B), then step 0.1) — not decided (DECISION_LOG §27).
Docs updated at the user's request: DECISION_LOG §25–§27 + Unresolved rows, PROJECT_DECISIONS
session line + §9 rows, PHASE_0_PLAN note, this file. Session 11 closed because the conversation
grew too long — **Session 12 starts from this file**.
Earlier: Session 10 — clarifications for approval (A): F1/F3 clause list,
(A) vs (B) file scope, T4 failure handling in step 0.1, backup-root requirements. User direction
(not approval): F1/F3 acceptable only with additions [F1+] (checks prove this
repository/environment only) and [F3+] (fixed flags do not neutralize all config; what check (c)
detects); F2/F4 keep §23 wording. New proposals: [R] independent copy of the 409 received files
(R1 original source / R2 byte copy / R3 none — T4 backups do not copy them) and [C] `CURRENT.txt`
only for verified backups, failed folders kept as written. Variant A (F1–F4) and Variant B (F2+F4)
diffs shown in chat. Exact text: DECISION_LOG §24. **Nothing approved; v6.1 still not applied;
D-07 text still v6, still Proposed.** Documentation only; only read-only Git commands run.
Session 10 closed because the conversation grew too long — **Session 11 starts from this file**.
Earlier: Session 9 — final F2 (c)4 / F4 Isolation wording, approval-readiness checklist (A)–(F)
(DECISION_LOG §23).
Earlier: Session 8 — targeted review of D-07 v6 T1/T4 (F1–F4) and the v6.1 correction draft
(DECISION_LOG §22).
Earlier: Session 7 — D-07 v4 → v6 (B1–B6 / C1–C4, T1-EOL split off, T3 step 0.1 records in 0.2,
T4 three-level verification; DECISION_LOG §21). Session 6 — D-07 v1 → v4; finding `core.autocrlf=true` would alter 29 received files
(DECISION_LOG §20). Session 5 (M10: borrowing rules re-specified as a configurable
per-category borrowing-policy system; M11: independent settings, values vs logic, D-48 wholly
Proposed with locking assessment, decisions split now/later; M12: consistency audit, archive
reconciliation, overclaims corrected; M13: five corrections; **documentation package approved
2026-10-13** (DECISION_LOG §18) — **step 0.1 still needs separate explicit approval**; M14: Git
branching workflow proposed as **D-07 (Proposed)**, final wording awaiting approval
(DECISION_LOG §19). Session 5 closed here because the conversation grew too long — **Session 6
starts from this file**). Previous: Session 4 (M9: six new functional
requirements), Session 3 (M7 = M6 re-sent; M8 answered four open questions).
Update at every review gate / meaningful milestone._

## 1. Project
CSE Department Library Management System: books + individual physical copies, Student and
Department Fellow members, issue/return with history, member self-service portal, return
confirmation + due-date/overdue reminder emails, no fines in v1.
Stack: ASP.NET Core MVC, C#, EF Core, SQL Server, ASP.NET Core Identity, Razor/Bootstrap, xUnit.
Base code: `DefaultTemplate/` (4-project Onion starter; currently net8.0).

## 2. Repository state (verified 2026-10-12, Session 2; re-verified Sessions 3 and 4 — unchanged)
- Branch: `main` · Latest commit: `af89144` "Initial commit" (only `.gitignore`, `LICENSE`).
- Untracked: `DefaultTemplate/`, `knowledge Docs/`, and the session docs:
  `CLAUDE.md`, `docs/SESSION_HANDOFF.md`, `docs/PROJECT_DECISIONS.md`,
  `docs/PHASE_0_PLAN.md`, `docs/DECISION_LOG.md`, and (new in M9) `docs/requirements.md`,
  `docs/business-rules.md` — **uncommitted; content approved 2026-10-13 (DECISION_LOG §18), not
  committed (committing them is step 0.2).** Branch `phase-0/baseline` not created yet.
  (Re-verified Session 5, M10–M13 (2026-10-12/13): branch `main`, HEAD `af89144`, only `main`
  exists locally, no stash, same untracked set; archive SHA-256 unchanged.)
- **Re-verified Session 6 (2026-10-13, read-only):** branch `main`, HEAD `af89144` =
  `origin/main`, only `main` locally, no stash, same four untracked items, archive SHA-256
  unchanged. Git 2.52.0.windows.1.
- **Line endings (Session 6 finding):** `core.autocrlf=true` (system config
  `C:/Program Files/Git/etc/gitconfig`); no `.gitattributes`. Of the 151 files step 0.1 would
  stage: 29 CRLF-only, 98 LF-only, 0 mixed, 24 without newline — a plain `git add` would change
  29 blobs (incl. the archive) → not byte-identical. Tracked `.gitignore`/`LICENSE` are
  `i/lf w/crlf`. Fix proposed as D-07 T1 (not approved). Full report: DECISION_LOG §20.
- **Re-verified Session 7 (2026-10-13, read-only; DECISION_LOG §21):** same branch/HEAD/untracked
  set, no stash; index = HEAD with 2 entries (`.gitignore`, `LICENSE`), none under the received
  folders; `core.filemode=false`, `core.symlinks=false`, `core.ignorecase=true`; 409 files, 0
  non-ASCII paths, 0 case-only collisions; 53 of the 151 lack a final newline; 1 empty directory
  inside ignored `obj/` (`…/obj/Debug/net8.0/staticwebassets`); session docs LF-only; archive
  SHA-256 unchanged.
- **Session 8 (2026-10-13, read-only; DECISION_LOG §22):** Git status at session start (harness
  snapshot): `main`, HEAD `af89144`, same four untracked items. No `.gitattributes` under the
  received folders; `.git/info/` only `exclude`; `core.attributesFile` unset; system
  gitattributes only `diff=astextplain` (no received file matches); `filter.lfs.*` defined in
  system config but no `filter=lfs` attribute (inactive); no `$Id` in received files; no
  `color.*`/`diff.noprefix`/`diff.relative`/`apply.*` config. Branches, index, stash and archive
  hash were **not** re-checked in Session 8.
- **Re-verified Session 9 (2026-10-13, read-only; DECISION_LOG §23):** `main`, HEAD `af89144`,
  only `main` local (+ `origin/main`), no stash, same four untracked items, index = HEAD
  (`.gitignore`, `LICENSE` only), `core.autocrlf=true` (system), no `.gitattributes`, archive
  SHA-256 unchanged. **Not re-checked:** 409/151/258 counts, line-ending counts, attribute sources.
- **Re-verified Session 11 (2026-10-13, read-only):** `main`, HEAD `af89144`, only `main`, no
  stash, same four untracked items, index hash (`ls-files -s` SHA-256) `cc7be7f6…053b`
  (`.gitignore`, `LICENSE` only; `ls-files -v` = 2 × `H`); `.git/info/` only `exclude`;
  `core.attributesFile`/`core.excludesFile` unset; only system `core.autocrlf=true` and
  `diff.astextplain.textconv`; 409 files (408 + 1), 110 directories, 1 empty directory, 0
  non-regular entries, longest relative path 119 chars. Unchanged after the backup.
- **Re-verified Session 12 (2026-10-13, read-only):** `main`, HEAD `af89144`, same four untracked
  items; `DLMS-Backups/` contains only `received_20261013-152404/`. Only docs edited (approval (A)
  records); `CLAUDE.md` unchanged; docs LF-only.
- **Independent backup (Session 11, DECISION_LOG §26):**
  `H:/Internship Project/DLMS-Backups/received_20261013-152404/` — `files/` (both folders) +
  `_verification/` (manifests, `RESULT.txt` = `verified`). Never modify, delete or restore from
  it without the user's explicit approval. Not D-07's formal R2; not yet the T4 `<backup-root>`.
- Inventory: 409 files under `DefaultTemplate/` + `knowledge Docs/` = 152 candidates + 257
  ignored (`bin/`, `obj/` only); 151 after the `*.user` rule.
- `DefaultTemplate/**/bin|obj` exist from a build (gitignored).
  `DefaultTemplate/DefaultTemplate/DefaultTemplate.csproj.user` exists (to be ignored in 0.1).
- Archive `knowledge Docs/Requirement/1st discussion.md` SHA-256:
  `6826f819e3ea5cd6c756cababcd4b2e17bd5dd95199bfcb0e31829c07255a86b` (re-verified Session 3 and
  Session 5 M13, full hash, `sha256sum`).

## 3. Phase status
- **Phase 0: not started.** Current position: before step 0.1, Gate 1.
- Implemented and verified: **nothing** (no application code, migration or DB changed).
- Implemented but unverified: nothing.
- Documented only: architecture decisions and Phase 0 plan (`PROJECT_DECISIONS.md`,
  `PHASE_0_PLAN.md`), updated with the user's Session 2 approvals (M5, M6) and Session 3
  answers (M8); M9 requirements FR-M9-01..06 recorded (`requirements.md`, `business-rules.md`,
  D-44..D-47, D-15/D-31 M9 extensions) — **all Proposed/Pending, none implemented**.
  M10–M12: FR-M10-01 ([Confirmed] requirements + archive reconciliation table), BR-M10-01..14,
  D-48 (borrowing policy system on the archive's `LibraryPolicies` table; D-44/D-45
  Superseded), tests P1–P21 — **D-48 wholly Proposed, no values selected; not implemented, not
  verified** (concurrency protection Proposed until P12, P14, P17–P19, P21 pass).

## 4. Evidence (last known)
| Check | Result | Evidence |
|---|---|---|
| `dotnet build DefaultTemplate/DefaultTemplate.sln` | Success, 0 errors, 2 × NU1903 (AutoMapper 12.0.1) | Run in Session 1, SDK 10.0.203 |
| Run the app | **Not run** — net8.0 target, only .NET 10.0.7 runtimes installed | `dotnet --list-runtimes` (Session 2) |
| Tests | **None exist** | — |
| SQL Server | `MSSQL$SQLEXPRESS` running; LocalDB `MSSQLLocalDB` available | Session 1: `Get-Service`, `sqllocaldb info` |
| `dotnet-ef` | Not installed | Session 1: `Get-Command dotnet-ef` |
| Startup DB hazard | `Program.cs:47` → `DbSeeder.SeedAsync` → `MigrateAsync()`; `appsettings.json` → `Database=DefaultTemplate` on `.\SQLEXPRESS`; `UserSecretsId` present | Code read, Session 2 |

## 5. Approved decisions (summary — details in `PROJECT_DECISIONS.md`)
- D-01/D-02 stack and v1 scope · D-03 keep `DefaultTemplate` · D-04 Bangla talk / English docs ·
  D-05 docs structure (no duplication).
- **D-12 Repository + UoW:** services inject only `IUnitOfWork` and use its typed repository
  properties; one scoped `ApplicationDbContext`; repositories never call `SaveChanges`; no
  service locator. Lazy creation inside the UoW approved **only while repositories need nothing
  but the DbContext**; direct repository injection forbidden by test E3 (not compile-time).
- D-10 layer boundaries **with enforcement** (Services: no Data/EF/`Microsoft.AspNetCore.App`;
  `Microsoft.Extensions.Identity.Core` only). D-13 repository contract (signatures at Gate 2).
- D-14 transactions/retries — approved in principle; **must be proven** (E2, F1–F4, G1).
- D-15 fetch only needed data; Users list = bounded count-and-page, correctness first.
- D-17 remove AutoMapper. D-20/D-21/D-22/D-26 central `AuditLogs` via interceptor, `IAuditable`
  allowlist, Identity excluded. **D-23 opt-in `.Audited()`/`.NotAudited()` per property.**
- **D-37 DB-backed permission authorization** (approved in principle; done only when A4/A7–A9
  pass). D-33 claims factory → Superseded by D-37.
- D-24 security events (principle) · D-31 security baseline · D-34 error handling.
- D-06 gates (Gate 1 = 0.1–0.5, Gate 2 = 0.6–0.8, Gate 3 = 0.9–0.12), branch `phase-0/baseline`.
- D-40 never touch existing DBs; dev DB `CseLibrary`; tests `CseLibrary_Test_{guid}` with guard.
- D-41 `src/CseLibrary.*` + `tests/CseLibrary.Tests` · D-42 .NET 10 LTS (compatibility checks) ·
  D-43 test strategy.
- **D-29 HiLo: preferred candidate only** — demo in disposable LocalDB `CseLibrary_Test_{guid}`
  (never the dev DB) + migration/script diff + approval before any migration change (0.11a).
- **M8:** D-33 → D-37 timing accepted (D-37 now states its security rationale); D-40 guard
  checked before migrate and cleanup, cleanup only of the run's own DB (test H1); D-14 rule 3
  per-use-case duplicate protection approved in principle, assessment at Gate 2.

- **M9 (Session 4):** no approved decision changed. New: D-44 (limit enforcement, Proposed),
  D-45 (outstanding-loan rule, Pending — conflict), D-46 (jQuery Load More, Proposed), D-47
  (naming, Pending — conflict with D-41). D-15 / D-31 extended (Proposed). Tests A10, A11, E4
  added to Phase 0; I/J/K tests for later phases in `requirements.md`.
- **M10/M11 (Session 5):** no approved decision changed; **D-48 is Proposed** (not listed as
  approved). The user's own requirements are recorded as [Confirmed] in FR-M10-01: per-category
  configuration by authorized staff, values configurable / logic not, no defaults without
  approval, **no reservation/request workflow** (Q-M9-2 → (i)), no generic rule engine.
  D-44, D-45 → Superseded by D-48. I1–I10 → superseded by P-tests. No Phase 0 step changed.
  M12 audit: no status changed; corrections recorded in DECISION_LOG §16.
- **M13 / approval / M14 (Session 5):** M13 five corrections (§17). Documentation package
  approved 2026-10-13 — documentation only, no status changed (§18). M14: **D-07 Git branching
  workflow — Proposed**, not approved (§19); D-06 stays Approved and unchanged (only a "pending"
  note added).
- **Session 6:** no approved decision changed. D-07 revised to **v4, still Proposed** (§20);
  user's accepted directions (not approval): Option A, gate branches, local-only, `--no-ff`
  integration merges, revised working-tree rule, **option (b) review-before-commit**. T1–T5
  proposed only. D-06 "pending" note now points at the v4 partial supersession (branch clause,
  M8 §4 items (2)–(4), correction exception).
- **Session 7:** no approved decision changed. D-07 revised to **v6, still Proposed** (§21).
  **T1-EOL (LF for new authored files) is a separate Pending decision** — not confirmed by D-07
  approval; needed before 0.2. D-06 "pending" note points at the v6 wording in DECISION_LOG §21.
- **Session 8:** no approved decision changed. D-07 v6 review: F1 (T1 claim too broad, medium),
  F2 (T4 deletion/untracked contradiction, medium), F3 (patch config dependence, low), F4 (index
  isolation claim, low). **v6.1 correction draft** in DECISION_LOG §22 — **not applied, not
  approved**; D-07 text in `PROJECT_DECISIONS.md` is still v6 (with a pending-v6.1 note).
- **Session 9:** no approved decision changed. Final F2 (c)4 and F4 Isolation wording accepted
  **for the review** (DECISION_LOG §23; supersede those parts of the §22 draft); targeted checks
  found no further issue (not a full audit). F1/F3 acceptance not stated. v6.1 not applied.
- **Session 10:** no approved decision changed. User direction only (DECISION_LOG §24): F1/F3
  with [F1+]/[F3+]; [R] received-files copy and [C] `CURRENT.txt` rule proposed; Variant A/B
  defined (B needs two extra consistency wordings). Nothing approved or applied.
- **Session 11:** no approved decision changed. Variant A review findings + proposed wordings
  (DECISION_LOG §25) — not applied. Independent received-files backup made and verified on the
  user's instruction (§26); same drive `H:` accepted by the user.
- **Session 12:** no approved decision changed status. Approval (A): v6.1 applied to the D-07 text
  (Variant A + G1, G2, M1, M9); D-07 still Proposed. Approval (D): `<backup-root>` =
  `H:/Internship Project/DLMS-Backups/` (DECISION_LOG §28).

## 6. Open — needs the user's decision (before the affected step)
**Required now:** only (1). Everything else is asked before the step it blocks.
1. ~~Approve the Session 2–5 docs~~ — **done 2026-10-13** (DECISION_LOG §18; documentation only,
   no status changed). Now needed: a **separate explicit approval of step 0.1**.
   Kept limitations: archive not read end to end; full FR/BR traceability in step 0.2; P/I/J/K
   tests are specifications, not evidence.
   **Before step 0.1 (M14, revised Sessions 7–8):** the user decides on **D-07 v6** and on the
   **v6.1 correction draft** (DECISION_LOG §22: F1, F2 mandatory; F3, F4 recommended) — its wording in
   `PROJECT_DECISIONS.md`, the D-06 partial-supersession wording (DECISION_LOG §21), the
   documentation impact list (DECISION_LOG §21, replacing §20) and **T1–T5**:
   T1 scope — `.gitattributes` (`* -text`; `text` for `/.gitignore`, `/LICENSE`) in step 0.1 +
   inventory generation and checks B1–B6 (pre-commit) / C1–C4 (post-commit); T2 corrections on
   the next open gate branch; T3 additive prospective/retrospective records, step 0.1's records in
   step 0.2; T4 three-level verified backups and **`<backup-root>` path + write permission** (user
   must supply; nothing written anywhere before); T5 six separate approvals.
   **Session 9 status / approval order (DECISION_LOG §23 checklist), each separate:**
   (A) documentation approval — apply v6.1 to D-07 (with the §23 final F2/F4 wording; F1/F3 only
   if the user accepts them), D-07 stays Proposed, show diff, stop; (B) D-07 approval + D-06
   partial supersession + impact list; (C) T1-EOL (before 0.2; recommended LF); (D) `<backup-root>`
   path + write authorization (before 0.1; user supplies — not probed); (E) step 0.1 start;
   (F) exact staged tree hash + commit message.
   **Session 10 additions (DECISION_LOG §24):** for (A) choose **Variant A** (F1+F2+F3+F4 with
   [F1+], [F3+]) or **Variant B** (F2+F4 + two consistency wordings; F1/F3 limitations stated);
   accept or drop [C] (`CURRENT.txt` / `RESULT.txt` / failed-folder rule); decide [R] received-files
   copy — R1 original source (recommended if it exists), R2 byte copy under `<backup-root>`
   (own approval), or R3 none (risk recorded) — **before step 0.1**.
   **Session 11 additions (DECISION_LOG §25–§26):** decide which review findings/wordings go into
   (A) (B4 wording; G1–G6 preconditions; manifest/RESULT rules M1–M9; R2-failure wording); [R] is
   practically covered by the verified independent backup (§26) — confirm whether D-07 should
   record it as R1-like, and whether `H:/Internship Project/DLMS-Backups/` is the T4
   `<backup-root>` (approval (D); drive `H:` already accepted).
   **Session 12 (DECISION_LOG §28):** (A) **done** (Variant A + G1, G2, M1, M9 applied; [C]
   included); (D) **done** (`H:/Internship Project/DLMS-Backups/`). **Still needed before 0.1:**
   (B) D-07 approval + D-06 partial supersession + impact list (§21; item 4 `CLAUDE.md` line
   consistent with F1) + T1–T5; [R] choice (R1-like copy exists, §26 — recommended: record R1);
   (E) step 0.1 start; (F) staged tree hash + message.
   **Separately, before step 0.2:** T1-EOL (LF for new authored text files) — Pending.
   If approved: apply exactly the impact list, show the diff (against a scratchpad snapshot),
   stop; step 0.1 still needs its own approval afterwards.
   **The line-ending issue blocks step 0.1 even if D-07 is rejected** (the D-06 path stages the
   same files).
2. Later in Phase 0: Q-M9-11 → 0.2; **Q-M9-9 (project prefix) + D-41 tooling → 0.3**; D-40
   connection string → 0.4; Q-M9-8 + D-14 explicit `ReadCommitted` (optional) → 0.8; Q-M9-10 →
   0.9. (Q-M9-1 superseded, Q-M9-2 answered, Q-M9-4 answered in part by M10.)
3. Can wait until the Lending phase — **no answer needed now:** Q-M10-1..9 (D-48 review,
   settings, initial values, constraints, overrides, activation, permission holder, due-date
   edit, category change), Q-M9-3, Q-M9-5. Phase 1: Q-M9-6, Q-M9-7.

Answered in Session 3 (M8, recorded): D-33 timing; HiLo demo DB = `CseLibrary_Test_{guid}`;
ambiguous-commit approach; strict stop after 0.1.

Technical note (Session 3, no decision needed): test E1 must check referenced assemblies, not
namespaces — `UserManager` is in `Microsoft.Extensions.Identity.Core` but namespace
`Microsoft.AspNetCore.Identity`. `Default.service.csproj` currently references
`Microsoft.AspNetCore.App` + `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (removed in 0.8).

Other open items:
- 0.3: `global.json` + local `dotnet-ef` tool manifest (not covered by D-41 approval).
- 0.4: where the dev connection string and seed admin password live (proposed:
  `appsettings.Development.json` → `CseLibrary`; password in user-secrets).
- Gate 2: exact repository signatures (D-13); `PageRequest`/`Result` details (D-15, D-34);
  per-use-case ambiguous-commit assessment — risk, mechanism, tests, limitations (D-14, M8).
- Gate 3: D-31 values, D-32 mechanism, D-37 `AspNetUserClaims` cleanup (separate approval),
  D-21/24/25/27 details, D-29 (after 0.11a), D-30.
- Later phases: see `PROJECT_DECISIONS.md` §9 "Can wait".

## 7. Safety restrictions
- **Never** connect to, migrate, reset, overwrite or drop the existing `DefaultTemplate`
  database on `.\SQLEXPRESS` or any other existing DB. **Do not start the app** until the
  resolved connection string (incl. user-secrets) is verified to be `CseLibrary` (step 0.4).
- No migration reset, destructive operation (DB drop, deleting files/migrations,
  `git reset --hard`, force push) or `AspNetUserClaims` cleanup without separate explicit approval.
- No commits without the user asking. **M6: after the docs diff is approved, only step 0.1 is
  authorized** — no blanket approval for later steps or gates.
- Do not implement Proposed/Pending items.

## 8. Next action (single)
0. **Session 13 start order** (same as Session 12; in (c) read the D-07 **v6.1** text and
   DECISION_LOG §28 first): (a) read this file; (b) verify read-only: `git status`,
   `git branch -a`, `git log --oneline -10`, `git stash list` (expected: only `main`, HEAD
   `af89144`, four untracked items), `git ls-files -s` (expected: only `.gitignore`, `LICENSE`)
   and `git diff --cached --quiet HEAD`, archive SHA-256,
   `git config --show-origin --get core.autocrlf` (expected `true`); backup folder
   `H:/Internship Project/DLMS-Backups/received_20261013-152404/` still present (read-only;
   optionally `sha256sum -c --strict` of `_verification/backup.sha256` from inside `files/`);
   (c) read `PROJECT_DECISIONS.md` D-07 (v6 + pending notes), DECISION_LOG §25–§26 (Session 11
   review + backup), §24 (Session 10 additions, Variant A/B), §23 (final F2/F4 wording, approval
   checklist), §22 (v6.1 draft and F1–F4).
   No edits, branches, staging or commits during this.
1. **Session 12: (A) and (D) done — next wait for approval (B)** (D-07 v6.1 + D-06 partial
   supersession + §21 impact list + T1–T5) and the [R] choice; on (B), apply exactly the §21 impact
   list, show the diff, stop. Historical text of this item follows.
   **Wait for the user's decision on approval (A)** (§6 item 1): Variant A or B, [C], and
   whether to apply; then D-07 and T1–T5, [R] received-files copy, and separately T1-EOL and
   `<backup-root>`. If the user approves (A): apply exactly the chosen variant — §22 replacement
   texts **with §23's final F2 (c)4 case and F4 Isolation sentence**, plus the accepted §24
   additions — to the D-07 entry (still Proposed), show the full diff of every changed file
   (against a scratchpad snapshot), stop. No new review cycle or v6.2 unless the user asks. The user has repeatedly said: D-07 stays Proposed until explicitly
   approved; approval of D-07 ≠ step 0.1 authorization ≠ T1-EOL confirmation ≠ backup location;
   never infer an approval from a request. If the user asks for further revisions, revise the
   D-07 entry in place, record the round in DECISION_LOG (new numbered entry), and stop.
   On approval: snapshot the docs to the scratchpad, apply **exactly** the DECISION_LOG §21
   "Documentation impact list", show the diff, stop.
2. Then wait for the **separate explicit approval of step 0.1**. With D-07 approved, run step 0.1
   per D-07 "Phase 0 mapping" 1–4: record `I_before` + `L_disk` in the authorized backup folder,
   `develop` from `main`, `feature/phase-0-gate-1` from `develop`, `.gitattributes` + `*.user`,
   `L_excluded`, single staging command, B1–B6, show per Step workflow 3, verified T4 backup,
   **stop — commit only after the user approves the exact staged tree and message**, then C1–C4
   and report (Step workflow 6).
   If D-07 is rejected: the D-06 path (`phase-0/baseline`) still needs a line-ending decision
   before staging — ask.

History: Session 3 §9–§11, Session 4 (M9) §12–§13, Session 5 M10–M13 §14–§17, documentation
approval §18, M14 §19, Session 6 D-07 review §20, Session 7 D-07 rounds 5–6 §21, Session 8
D-07 v6 review + v6.1 draft §22, Session 9 final F2/F4 + approval checklist §23, Session 10
clarifications + Variant A/B + [R]/[C] §24, Session 11 Variant A review §25 + independent
backup §26 + progress check §27. Files are untracked; diffs use scratchpad snapshots (not
persistent across sessions).

Files to read first for that work: `CLAUDE.md`, this file, `docs/PROJECT_DECISIONS.md` (D-06,
D-07), `docs/DECISION_LOG.md` §19–§22, `docs/PHASE_0_PLAN.md` (Gate 1, step 0.1), `.gitignore`.
