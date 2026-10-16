# Project Decisions

Authoritative register of decisions. **Only `Approved` items are binding.** `Proposed` items are
the assistant's recommendations awaiting the user's explicit approval; `Pending` items are open
questions the user has explicitly deferred or must answer. History of how each decision evolved:
`docs/DECISION_LOG.md`.

Status values: **Approved** · **Proposed** · **Pending** · **Rejected** · **Superseded**

"Source" refers to the user's messages in Session 1 (2026-10-12): **M1** kickoff + docs
strategy, **M2** "proceed carefully" + Mandatory Engineering Standards, **M3** revised
architecture (items 1–7) + Data Fetching rule (item 8), **M4** session-handoff request.
Session 2 (2026-10-12): **M5** revised-plan review (UoW as repository access point, §1–§8) and
request for a decision report; **M6** approval of the decision report (A1–A8, B, C).
Session 3 (2026-10-12): **M7** M6 text re-sent unchanged — no new decision (DECISION_LOG §9);
**M8** answers to the four open questions (D-33 timing, HiLo demo DB, ambiguous commits,
strict stop after 0.1).
Session 4 (2026-10-12): **M9** six new functional requirements (FR-M9-01..06: borrowing limit,
outstanding-loan restriction, pagination, jQuery AJAX loading, controller-level authorization,
project naming) and a documentation-consolidation request — **documentation only; no decision in
this file changed status because of M9** (DECISION_LOG §12).
Session 5 (2026-10-12): **M10** clarification — borrowing rules are one configurable,
per-category **borrowing-policy system** (FR-M10-01), not two competing fixed rules; no
reservation/request workflow. D-44, D-45 → Superseded by D-48 (Proposed). Documentation only
(DECISION_LOG §14).
Session 5, 2026-10-13: **M11** semantics correction — three independent borrowing checks
(`MaxActiveLoans`, `BlockIssueWhenAnyActiveLoan`, `BlockIssueWhenOverdue`), values vs logic,
no defaults without approval, D-48 kept wholly Proposed with an architecture/locking
assessment, decisions split into "now" vs "Lending phase" (DECISION_LOG §15).
Session 5, 2026-10-13: **M12** consistency audit — archive reconciliation (D-48 keeps the
archive table name `LibraryPolicies`; FR-14 renewals/fines deferred, not discarded; FR-06 due
date preserved), overclaims in D-48 §4 corrected, tests P19 extended and P21 added, Q-M10-6/7
conflicts recorded. No status changed (DECISION_LOG §16).
Session 5, 2026-10-13: M13 five corrections (§17); documentation package approved, step 0.1
still needs separate approval (§18); **M14** Git branching workflow proposed — D-07 Proposed,
final wording awaiting approval (§19).
Session 6, 2026-10-13: D-07 revised v1 → v4 at the user's request (working-tree safety without
per-checkout approval; step vs merge commit semantics; review-before-commit; baseline integrity
check; correction exception; byte-for-byte line endings T1; correction location T2; records T3;
verified backups T4; separate approvals T5). **D-07 still Proposed; no status changed; nothing
executed** (DECISION_LOG §20).
Session 7, 2026-10-13: D-07 revised v4 → v5 → v6 at the user's request (T1 inventory generation,
pre-existing index entries, pre- vs post-commit checks B1–B6 / C1–C4; T1-EOL split off as a
separate Pending decision; T3 additive records and step 0.1 records in step 0.2's scope; T4
three-level verification with bundle-based reconstruction and stated limitations; no write before
the backup location is authorized). **D-07 still Proposed; no status changed; nothing executed**
(DECISION_LOG §21).
Session 8, 2026-10-13: targeted T1/T4 review of D-07 v6 (findings F1–F4) and a **v6.1 correction
draft** (F1 T1 byte-exact scope + attribute checks, F2 T4 deletion/untracked overlap — mandatory;
F3 fixed patch commands, F4 isolation limits — recommended). **Draft not applied; D-07 text below
is still v6; D-07 still Proposed; nothing executed** (DECISION_LOG §22).
Session 9, 2026-10-13: two targeted v6.1 corrections finalized — F2 check (c)4 second case and
F4 "Isolation" sentence (exact text in DECISION_LOG §23, replacing those parts of the §22 draft);
approval-readiness checklist prepared. **v6.1 still not applied; D-07 text below is still v6;
D-07 still Proposed; nothing executed** (DECISION_LOG §23).
Session 10, 2026-10-13: clarifications (F1/F3 clause list, scope of approvals (A)/(B), T4 failure
handling, backup-root requirements); proposed additions [F1+] environment scope, [F3+]
configuration limits, [R] received-files copy, [C] `CURRENT.txt`/failed-folder rule; Variant A/B
diffs shown in chat. **Nothing approved; v6.1 still not applied; D-07 text below is still v6;
D-07 still Proposed; nothing executed** (DECISION_LOG §24).
Session 11, 2026-10-13: read-only review of Variant A (B4 check-attr parsing, multi-state
reconstruction gaps G1–G6, manifest/RESULT rules M1–M9, R2-failure wording) — **not applied; D-07
still v6 and Proposed** (DECISION_LOG §25). Separately, on the user's instruction, an independent
verified backup of `DefaultTemplate/` + `knowledge Docs/` was made at
`H:/Internship Project/DLMS-Backups/received_20261013-152404/` (DECISION_LOG §26; not a D-07
decision).
Session 12, 2026-10-13: user approved (A) documentation — **v6.1 applied to D-07 (Variant A + §25
G1, G2, M1, M9)** — and (D) `<backup-root>` = `H:/Internship Project/DLMS-Backups/` with write
authorization. **D-07 still Proposed** (approval (B) not given); step 0.1 not authorized; nothing
executed (DECISION_LOG §28).
**Session 13, 2026-10-13:** user approved the assistant's consolidated senior review package
(items 1–13, DECISION_LOG §29): **D-07 v7 (simplified) Approved** (v6.1 Superseded), T1
`.gitattributes`, R1 recovery source, T1-EOL = LF, D-47 Rejected (`CseLibrary.*` kept), D-41
tooling, D-40 connection-string location + base `appsettings.json` fix, Q-M9-8/10/11 answered,
step 0.2 slimmed, D-14 `ReadCommitted` deferred to Phase 3, D-16 Approved. Review tooling added
to the gate checklist (D-07 v7). Step 0.1 authorized by the same approval.
**Session 14, 2026-10-13:** Gate 1 executed — commits `1fdbe5e` (0.1), `9cf511a` (0.2), `3c8d98a`
(0.3), `468a82c` (0.4), `22d27ae` (0.5), each after the user's OK; Gate 1 review findings reported
(DECISION_LOG §35), **Gate 1 awaiting approval**. User decisions: no DB username/password and no seed
password (app-start check moved into tests, D-40 note); **M15** new requirements (overall
architecture + DB/ER `archify` diagrams, multi-copy titles with holder, full lending history,
senior-quality DB design) to be designed **after the Gate 1 report** (§33). D-37 finding corrected
by test evidence. Open items: §9 "Session 14" table.
**Session 15, 2026-10-13:** Gate 1 review findings fixed — `8504115` (DECISION_LOG §37, tests 16/16);
docs `346496a`, `97891e8`. M15 confirmed for after Gate 1, before Gate 2. **Gate 1 merge → `develop`
still awaiting the user's explicit approval** (§38).
**Session 16, 2026-10-13:** Gate 1 approved → merged into `develop` (`bbee8dc`; build 0 errors, tests
16/16; DECISION_LOG §39). No decision status changed. M15 delivered as **D-49 (Proposed)** +
`database.md` + two archify diagrams (DECISION_LOG §40); ER diagram added; **D-50 (naming, audit
columns) and D-51 (`RecordStatus`) Approved** by the user (§41), both implemented in Gate 2 step 0.8.
`Archive §n` = `docs/archive/1st discussion.md` (moved from `knowledge Docs/Requirement/` in step 0.2).

---

## 1. Product and process

### D-01 Technology stack — Approved
ASP.NET Core MVC, C#, EF Core, SQL Server, Razor Views + Bootstrap + JS, ASP.NET Core Identity,
email + scheduled background processing, xUnit. **Source:** M1, Archive §8.

### D-02 First-release product scope — Approved
Separate Book (title) and BookCopy (physical copy, unique AccessionNumber); Student and Department
Fellow members; staff operations **and** member self-service portal; issue, return, history;
return-confirmation email; reminders 3 days before due date, on due date, and while overdue;
**no fines** in v1. Supersedes Archive SRS v1.0 §1.3, which deferred the portal.
**Source:** M1, Archive §8. Detailed business rules (BR-01..BR-18, Archive §9 "Work Flow" §3)
are **Proposed** — not department-approved yet.

### D-03 Start from `DefaultTemplate` — Approved
Keep the existing template (Identity, permission-based authorization, admin UI, Serilog, seeder)
as the base. Do not rewrite working components without a concrete reason. **Source:** M2 item 3.

### D-04 Language — Approved
Talk to the user in Bangla (English technical terms kept). Project docs and code in English.
**Source:** M1, M2 item 6.

### D-05 Documentation system — Approved
Structure approved in principle (M2 item 1). M4 defined the session files: `CLAUDE.md`,
`docs/SESSION_HANDOFF.md`, `docs/PROJECT_DECISIONS.md` (this file — supersedes the earlier
proposed `docs/decisions.md`), `docs/PHASE_0_PLAN.md`, `docs/DECISION_LOG.md`.
**Approved (M6 B), created in step 0.2:** `README.md`, `requirements.md` (FR with tags
[Confirmed]/[Assumption]/[Proposed]/[Deferred]), `business-rules.md`, `architecture.md`,
`database.md`, `development.md`, `status.md` (traceability FR→BR→entity→service→controller→test).
Condition (M6): avoid duplication — each fact has one owning file; other files link to it. The earlier
proposed `open-questions.md` is **Superseded** by section 9 of this file.
**M9 note:** `docs/requirements.md` and `docs/business-rules.md` were **seeded early** (Session 4)
with the M9 requirements/rules only, because M9 asked for them to be recorded there; step 0.2
consolidates the archive content into the same files (not new files).
Archive: original discussion stays byte-identical (SHA-256
`6826f819e3ea5cd6c756cababcd4b2e17bd5dd95199bfcb0e31829c07255a86b`); move to `docs/archive/`
via `git mv` in step 0.2.

### D-06 Review gates and commit policy — Approved
Phase 0 is split into three gates: **Gate 1 = 0.1–0.5, Gate 2 = 0.6–0.8, Gate 3 = 0.9–0.12.**
Stop at each gate until the user reviews and explicitly approves the next stage. One commit per
step. Do not commit documentation without the user's review. **Source:** M3 item 7, M4, M5 §8.
**Branch — Approved (M6 B):** `phase-0/baseline`, created from `main`.
At every gate report: commit hashes, changed files, build/test results, unresolved issues, next
proposed action (M5 §8). Stop earlier if a destructive operation, unresolved architectural
decision or failed acceptance test needs a decision. **M6 C:** approval of the documentation
authorizes **step 0.1 only**; each later step/gate needs explicit approval — no blanket approval.
**Confirmed strictly (M8 §4):** (1) show the documentation changes; (2) after approval run
step 0.1 only; (3) verify baseline, branch/status and repository state; (4) stop and report;
(5) steps 0.2–0.5 only after explicit review and approval of the next stage.
**Documentation approval (2026-10-13, DECISION_LOG §18):** the documentation package was
approved; at the user's instruction this does **not** unlock step 0.1 — step 0.1 needs its own
separate explicit approval. Status of this decision unchanged.
**M14 (pending):** D-07 (Proposed, v6 — Session 7) would supersede **in part** — the branch
clause, items (2)–(4) of M8 §4, and add a correction-commit exception to "one commit per step" —
if the user approves it; until then this decision stands exactly as written. Proposed
partial-supersession wording is in DECISION_LOG §21.
**Partially Superseded by D-07 v7 (Session 13, DECISION_LOG §29):** the branch clause
(`phase-0/baseline` → gate branches) and M8 §4 items (2)–(4) (→ D-07 v7 approval rule); a
correction commit on the open gate branch is an allowed exception to "one commit per step".
Gates, stop-at-gate and gate reports stay as written.

### D-07 Git branching workflow — Approved (v7, simplified; Session 13, 2026-10-13)
Supersedes D-07 v6.1 (kept below as history). **Source:** user approval of the Session 13
senior review package, DECISION_LOG §29.
- **Branches:** `main` (stable) · `develop` (integration) · `feature/<name>` one per review gate
  (Phase 0: `feature/phase-0-gate-1`, `-gate-2`, `-gate-3`) from the latest `develop`. Flow:
  feature → `develop` after gate approval → `main` after release review; merges `--no-ff`; no
  squash/rebase of shared branches. **Local only** — push / PRs need separate approval.
- **Commits:** before each commit the assistant shows the staged file list
  (`git diff --cached --stat`, plus the diff for authored files) and the message; commit only
  after the user's OK. Corrections = an extra commit on the open gate branch.
- **Separate approvals stay:** step commit, gate merge, release merge, push, branch deletion,
  destructive operations, migrations/DB (D-29, D-30, D-40).
- **Line endings (T1):** root `.gitattributes` = `* -text`, `/.gitignore text`, `/LICENSE text`
  → received files stored byte-for-byte despite `core.autocrlf=true`. **T1-EOL:** new authored
  text files are written with LF.
- **Step 0.1 check:** committed file set = backup manifest minus ignored paths (`bin/`, `obj/`,
  `*.user`; expected 151), and every committed blob's SHA-256 equals the manifest value.
- **Recovery source (R1):** `H:/Internship Project/DLMS-Backups/received_20261013-152404/`
  (verified Sessions 11 and 13). Never modified, deleted or restored from without explicit
  approval. Optional `git bundle` into `DLMS-Backups/` at a gate end.
- **Gate review tooling:** before every gate report the assistant runs `/code-review` (correctness)
  and `/ponytail-review` (over-engineering) on the gate diff and reports findings; step 0.2
  (and later architecture changes) produce an `archify` architecture/dependency diagram from the
  real code. Findings are reported, not silently fixed.
- **Dropped from v6.1:** T4 three-level verification and bundle reconstruction, B1–B6 / C1–C4
  inventory machinery, `MANIFEST`/`RESULT`/`CURRENT.txt` rules, T3 records, six-way T5 split.
  **Why:** single-developer local repository; the real risks (line-ending conversion, loss of
  the received files) are covered by `.gitattributes`, the hash check and the verified backup.

### D-07 v6.1 Git branching workflow (M14) — **Superseded by D-07 v7 (Session 13)**; historical text below
**User direction (M14, 2026-10-13; revised Sessions 6–7) — preferences, not yet approval of this
entry:** Phase 0 mapping Option A; one feature branch per review gate; remote push and GitHub PRs
deferred (local-only); `--no-ff` integration merges, no squash/rebase of shared branches; revised
working-tree safety; review and approve the exact staged tree and commit message before every
commit (Session 6 option (b)); byte-for-byte storage (T1); corrections on the next open gate
branch (T2); prospective/retrospective records (T3); verified repo-external backups of uncommitted
authored work (T4); all approvals separate (T5); D-06 status and history untouched.
**Approving D-07 does not authorize step 0.1**, does not confirm the line-ending policy for new
authored files (T1-EOL, Pending) and does not name the backup location (T4, Pending). Step 0.1
additionally needs: the T1 `.gitattributes` change in its scope, `<backup-root>` named by the user
with explicit permission to write there (T4 — given separately in Session 12, approval (D)), and
its own separate explicit approval.
History of v1–v6 and the read-only findings behind them: DECISION_LOG §19–§21.
**v6.1 applied (Session 12, documentation approval (A), DECISION_LOG §28):** Variant A — §22
items 1–13 with §23's final F2 (c)4 second case and F4 Isolation sentence, §24 additions [F1+],
[F3+], [R], [C] (with its consequential edits), and from the §25 review only G1, G2
(preconditions), M1 (manifest exclusions) and M9 ([R] first sentences). Not applied: B4 wording
B1–B3, G3–G6, M2–M8, R2-failure wording. **Applying (A) does not approve D-07** — status stays
Proposed until approval (B).

**Branches.** `main` — stable, reviewed, release-ready. `develop` — integration of approved work.
`feature/<short-name>` — one per **review gate** (Phase 0: `feature/phase-0-gate-1`, `-gate-2`,
`-gate-3`), created from the latest approved `develop`. `hotfix/<short-name>` — optional, from
`main`, merged back into `main` and `develop`; same approvals and tests as any other change.
Flow: `main` → `develop` → `feature/*` → review → `develop` → release review → `main`. Feature
branches never merge directly into `main`.

**Local-only until push is approved.** No branch, tag or commit is pushed to `origin` and no Pull
Request is opened until the user explicitly approves remote pushes. Until then, "review" means:
the local diff (and `git log --graph`) is shown to the user; a step diff is reviewed before its
commit, and a merge happens only after the user's explicit approval of that merge.

**Approvals (T5) — each separate and explicit, never inferred from another:**
1. starting a step; 2. the exact staged tree hash and commit message of that step; 3. a correction
request (scope); 4. the exact staged tree hash and commit message of that correction; 5. a gate
review; 6. a gate merge — likewise the `develop` → `main` release review and release merge.
Also separate and unchanged: remote pushes, branch deletion, the backup location and any backup
deletion or restore (T4), the line-ending policy for new authored files (T1-EOL), migration and
database approvals (D-29, D-30, D-40), destructive operations.

**Line endings — byte-for-byte storage (T1).** Step 0.1 adds a root `.gitattributes` (authored,
LF), present in the working tree before anything is staged:
```
# D-07 T1: store and check out every file byte-for-byte (no line-ending conversion).
* -text
# Committed in af89144 before this rule and stored normalized (LF); keep that behavior.
/.gitignore text
/LICENSE text
```
Consequences of these rules:
- **Reach:** the file is versioned, so its rules apply in every commit and branch that
  contains it (not in commits without it: `af89144`; `main`/`develop` until the Gate 1
  merge). In any clone they take precedence over `core.autocrlf`, `core.eol`, and over
  `text`/`eol` set in `core.attributesFile` or the system attributes file. They do **not** take
  precedence over that clone's `$GIT_DIR/info/attributes` or over a `.gitattributes` in a
  subdirectory, and they do not unset other content-changing attributes (`filter`, `ident`,
  `working-tree-encoding`), which any attributes source — including lower-precedence ones — can
  still set. A clone's local attribute sources are not versioned and are outside this
  decision's control.
- **`* -text`:** Git performs no line-ending conversion for these paths, on add or on
  checkout, wherever the rule is in effect (see Reach). This alone does not guarantee
  byte-for-byte storage or checkout: bytes are unchanged only where, for that path in that
  clone, `text` resolves to `unset` and `filter`, `ident` and `working-tree-encoding` are
  `unspecified`. Byte-for-byte **storage** is established per commit by checks (step 0.1: B4
  before the commit, C2 after it; later steps: policy (4)). Byte-for-byte **checkout** in
  another clone or on another machine, or after any attribute-source change, is not guaranteed
  or verified by this decision. Where these conditions hold, the received files keep their
  current form (29 CRLF-only, 98 LF-only,
  24 without any newline; 53 without a final newline). It does not mark files binary: diff, merge
  and blame stay textual (CRLF lines may show `^M` whitespace highlighting); a line-ending change
  shows as a change of every affected line. Where these conditions hold, `git add --renormalize`
  causes no change on these paths.
- **New and edited files:** stored exactly as the editor or tool wrote them. On Windows, LF files
  stay LF in the working tree (no checkout conversion). A tool that writes CRLF (e.g. IDE templates,
  `dotnet` scaffolding, EF migration generation) produces CRLF blobs; only the review check (rule 4
  below) detects a deviation. Per-file conventions in the repository are therefore permanently
  mixed, by design.
- **`/.gitignore text`, `/LICENSE text`:** unchanged behavior — stored LF (as in `af89144`),
  checked out per `core.autocrlf`/`core.eol` (CRLF here, `i/lf w/crlf`); neither file appears as
  modified when `.gitattributes` is added. Step 0.1 shows `git ls-files --eol` for both to prove it.
- **`.gitattributes` itself** falls under `* -text`; its LF bytes are approved as part of step
  0.1's staged tree. That approval covers this one file only and does not confirm T1-EOL.
Policy for authored and generated files: (1) an edit keeps the existing file's line-ending
convention — no file is ever converted (the session docs are LF-only, verified Session 7);
(2) **new authored text files use LF — Pending (T1-EOL)**: a separate decision, not confirmed by
approving D-07; until the user confirms it, no new authored text file is created other than
`.gitattributes` in step 0.1, so step 0.2 cannot start before it is decided; (3) generated files
(e.g. EF migrations) are committed as the tool wrote them; (4) Step workflow 3 reports, for every
staged authored or generated path, `git ls-files --eol` and `git check-attr text eol filter ident
working-tree-encoding`. Expected: `text: unset` (`/.gitignore`, `/LICENSE`: `set`) and the other
four `unspecified`. Any convention change, deviation or other attribute value is flagged and stops
the step for approval; (5) attribute sources: (i) creating, changing or deleting a `.gitattributes`
anywhere in the tree (root or subdirectory) changes the T1 rule and needs explicit approval of that
path and its content. A `.gitattributes` found under a received folder (in `L_disk` or a later
diff) → stop; (ii) `$GIT_DIR/info/attributes`, `core.attributesFile` and the system attributes
file are local configuration: the assistant never creates or changes them, since every Git config
or attribute-source change needs explicit approval. Their per-path effect is checked by B4 and (4)
only at the time of each check; (iii) any `git add --renormalize` needs explicit approval and never
applies to received files; (6) renames (`git mv`, steps 0.2/0.3) keep
blob ids unchanged (verified).

**As-received verification (T1, step 0.1 baseline).**
*Conventions.* `F` = the two received folders `DefaultTemplate/` and `knowledge Docs/`. All lists
hold repository-relative paths, NUL-separated (`-z`/`-print0`), sorted with `LC_ALL=C sort -z`
and compared byte-for-byte with `comm`; counts are reported. Paths are compared as exact bytes
even though `core.ignorecase=true` here; a case-only path collision → stop (none exists, verified
Session 7). The lists are written to `inventory/` in step 0.1's backup folder (T4), never into the
repository. All read-only Git commands of the generation steps and of B1–B6 / C1–C4 run as
`git --no-optional-locks …` from the repository root.
*Generation, in this order:*
1. **`I_before`** — the whole index before step 0.1 changes anything: `git ls-files -s -z`.
   Precondition: index = HEAD (`git diff --cached --quiet HEAD`), no unmerged entries
   (`git ls-files -u` empty), and no index entry under `F`. Expected: exactly `.gitignore` and
   `LICENSE` with their `af89144` blobs. Otherwise stop before anything is created or staged.
   Also reported (not assumed from earlier sessions): contents of `$GIT_DIR/info/attributes` if
   present, and `git config --show-origin --get-all core.attributesFile`. These are reported only;
   the per-path effect of all attribute sources is checked by B4.
2. **`L_disk`** — the complete received-file inventory: `find F -type f -print0`, each file with
   size and SHA-256; recorded before step 0.1 changes anything and recorded again immediately
   before staging — the two recordings must be byte-identical. `L_nonregular` —
   `find F ! -type f ! -type d` and `find F -type d -empty` — listed and reported (not trackable;
   none created or removed).
3. Branches created; `.gitattributes` written; `*.user` added to `.gitignore` (Phase 0 mapping).
4. **`L_excluded`** — after step 3, before staging: `git ls-files -z -o -i --exclude-standard -- F`,
   each path with its rule from `git check-ignore -v -z --no-index --stdin`. Every entry must
   match `.gitignore` with pattern `[Bb]in/`, `[Oo]bj/` or `*.user`; any other source or pattern →
   stop.
5. **Staging — one command:** `git add -- .gitattributes .gitignore F` (no `-f`, no `-A`, no
   `--renormalize`, no other path).
6. **`I_after`** = `git ls-files -s -z`; **`L_staged`** = the `I_after` entries under `F`. Because
   `I_before` has no entry under `F`, every `L_staged` entry was added by this step.
   **`D_step`** = `git diff --cached --name-status --no-renames -z HEAD` — the complete staged
   change of the step.
*Pre-commit checks* (Step workflow 2–3; before any commit exists):
- **B1 Inventory:** `L_staged` ∩ `L_excluded` = ∅ and `L_staged` ∪ `L_excluded` = `L_disk`
  exactly; `git ls-files -z -o --exclude-standard -- F` is empty (nothing left unstaged and
  unexcluded). Expected at the Session 7 state: 151 + 258 = 409.
- **B2 Index scope:** `D_step` = exactly `A` for every `L_staged` path, `A .gitattributes`,
  `M .gitignore`, and nothing else; `I_after` minus `L_staged` equals `I_before` except the new
  `.gitignore` blob and the added `.gitattributes`; `LICENSE` entry unchanged; every entry stage 0;
  every `L_staged` mode 100644. No `L_staged` path is named `.gitattributes`; the root
  `.gitattributes` is the only one in `I_after`.
- **B3 Working tree:** after staging, SHA-256 of every `L_disk` file = its `L_disk` value.
- **B4 Staged blobs:** for every `L_staged` path, SHA-256(`git cat-file blob :<path>`) = its
  `L_disk` value; blob id = `git hash-object --no-filters -- <path>` (no `-w`);
  `git check-attr -z --stdin text eol filter ident working-tree-encoding` over all `L_staged`
  paths gives `text: unset` and `unspecified` for the other four. For `.gitattributes` the
  same; for `.gitignore` and `LICENSE`: `text: set`, the other four `unspecified`. Any other
  value → stop. These results describe this clone's attribute sources at the time of the
  check only.
- **B5 Archive (pre-commit):** working-tree file and staged blob SHA-256 = `6826f819…a86b` (D-05).
- **B6 Staged tree:** `T_staged` = `git write-tree` (the hash shown for approval);
  `git ls-tree -r -z T_staged` paths under `F` = `L_staged` with the `I_after` blob ids;
  `git ls-files --eol -- .gitignore LICENSE` = `i/lf … attr/text`; `git diff --quiet` (no unstaged
  change to a tracked file).
- **Immediately before the commit (Step workflow 5):** `git write-tree` = the approved `T_staged`;
  re-read `I_after` byte-identical; the message file byte-identical to the approved message.
*Post-commit checks* (Step workflow 6; only possible once `HEAD` is the new commit):
- **C1 Tree and commit:** `git rev-parse HEAD^{tree}` = the approved `T_staged`; `HEAD^` =
  `af89144`; `HEAD` is `feature/phase-0-gate-1`; the commit message is byte-identical to the
  approved message.
- **C2 Committed blobs:** paths of `git ls-tree -r -z HEAD -- F` = `L_staged`;
  SHA-256(`git cat-file blob HEAD:<path>`) = its `L_disk` value for every path.
- **C3 Archive (committed):** SHA-256(`git cat-file blob "HEAD:knowledge Docs/Requirement/1st discussion.md"`)
  = `6826f819…a86b`.
- **C4 State:** working-tree SHA-256 of every `L_disk` file still = its `L_disk` value;
  `git diff --cached --quiet HEAD`; `git status --short` shows only the untracked session docs;
  `main` and `develop` at `af89144`; nothing pushed.
No B-check is reported as proving a committed state, and no C-check is run or claimed before the
commit exists.
**Scope of these checks (environment).** B1–B6, C1–C4 and policy (4) describe this repository on
this machine — its working tree, index, object store, Git version, configuration and attribute
sources — at the moment each check runs, and nothing else. C2 proves that the committed blobs are
byte-identical to the received files; because a blob's id is derived from its bytes, every intact
clone that contains the commit holds the same blob bytes. The checks do **not** prove that
checking out those blobs in another clone, on another machine, with another Git version, or after
any attribute-source or configuration change writes byte-identical working-tree files. That would
need its own check in that environment (e.g. SHA-256 of the checked-out files against the
`L_disk` manifest), which is not part of this decision.
**Any mismatch → stop at once and report** the failed check, the paths and the expected vs actual
values. Received files are never modified, re-saved, converted, renamed, moved, deleted or
re-created to make a check pass; no `.gitattributes`, `.gitignore` or Git config change, no
re-staging, unstaging, renormalization, amend, reset or further commit is made for that purpose.
The index and working tree are left as they are; a failed B-check means no commit; any remedy
needs the user's explicit approval (before the commit: a revised step 0.1 review; after it: a
correction under the exception rule).

**Step workflow (supersedes items (2)–(4) of D-06 "Confirmed strictly (M8 §4)").** Each step
starts only after its own explicit approval (D-06, M6 C), then:
1. **Complete the step** on its gate branch, without committing. Nothing else is started.
2. **Verify (pre-commit)** the final content with the checks the phase plan requires for that step
   ("Done when"; build/tests where applicable; for step 0.1 the B-checks of T1).
3. **Show for review and stop:** before staging, the index must equal `HEAD`
   (`git diff --cached --quiet HEAD`) — a pre-existing staged entry → stop and report. Then stage
   exactly the step's changes and show the complete staged diff of every authored change, the
   records-only section (T3), the proposed commit message verbatim, the pre-commit verification
   output, `git status --short --ignored`, `git ls-files --eol` for authored/generated files and
   the staged tree hash (`git write-tree`). For content committed **as received**, the `--stat`,
   full file list, SHA-256 manifest, B-check results (T1) and the complete patch of every authored
   change (e.g. `.gitignore`, `.gitattributes`) form a **baseline inventory and integrity check —
   not a substitute for inspecting content**. Before approving, the user may request and inspect
   any file's content or patch; each request is answered read-only, without changing the staged
   tree. A verified backup (T4) is made at this stop.
4. **Explicit user approval of the exact staged tree hash and the exact commit message.**
   Requested changes are made in the working tree, then 2–3 are repeated in full (re-verify;
   re-show the complete diff, new tree hash, message; new backup). Approval of an earlier tree or
   message never carries over to a changed one.
5. **Exactly one step commit**, of exactly the approved tree with exactly the approved message,
   after re-checking that `git write-tree` equals the approved hash. If anything differs from what
   was approved, do not commit — stop and report.
6. **Post-commit verification, report and stop:** commit hash, `git show --stat HEAD`, proof that
   `HEAD^{tree}` equals the approved tree hash, the post-commit checks (for step 0.1 the C-checks
   of T1), `git status`, `git log --graph --all --oneline`. A failed post-commit check → stop;
   nothing is amended or reset. The next step needs its own approval.
Commit message: subject `Step <N>: <summary>`; body includes the `Records:` line (T3) and
`Review: tree <hash> approved by the user before this commit (D-07)`.

**Records (T3).** Records-only updates are limited to:
- `docs/PHASE_0_PLAN.md` — checklist marker and evidence of the step(s) concerned;
- `docs/SESSION_HANDOFF.md` — header date line, §2 repository state, §3 phase status, §4 evidence,
  status of items in §6, §8 next action;
- `docs/DECISION_LOG.md` — entries recording events that already happened (approvals given,
  commits, merges, verification results) — never a new or changed decision.
Anything else (decision status, requirements, business rules, plan step text, rules, code,
configuration) is not a record and needs its own step or decision approval.
**Prospective records** — facts known before the commit and true once it exists (checklist `[x]`
"committed as this step's commit", pre-commit verification evidence, next action) — are part of
the step's own diff. **Retrospective records** — facts known only after a commit or merge (commit
hash, tree-match proof, post-commit check results, approval dates, gate review/merge outcome,
merge commit hash; also for correction commits) — are written only when the next step starts and
are included in that step's commit.
**Additive.** A record adds facts about events that already happened; the only existing text it
may change is the state fields listed above (checklist marker, SESSION_HANDOFF §2/§3/§4/§6/§8),
updated to the current state. It never removes or rewrites approved non-record content or a
DECISION_LOG entry; a wrong earlier record is fixed only by a correction under the exception rule.
**Retrospective records never touch existing history.** They are added only as new hunks of a
later step commit. No committed step or correction is amended, reverted, rebased or otherwise
changed to add its own hash or results, and no merge commit (gate or release) ever receives
records — not in the merge itself, not by amending it, and not by a follow-up commit on `develop`
or `main`. Retrospective records of a gate's last step and of its merge go into the next gate's
first step commit (0.6, 0.9; after Gate 3 and after the release merge: the first step commit on
the next branch). Until such a commit exists, Git and the approved commit messages are the
authority. A separate records commit is not part of this decision.
**Step 0.1 records in step 0.2 (declared scope).** The session docs are untracked during step 0.1,
so step 0.1's commit contains no records (`Records: none — carried in step 0.2 (T3)`). Step 0.2's
declared scope therefore explicitly includes, in addition to its own step content and its own
prospective records, these records of step 0.1, classified "retrospective — step 0.1":
- `docs/PHASE_0_PLAN.md`: step 0.1 row `[x]`, commit hash, `HEAD^{tree}` = approved tree, B- and
  C-check summary, any correction commits listed under 0.1;
- `docs/SESSION_HANDOFF.md`: §2 (branches `develop`, `feature/phase-0-gate-1`; 0.1 commit hash;
  `main`/`develop` at `af89144`), §3 (0.1 done), §4 (B/C evidence), §6 (0.1 approval item
  closed), §8; the "Uncommitted review state" section replaced by the permanent pointer to
  `<backup-root>/CURRENT.txt` (T4 rule 2);
- `docs/DECISION_LOG.md`: an entry recording the step 0.1 start approval, the approval of its tree
  hash and message (with date), the commit hash and the B/C results.
Because these three files enter Git for the first time in step 0.2, their records hunks are shown
as a diff against a byte snapshot of each file taken immediately before the records are written
(`pre-records/` in step 0.2's backup folder), not against Git. Step 0.1's commit is never amended,
and no record goes into a merge commit.
Between a commit and the next step the working tree stays clean (apart from the known untracked
session docs before step 0.2), so no record is ever left uncommitted across a gate merge or branch
switch.
Identification and exclusion: the review shows a separate "Records-only" section listing each
records hunk (file, section, prospective/retrospective, which step or merge it records); the
commit message's `Records:` line lists the same; `git diff --cached --name-only` must lie within
the step's declared files plus the three records files; in those three files every hunk is
classified as step content or records — an unclassifiable hunk → stop and ask.

**Uncommitted review state and recovery (T4).** Three different things:
- **staged tree hash** — an identifier only: it proves equality but restores nothing (staged
  objects are referenced by no commit and can be lost through index changes, `git gc`/prune or
  disk loss);
- **handoff / STATE description** — says what exists and where; it holds no content;
- **verified backup** — a byte copy outside the repository that has passed all checks below.
**Location.** `<backup-root>` lies outside the repository and the session scratchpad and is
chosen by the user. **No backup is created until the user has explicitly named `<backup-root>`
and authorized writing there** — before that, nothing is written to any candidate location: no
folder, file, test write or permission probe. **Named (approval (D), Session 12, 2026-10-13,
DECISION_LOG §28):** `<backup-root>` = `H:/Internship Project/DLMS-Backups/`, with write
authorization for T4 backup folders; same drive as the repository — risk accepted by the user
(§26). This approval does not authorize step 0.1 or any backup before it. Backups are never
deleted, and nothing is restored from them, without explicit approval.
**Received files (not fully covered by T4 backups).** A T4 backup copies the authored change set
`A`, which at step 0.1's review stop includes the 151 staged received files. Not covered by any T4
backup: the 409 `L_disk` files from the start of step 0.1 until its first verified backup, and the
258 ignored files at all times (`history.bundle` contains commits only).
Independent copy — **Pending (user decision, before step 0.1)**, one of:
(R1) the user's original source of `DefaultTemplate/` and `knowledge Docs/` (e.g. the archive or
download they came from), kept outside the repository and confirmed by comparing its SHA-256 list
with `L_disk`;
(R2) a byte copy of all 409 `L_disk` files under `<backup-root>/received_<yyyymmdd-hhmmss>/`,
made only after its own explicit approval, after `L_disk` is first recorded (Phase 0 mapping 1)
and before anything is created or staged; verified only when the copy's SHA-256 list equals
`L_disk` byte-for-byte (T1 comparison rules). Its result is recorded like a T4 folder
(`RESULT.txt`); it is never modified, deleted or restored from without explicit approval;
(R3) no independent copy — only by the user's explicit decision, recorded with the risk: loss or
corruption of the working tree (disk failure, accidental deletion, a process outside the
assistant's control) before the commit loses the received files; after it, the 258 ignored files
(regenerable build output, not byte-identical) remain at risk, and the committed blobs share the
repository's disk.
Recommended: (R1) if the original source exists, otherwise (R2). Already available (Session 11,
DECISION_LOG §26): a verified independent copy of all 409 files at
`<backup-root>/received_20261013-152404/` (R1-like protection; not made by the R2 procedure).
**Contents.** A step's backup folder `<backup-root>/<step-or-correction-id>_<yyyymmdd-hhmmss>/` is
written at every review stop and before any session end or pause while authored work is
uncommitted (step 0.1 also stores its T1 `inventory/` there from the step's start):
- **Fixed commands.** At backup time, in the regeneration check (a) and on resume (rule 4),
  the backup's Git outputs are produced with exactly these commands, run from the repository
  root, with `P` = `--binary --full-index --no-renames --no-ext-diff --no-textconv --no-color
  --src-prefix=a/ --dst-prefix=b/ --no-relative`:
  `staged.patch` — `git --no-optional-locks diff --cached P HEAD`;
  `unstaged.patch` — `git --no-optional-locks diff P` (index → working tree, tracked paths; may
  be non-empty outside a review stop);
  path lists — `git --no-optional-locks diff --cached --name-status --no-renames -z HEAD`,
  `git --no-optional-locks diff --name-status --no-renames -z`,
  `git --no-optional-locks ls-files -o --exclude-standard -z`.
  `P` overrides only the options it names. It does **not** neutralize every Git configuration
  that can affect `git diff` output or `git apply` (e.g. `diff.suppressBlankEmpty`,
  `diff.context`, `apply.whitespace`; not tested). Check (a) cannot detect such an effect,
  because regeneration uses the same configuration. Check (c) detects any effect that makes a
  patch fail to apply or changes the content it reproduces: (c)2 compares the tree rebuilt from
  `staged.patch` with `T_staged`, which `git write-tree` computes from the real index
  independently of diff configuration; (c)4 compares each reconstructed tracked blob with
  `git hash-object` of its byte copy in `files/`, also independent of diff output; (c)3 compares
  the set of unstaged paths with `CHANGES.txt`. A detected effect fails (c) → not verified →
  rule 3. Not detected: an effect that changes patch bytes without changing the reproduced
  content (harmless for restoring), and effects on files outside (c) (`STATE.md`, ignored
  paths). Scratch-tested (Session 8): `color.ui`, `color.diff`, `diff.noprefix`,
  `diff.mnemonicPrefix`, `diff.relative`, running from a subdirectory. The statements about (c)
  follow from which values each comparison uses; they are not tested for every configuration.
- **Preconditions (at backup time and on resume; each → not verified if violated):**
  (G1) `git --no-optional-locks ls-files -v -z` shows only tag `H` — no `assume-unchanged` or
  `skip-worktree` entry (their changes appear in no path list and would be silently missing
  from `A`); (G2) `git --no-optional-locks ls-files -o -i --exclude-standard --directory -z`, each
  entry checked with `git check-ignore -v -z --no-index --stdin`, matches only the root
  `.gitignore` patterns `[Bb]in/`, `[Oo]bj/`, `*.user` — any other source (e.g.
  `core.excludesFile`, `.git/info/exclude`) or pattern is reported and stops (an untracked
  authored file ignored that way would be silently missing from `A`).
- `CHANGES.txt` — the authored change set `A`: the union of the three path lists, excluding
  untouched as-received untracked content (checked against the T1 manifest instead). One
  entry per path, listing **every** state that applies: staged (A/M/D/T), unstaged (M/D/T),
  untracked. A path may have several, e.g. staged A + unstaged M, or staged D + untracked (the
  file remains or reappears in the working tree after a staged deletion, e.g.
  `git rm --cached`). Each entry also records the SHA-256 of the working-tree file, or
  "absent", determined by testing the working-tree path directly, not from the lists.
  Consistency: an untracked state occurs only for paths absent from the index, and an
  unstaged state only for paths in the index. Any other combination → stop.
- `files/` — byte copies of exactly those paths in `A` whose working-tree value is not
  "absent", whatever their states;
- `history.bundle` — `git bundle create` of `HEAD` and the existing local branches (`main`,
  `develop`, the gate branch), so reconstruction does not depend on the real repository;
- `MANIFEST.sha256` and `STATE.md` (branch, HEAD commit id, staged tree hash `T_staged`, commit
  message verbatim, approval status, `git status --short --ignored`, restore instructions).
**Result and `CURRENT.txt`.** After the checks below, `RESULT.txt` is written in the folder:
`verified`, `patch-checked`, or `not verified: <check>` with the failed check, its output and the
time. `verify/` and `RESULT.txt` are created after `MANIFEST.sha256` and are not listed in it.
`<backup-root>/CURRENT.txt` names only the latest **verified** folder: it is written only after
(a), (b) and (c) have all passed for that folder, as the last write of the procedure, and never
for a patch-checked or failed folder; until the first verified backup it does not exist.
**A failed or patch-checked folder** is kept exactly as written — never deleted, overwritten,
reused, completed or "repaired" without explicit approval; a retry writes a new timestamped
folder, and `CURRENT.txt` keeps naming the previous verified folder, if any. The failed folder's
path and result are reported to the user and recorded per rule 2.
Renames: with `--no-renames` every rename is recorded as delete + add (no rename-detection
heuristic is relied on); an unstaged rename appears as a deletion in `unstaged.patch` plus an
untracked new path copied in `files/`. A staged deletion whose file is still or again present in
the working tree is recorded as staged D plus its working-tree hash, and is copied in `files/`.
**Checks — three levels:**
- (a) **Integrity:** every backup file except `MANIFEST.sha256` itself, `RESULT.txt` and anything
  under `verify/` is listed in `MANIFEST.sha256` and matches it. The set
  of paths in `files/` equals exactly the paths of `A` not recorded "absent", and each copy has
  the SHA-256 recorded in `CHANGES.txt`, equal to its working-tree file. Regenerating both
  patches and `CHANGES.txt` with the fixed commands gives byte-identical results.
  `git bundle verify history.bundle` passes and the bundle contains the recorded HEAD.
- (b) **Patch-application check:** in the verification repository (below),
  `git apply --cached --check staged.patch` against the recorded HEAD succeeds. This proves only
  that the patch applies cleanly — not that it reproduces the reviewed content. A backup that has
  passed only (a) and (b) is **patch-checked, not verified**.
- (c) **Full reconstruction**, entirely inside `<folder>/verify/`:
  1. `git clone --bare history.bundle verify/repo.git` — an independent repository; every
     following Git command uses `--git-dir=verify/repo.git` and a temporary `GIT_INDEX_FILE`
     inside `verify/`.
  2. **Staged state:** index `verify/index.staged` ← `git read-tree <recorded HEAD>`;
     `git apply --cached staged.patch`; `git write-tree` must equal the recorded `T_staged`. This
     covers added, modified, deleted, type-changed and (as delete + add) renamed paths.
  3. **Working-tree state:** index `verify/index.work` ← `git read-tree <T_staged>`; if
     `unstaged.patch` is non-empty, `git apply --cached unstaged.patch`. The paths where
     `index.work` differs from `T_staged` (`git diff-index --cached --no-renames --name-status
     T_staged`) must equal exactly the unstaged entries of `CHANGES.txt`.
  4. **Per path in `A`, by final state** (index = `index.work`; working tree = the recorded
     value):
     - in `index.work`, present in the working tree (staged A/M/T and/or unstaged M/T): copy in
       `files/`; its blob id in `index.work` = `git hash-object --path=<path> --
       <folder>/files/<path>` run against the real repository without `-w` (applies that path's
       attributes as `git add` would); copy SHA-256 = recorded value;
     - in `T_staged`, not in `index.work`, absent from the working tree (unstaged D, including
       staged A + unstaged D): recorded "absent", no copy;
     - not in `index.work`, present in the working tree (untracked, including a staged deletion
       whose file remains or reappears, ignored or not): copy in `files/`, SHA-256 = recorded
       value. The copy is the only source, so byte equality is the proof;
     - not in `T_staged`, not in `index.work`, absent from the working tree (staged D only):
       recorded "absent", no copy.

     A path that fits none of these cases, or whose `CHANGES.txt` states contradict its case →
     not verified.
The backup is **verified** only when (a), (b) and (c) all pass: the reconstructed staged tree
equals `T_staged`, and every remaining authored working-tree change is reconstructed and verified.
**Isolation.** The backup and verification procedure uses the real repository only through
`git --no-optional-locks` with `diff`, `ls-files`, `status` and `check-attr`, `git hash-object`
without `-w`, and `git bundle create` (which writes only the bundle file in the backup
folder). The reconstruction verification repository, including its temporary indexes and
objects, lives in the backup folder. The backup contains the history bundle and the other backup
artifacts. The separate step-workflow command `git write-tree` may write tree objects to the real
repository's object store, as described below. The procedure itself never writes the real index
entries, HEAD, refs or config. `--no-optional-locks` keeps `git status`/`git diff` from
opportunistically rewriting the real index with refreshed stat data. It is not a guarantee that
nothing writes the real repository: (1) `git write-tree`, used for `T_staged` in Step workflow
3–5, B6 and resume rule 4, writes tree objects into the real object store and may rewrite the
real index file (cache-tree data; entries unchanged). It belongs to the step workflow, not to the
backup procedure; (2) other processes (IDE Git integration, editors, build tools) can write the
index, working tree or object store at the same time; this decision does not control them.
Concurrent content changes are detected only if they fall between generation and regeneration
(check (a)) or show up at resume (rule 4). The proof holds only for the moment of the backup.
**Limitations — each makes the backup not verified (rule 3 applies):** an authored change under an
ignored path (`bin/`, `obj/`, `*.user`: ignored files are not backed up); a symlink, submodule or
gitlink in `A` (none exists; `core.symlinks=false`); an unstaged mode-only change
(`core.filemode=false` hides it); a path that cannot be copied or represented byte-exactly; any
check that cannot be run. The proof holds for the moment of the backup; later changes need a new
backup. (Session 7: `git bundle`, `git apply` and `git clone` were not exercised — level (c) is a
specification, not evidence, until first run at step 0.1.)
While work is uncommitted:
1. nothing that could lose, hide or replace it: no commit before approval, no stash, branch
   switch, reset, clean, restore, path checkout, `git gc`/`git prune`;
2. the state is recorded in the backup's `STATE.md` and, while `SESSION_HANDOFF.md` is untracked
   and not staged, also in its "Uncommitted review state" section; from step 0.2 on,
   SESSION_HANDOFF carries a permanent pointer to `<backup-root>/CURRENT.txt` instead;
3. **if a verified backup cannot be made** (no authorized location, not writable, any of (a)–(c)
   fails, only patch-checked, a limitation applies), **stop and ask before ending the session or
   taking any action that could lose the work**;
4. on resume, before anything else: verify branch, HEAD, `git write-tree`, `CHANGES.txt`,
   manifest and patches, regenerated with the fixed commands, against the backup named in
   `<backup-root>/CURRENT.txt` (none exists → stop and report); any mismatch → stop and report; never "repair"
   by resetting, restoring or re-staging without approval.

**Commits and merges.**
- **Step commit:** each approved step gets **exactly one step commit**, created on its gate branch
  `feature/phase-0-gate-N` only after the user has approved the exact staged tree and commit
  message (Step workflow). No step commit is made on `develop` or `main`; a step is never split
  across several commits or combined with another step. The only other commits allowed on a gate
  branch are correction commits under the exception rule below.
- **Gate merge commit:** a gate is integrated by **one `--no-ff` merge commit** of the gate
  branch into `develop`, made only after the gate review is approved and the merge itself is
  explicitly approved. It is a separate **integration commit — neither a step commit nor a
  correction commit**: it records the merge only and introduces no change of its own — no
  records, unrelated changes, edits or files beyond the merged commits. If the merge cannot
  complete without manual conflict resolution, stop and report before resolving anything.
- `develop` → `main`: one `--no-ff` merge commit after a release review and explicit approval
  (Phase 0: after Gate 3); same integration-only rule.
- **History is preserved:** no squash, rebase, amend, reset or any other rewrite of the history of
  `main`, `develop` or an open gate branch; no force push. Deleting a branch needs explicit
  approval.

**Correction after a step commit (exception rule).** A committed step is never amended, reset,
rebased or squashed, and a correction is never folded into a step commit. A correction requested
after a step commit is made only as a **correction commit**, which never bypasses the Step
workflow and needs **two separate approvals**:
- **(A) Correction-request approval** — of the scope: the corrected step and its commit hash,
  what will change, which paths may change, and where the correction is made (including, if
  needed, creating the next gate branch). (A) authorizes starting the work only; it approves
  neither a diff nor a commit. Recorded in DECISION_LOG.
- **(B) Final-diff approval** — after Step workflow 1–3 are run for the correction: explicit
  approval of the exact staged tree hash and commit message, as in Step workflow 4. Only (B)
  allows the commit; (A) never substitutes for (B). If the final diff goes beyond the scope
  approved in (A), stop and obtain a revised (A) first.
Further rules: only the approved correction to that one step — no step content, nothing
unrelated, records only about itself (prospective); message
`Correction to step <N> (<short hash>): <summary>`; one correction commit per approved request;
Step workflow 5–6 and the T4 rules apply; it is **not a step commit and never counts as one** —
the step keeps exactly one step commit, and the checklist and gate report list correction commits
separately under the corrected step. Undoing a step is done the same way (`git revert` = a
correction commit), never by rewriting history.
**Location (T2).** Before the corrected step's gate merge: that gate branch. After it: the next
open gate branch (created from the updated `develop` by the normal gate start, or as named in
(A)). After Gate 3, when no next gate exists: `feature/phase-0-correction-<k>` from `develop`, with
its own review and merge approvals, merged before the release review. Never on `develop` or
`main`.
**Isolation and integration.** (1) A correction starts only when no step or other correction diff
is uncommitted — `git status --short --ignored` shows nothing beyond known untracked/ignored
files. (2) Its staged paths are limited to the scope approved in (A) and shown as
`git diff --cached --name-only`. (3) It is committed as its own commit between step commits.
(4) If it touches files that the next step of that gate will also change, stop and ask whether it
is made before that step. (5) The gate review shows a separate section "Corrections to earlier
gates" (commit, corrected step and original gate, references to (A) and (B)), and the merge
approval request lists every commit to be integrated — `git log --no-merges develop..<gate branch>`
— classified as step or correction commits; the merge integrates exactly that list.

**A branch authorizes nothing.** Creating a gate branch does not approve the steps of that gate;
every step keeps its own approval requirement under D-06 and the phase plan ("Requires" column).
Approving a step or correction diff that contains a migration does not approve applying it to any
database. Schema changes and migrations keep their separate approvals (D-40, D-29, D-30).

**Working-tree safety (revised Session 6).** An ordinary branch switch does not need the user's
approval, but before every switch (`git switch` / `git checkout <branch>`):
1. Inspect `git status --short --ignored` and compare the current and target branch (e.g.
   `git diff --stat <current> <target>`) to identify uncommitted changes, untracked files, ignored
   files and files tracked on only one of the two branches.
2. Never discard or overwrite existing user data: no `-f`/`--force`, `--discard-changes`,
   `git clean`, `git reset --hard`, `git checkout -- <path>` / `git restore` on user files, and
   no `git stash` to make a switch possible.
3. If the switch could lose, hide, overwrite or leave behind files, or creates ambiguity about the
   safety of existing files (e.g. uncommitted changes — including a step or correction diff
   awaiting approval — an untracked file the target branch tracks, Git refusing the switch),
   **stop and report the exact files and risk before proceeding.**
Known Git behavior: **tracked files added by a commit on one branch disappear from the working
tree when a branch without that commit is checked out**; they remain in that branch's history and
return when it is checked out again. Example: after the step 0.1 baseline, checking out `develop`
or `main` before the Gate 1 merge removes the tracked `DefaultTemplate/` files and
`knowledge Docs/` (after step 0.2, also the committed docs). **Ignored files such as `bin/`,
`obj/` and `*.csproj.user` are not restored by Git checkout**: Git neither removes nor restores
them, so they stay behind unchanged (possibly stale for the other branch) and cannot be recovered
from Git if deleted. Ignore and attribute rules follow the checked-out branch's `.gitignore` and
`.gitattributes` — until the Gate 1 merge, `*.user` and `* -text` exist only on
`feature/phase-0-gate-1`. Untracked files (e.g. session docs before step 0.2) stay in the working
tree across switches.

**Databases are not isolated by branches.** All branches share the dev DB `CseLibrary`;
`Program.cs` runs `MigrateAsync()` on startup (D-40 hazard), so running the app on a branch with
a new migration applies it to the shared dev DB. Rule: never run the app against `CseLibrary` on
a branch carrying an unapproved migration. Tests stay on per-run `CseLibrary_Test_{guid}` (D-40).
D-40, D-29 and D-30 are unchanged.

**`main` protection** (no direct pushes) is a GitHub setting for the repository owner, applied
when pushes are approved and where the plan/visibility allows it; not done by the assistant.
Remote (read-only check, M14; re-checked Session 7): `origin` = `github.com/Rokibul-Hassan-Remon/Department-Library-Management-System`;
local `main` = `origin/main` = `af89144`.

**Phase 0 mapping (Option A).** Step 0.1 becomes, all local, no push:
1. record `I_before` and `L_disk` (T1 generation 1–2) in the step's backup folder; then, only if
   (R2) was approved, make and verify the received-files copy (T4 "Received files") before
   anything else is created or staged;
2. `develop` created from the current `main` baseline (`af89144`) — no commit;
3. `feature/phase-0-gate-1` created from `develop` — no commit;
4. on `feature/phase-0-gate-1`: write `.gitattributes` (T1) and add `*.user` to `.gitignore`;
   record `L_excluded`; re-record `L_disk`; stage with the single T1 command; run the B-checks;
   show per Step workflow 3; make the verified T4 backup; commit only after the user approves the
   exact staged tree and commit message — the only step commit of step 0.1; then the C-checks
   (Step workflow 6).
`main` and `develop` stay at `af89144` until their approved merges. Steps 0.2–0.5 add one step
commit each on the same branch, each after its own approval to start and approval of its exact
staged tree and message; step 0.2 also carries step 0.1's records (T3). (Rejected alternative,
Option B: run 0.1 on `phase-0/baseline` per D-06 and adopt D-07 from Phase 1.)

**Supersession:** D-06 in part — see the exact wording in DECISION_LOG §21. Until D-07 is approved
it is **not** applied; on approval it replaces D-06's "M14 (pending)" paragraph and D-06's heading
becomes "— Approved (partly superseded by D-07, <date>)".

**Documentation changes on approval:** complete list in DECISION_LOG §21 ("Documentation impact
list", replacing the §20 list) — PROJECT_DECISIONS (header, D-06, D-07, §9); PHASE_0_PLAN;
SESSION_HANDOFF; CLAUDE.md; DECISION_LOG.
**Verify:** at each gate, `git log --graph --all --oneline`, `git status`, branch list.

---

## 2. Architecture and data access

### D-10 Layer boundaries — Approved
- **Web:** controllers, views, ViewModels, request/response, authorization, UI composition,
  ViewModel↔DTO mapping. Controllers must not access DbContext, `IUnitOfWork` or repositories.
- **Services:** business rules and use-case orchestration only. No EF Core queries, no
  `HttpContext`, no MVC types.
- **Data:** all queries and persistence via repositories; EF Core/SQL Server details stay here.
- **Core:** entities, enums, DTOs/read models, interfaces; no EF/MVC dependencies.
Dependency direction: Web → Services → Core; Data → Core; Web → Data only as composition root.
Request path: Controller → Service → `IUnitOfWork` → repository interfaces → repository
implementations → `ApplicationDbContext` → SQL Server.
**Source:** M2 Mandatory Standards §1, M3 item 1, M5 §2.
**Enforcement — Approved (M5 §2, M6 A3):** the Services project references no Data project,
no EF Core package and no `Microsoft.AspNetCore.App`; it uses `Microsoft.Extensions.Identity.Core`
for `UserManager`/`RoleManager` (`SignInManager` stays in Web). Core references no EF Core and no
ASP.NET Core. Plus architecture tests.
**Verify:** architecture tests E1, E3 (PHASE_0_PLAN).

### D-11 Direct `ApplicationDbContext` in services ("Option A") — Rejected
Assistant proposed it (M2 response). **Rejected by user in M3 item 1.** Repository + UoW is mandatory.

### D-12 Repository + Unit of Work — Approved
- Services inject **`IUnitOfWork`** and reach repositories through its **typed repository
  properties** (e.g. `uow.Books`, `uow.GeneralConfigs`).
- All repositories share **one scoped `ApplicationDbContext`** per request.
- **Repositories never call `SaveChanges`.** The UoW is the only owner of SaveChanges and
  transactions.
- **No Service Locator:** no generic `Repository<T>()` / `GetRepository(Type)` accessor; only
  explicit typed properties.
- Services never inject repository interfaces directly; controllers never touch repositories,
  UoW or DbContext.
**Source:** M3 item 1, M4, M5 §1 (rules 1–10), M6 A1.
**Trade-offs:** `IUnitOfWork` gains one property per repository; every service can see all
repositories (wider than strictly needed) — accepted for consistent, simple service constructors.
**Supersedes:** the assistant's M3-response proposal to inject repositories directly into
services with UoW limited to save/transaction (**Superseded**).
**Repository creation — Approved conditionally (M6 A2):** the UoW creates repositories lazily
over the shared context (`_books ??= new BookRepository(_db)`); repositories are not registered
in DI.
- **Constraint:** valid only while a repository needs nothing but the shared `ApplicationDbContext`.
  If a repository ever needs another dependency, stop and propose a DI-compatible factory approach
  (e.g. repositories registered in DI and injected into the UoW constructor) as a new decision.
- **Not a compile-time guarantee** (M6 correction): omitting DI registration only makes direct
  injection fail at runtime. Architecture test **E3** explicitly forbids any Service constructor
  parameter of a repository type.

### D-13 Repository contract — Approved (M5 §1 rules 6–7, M6 A8); exact signatures reviewed at Gate 2
- Generic write-side base `IRepository<T>`: `GetForUpdateAsync(id, ct)` (tracked), `AddAsync`,
  `Remove`.
- Purpose-specific read methods per feature repository (e.g. `GetBookListAsync(PageRequest)`,
  `GetBookDetailsAsync(id)`, `KeyExistsAsync(key, excludeId)`), returning projected DTOs.
- Remove from the template's `IRepository<T>`: `Query()` (leaks `IQueryable`/EF into services),
  `GetAllAsync`/`FindAsync` (full-table materialization), `Update()` (marks every property
  modified → false audit diffs). Remove `IDisposable` from UoW (it disposed the DI-owned DbContext).
- `uow.Users`: **read-only** repository projecting Identity tables (Users list with roles, no
  N+1). User writes stay on `UserManager`.
Phase 0 UoW properties: `GeneralConfigs`, `Permissions`, `RolePermissions` (or a Roles read
repository), `Users`. Phase 1+ adds `Books`, `BookCopies`, `Members`, `LendingTransactions`.
**Session 17 — exact signatures Approved by the user (DECISION_LOG §45); implemented `0d01e4e`
(§46):** `IRepository<T> where T : BaseEntity` (`GetForUpdateAsync`, `AddAsync`, `Remove` = soft
delete); `IGeneralConfigRepository`, `IPermissionRepository` (incl. `GetNamesForUserAsync`, one
join), read-only `IRoleReadRepository` + write-side `IRolePermissionRepository` (composite key),
read-only `IUserReadRepository` (`GetPageAsync`); UoW properties `GeneralConfigs`, `Permissions`,
`Roles`, `RolePermissions`, `Users`; one `ExecuteInTransactionAsync<TResult> where TResult : Result`.
Deviation: `IRoleReadRepository.GetPermissionNamesAsync` dropped (no caller).

### D-14 UoW transactions — Approved in principle (M5 §5, M6 A5); correctness must be proven by tests
Approved (M3 item 1, M5 §1 rule 9): UoW coordinates persistence and transactions; no duplicate or
conflicting SaveChanges/transaction responsibilities; transactions only where a use case needs
atomic changes across multiple saves — ordinary single-save use cases stay simple.
Design:
- `SaveChangesAsync(ct)` — one call per ordinary use case; a single SaveChanges is atomic and
  retried by EF's execution strategy.
- `ExecuteInTransactionAsync(Func<CancellationToken, Task<Result>> work, ct)` — only for multiple
  SaveChanges, Identity (`UserManager`/`RoleManager`, which save internally) mixed with our
  writes, or a pessimistic lock read + write.
- Runs inside `Database.CreateExecutionStrategy().ExecuteAsync(...)` because
  `EnableRetryOnFailure` is on. Each attempt: `ChangeTracker.Clear()` → `BeginTransactionAsync`
  → `work` → commit; failed `Result` → rollback + `ChangeTracker.Clear()` → return the failure;
  exception → `ChangeTracker.Clear()`, transaction disposal rolls back, rethrow.
- **Nested calls are rejected** (`InvalidOperationException` if a transaction is already active).
- Translate `DbUpdateConcurrencyException` → `ConcurrencyConflictException` and SQL errors
  2601/2627 → `DuplicateKeyException` (both defined in Core) so services stay EF-free. These are
  not transient and are not retried.
Retry-correctness rules (`ChangeTracker.Clear()` alone is **not** sufficient — M5 §5):
1. The delegate must be re-runnable: it loads all state it needs **inside** the delegate via
   repositories/managers; it never reuses entities (incl. `ApplicationUser`) loaded before the call.
2. No external side effects inside the delegate (email, cache, static state); emails go through
   an outbox row in the same transaction (D-36).
3. Ambiguous commits (connection lost during commit): duplicate business operations must be
   prevented per use case by database constraints (unique / filtered unique indexes) or an
   idempotency check; where needed, EF's `verifySucceeded` overload. **Approved in principle
   (M8 §3)** with these conditions:
   - For each atomic use case, document its duplicate-operation risk and select a database
     constraint, idempotency mechanism or other **verified** protection. A unique constraint is
     **not** assumed to protect every operation (e.g. it cannot stop a repeated update or a
     repeated side effect).
   - Identify which use cases need protection, explain the chosen mechanism, add feasible tests.
   - Report remaining limitations; never claim every ambiguous-commit scenario is fully proven.
   - The detailed use-case assessment is presented at Gate 2.
   - **Session 18 (commit `8cd5f53`):** `ExecuteInTransactionAsync` gained an optional
     `verifySucceeded` callback (used by role/user create). Phase 0 assessment: DECISION_LOG §48.
4. Identity stores (`AddEntityFrameworkStores<ApplicationDbContext>`) resolve the same scoped
   context, so their internal SaveChanges join the transaction — **to be proven by test E2**, not
   assumed. Failed `IdentityResult` is converted to a failed `Result` (→ rollback).
5. Background jobs create one DI scope per job.
**Verify:** tests E2, F1–F4, G1. Template bug fixed by this: `RoleService.UpdateAsync` /
`UserService.UpdateAsync` are not atomic today.

### D-15 Data fetching and query efficiency — Approved
Fetch only what the use case needs: `Select()` projections to DTOs/ViewModels for reads;
`Include` only when related data is genuinely needed; filtering/sorting/counting/pagination in
the database; server-side paging for large lists (Books, Members, Lending, Audit, Users); tracked
loads only for updates; no N+1; no full-table materialization; correctness/security fields are
always fetched. **Source:** M3 item 8, M5 §7.
Testing rule (M5 §7): important screens get tests for pagination, projection and query behavior.
For the Users list the target is a **bounded** count-and-page pattern whose SQL command count does
not grow with the number of users (no N+1 role queries); correctness is never sacrificed to hit a
specific command count.
**Session 17 (DECISION_LOG §45): page size default 20 / max 100 (clamped, never rejected) and the
unpaged bounded admin tables are Approved by the user; implemented `48a9acf`/`0d01e4e`.**
Proposed details: `PageRequest`/`PagedResult<T>` in Core, page size clamped to max 100; bounded
admin tables (`Permissions`, `GeneralConfigs`, < ~100 rows) may skip paging (documented
exception); `AsNoTracking()` only for entity reads (projections are untracked anyway); EF SQL
logging at Information in Development.
**Verify:** tests D1–D3 (SQL command counting).
**M9 extension (FR-M9-03) — Proposed.** Requirement in `requirements.md`; this is the design.
- Two paging modes, chosen per list and documented:
  - **Keyset (cursor)** — default for large, growing lists: Books, Members (Students, Fellows),
    Lending, Audit, portal history. `ORDER BY <sortKey>, Id` with `Id` as unique tie-breaker;
    `WHERE (sortKey, Id) > (@lastKey, @lastId)` expressed as
    `sortKey > @k OR (sortKey = @k AND Id > @id)` (reversed for descending); fetch `pageSize + 1`
    to detect the end without a count.
  - **Offset count-and-page** — kept where page numbers / totals are needed and the data is
    bounded: the **Users list** (already Approved above, test D1 — not converted) and other small
    admin lists. Bounded admin tables may skip paging (exception above).
- Cursor: opaque, server-built token carrying the last row's sort-key value(s), `Id`, sort
  direction and a hash of the active filter/sort; a cursor whose hash does not match the request is
  rejected (first page / validation error, never wrong data). The cursor carries **position
  only**; authorization and ownership filters (e.g. member sees own history only, BR-14) are always
  re-applied server-side.
- Projection to listing DTOs only; no `Include` for lists; counts only where the UI needs them
  (a total count costs a second query).
- Page size: default 20, maximum 100 (consistent with the max above), from configuration;
  values Proposed.
- Indexes: per list, matching filter + sort columns + `Id` (e.g. Books `(Title, Id)`), decided when
  the list is built from its actual filters; not created speculatively.
- **Consistency model (documented limitation):** each page is a fresh query (no snapshot).
  Rows inserted before the cursor position are not shown on later pages; deleted rows are simply
  skipped; no row repeats **unless its sort-key value changes between requests** (it may then be
  skipped or shown twice — mitigated by preferring immutable sort keys and a client-side
  `data-id` duplicate guard, D-46).
- **Keyset limitations:** no jump to an arbitrary page number; no cheap "page X of Y"; "previous
  page" needs a reverse query or a client-kept cursor stack; free-text `LIKE '%term%'` filters
  cannot use the index (full-text search is a separate decision if volume requires it).
- Phase 0 impact: the step 0.7 `PageRequest`/`PagedResult<T>` contract must not preclude a later
  keyset contract (e.g. separate `CursorPageRequest`/`CursorPage<T>` in Phase 1). No keyset
  implementation in Phase 0 (no large library list exists yet).
**Verify:** J1–J8 (`requirements.md`), D1/D2 for the Users list.

### D-16 Known template query/persistence defects — Approved fixes (step 0.8; Session 13)
Q1 `UserService.GetAllAsync`: sync full-table load incl. `PasswordHash`, then N+1
`GetRolesAsync` → paged single projection. Q2 `RoleService` `Include(RolePermissions)` →
projection of ids. Q3 `GetPermissionsForUserAsync` 3 queries → 1 join. Q4 permission claims
written to `AspNetUserClaims` on every login (see D-37). Q5/Q6 see D-13. Q7 edit pages load
tracked entities for display → projections. Q8 non-atomic role/user updates → D-14.
Q9 `UnitOfWork.Dispose()` disposes the DI-owned DbContext.

### D-17 Remove AutoMapper — Approved
Remove AutoMapper (12.0.1 has advisory NU1903 / GHSA-rvv3-g6hj-g44x; newer versions are
commercially licensed). Use explicit manual mapping (repositories project to DTOs; Web maps
ViewModel↔DTO with small static extension methods when non-trivial). No replacement mapping
library without explicit approval. **Source:** M3 item 5.
**Implemented + verified Session 17:** `51e189a` (DECISION_LOG §44); the interim `ToDto()` helpers
were replaced by repository projections in `0d01e4e`.

---

## 3. Audit, logging and history

### D-20 Separate concerns — Approved
Application/error logs (Serilog, structured, correlation ID, redaction), **audit logs**
(who/when/what changed), and **business history** (e.g. `LendingTransactions`) are distinct.
Application logs never replace business records. **Source:** M2 Standards §4.

### D-21 Central `AuditLogs` via EF Core `SaveChangesInterceptor` — Approved; schema and change-data policy **Approved Session 22** (see "Audit package" below D-27)
One indexed central table, written by a `SaveChangesInterceptor` in the **same SaveChanges**
as the business change (atomic). Audit Books, BookCopies, Members, LendingTransactions and other
relevant application entities. **Source:** M3 item 2, M2 Standards §5.
Proposed schema: `Id` bigint, `Category` (1 Entity, 2 Security), `EntityType`, `EntityId`,
`Action`, `ActorUserId` (null = system), `OccurredAtUtc`, `CorrelationId` (request TraceId),
`Changes` (JSON). Indexes: (EntityType, EntityId, OccurredAtUtc), (ActorUserId, OccurredAtUtc),
(CorrelationId). Actor via `ICurrentUser` (Core; Web impl from HttpContext, worker impl =
system) and time via `TimeProvider` — both Proposed.
Change-data policy (Proposed): Created = all `.Audited()` values; Updated = only changed
`.Audited()` properties old→new; Deleted = key + audited snapshot; exclusions per D-23;
truncate strings to 1,000 chars.

### D-22 Allowlist; Identity tables excluded — Approved (principle and mechanism)
Only allowlisted application entities are audited automatically. **No automatic audit of
Identity tables** (AspNetUsers, AspNetRoles, AspNetUserRoles, AspNetUserClaims, etc.).
**Source:** M3 item 3, M5 §4. Mechanism (Approved M5 §4): marker interface `IAuditable` in Core —
Identity entities never implement it (verified by test C7).
Phase 0 candidates: `GeneralConfig`, `Permission`. `RolePermission` changes are recorded as an
explicit security event instead.

### D-23 Audit field safety policy — Approved (M5 §4, M6 A6)
Never record passwords, password hashes, security stamps, auth/reset tokens, credentials or other
secrets. **Explicit opt-in** (default = not audited):
- Every property of every `IAuditable` entity is classified in its Fluent configuration as
  `.Audited()` or `.NotAudited()` (model annotation; Core stays free of audit attributes).
  A missing classification **fails a test** (C6), so new properties force a decision.
- Always excluded regardless of classification: concurrency tokens, binary data, shadow
  properties.
- Secondary safety net only: a property named like Password/Hash/Token/Secret/Stamp/Credential
  classified `.Audited()` fails a test (C6).
Supersedes the earlier `[AuditIgnore]` name-check proposal. Member email/phone changes are
audited (accountability); audit access restricted to Admin.

### D-24 Explicit security audit events — Approved (principle); list and policy **Approved Session 22** (see "Audit package" below D-27)
Interceptor alone does not capture security events (M3 item 3). Proposed `ISecurityAuditService`
writing `Category = Security` rows:
- `LoginSucceeded`, `LoginFailed`, `LockedOut`, `Logout` — written independently, best-effort
  (failure to write is logged to Serilog and does not break login). Unknown-email attempts go to
  Serilog only (users sometimes type passwords into the username field).
- `UserCreated`, `UserUpdated` (changed field names), `UserActivated/Deactivated`,
  `UserRolesChanged`, `PasswordChanged`, `PasswordResetByAdmin`, `RoleCreated/Updated/Deleted`,
  `RolePermissionsChanged` — written in the **same UoW transaction** as the change.

### D-25 Interceptor limitations — Approved as documented constraint; no-bulk/raw-write rule **Approved Session 22**
The interceptor only sees change-tracked `SaveChanges`. It does **not** see `ExecuteUpdate`/
`ExecuteDelete`, raw SQL writes, direct DB edits, or migration data seeding. Rule (Proposed):
no bulk/raw writes on `IAuditable` entities; if ever required, the service writes explicit audit
rows in the same transaction, recorded as a new decision.

### D-26 Entity-specific history tables — Approved (policy)
Start with the central table. Add an entity-specific history table only with a concrete
requirement (own schema, query pattern, retention, or volume — e.g. copy-condition timeline
report). Such history is written explicitly by the service in the **same SaveChanges**.
`LendingTransactions` remain the authoritative business history. **Source:** M3 item 4.

### D-27 Consistency policy — **Approved Session 22** (unchanged table)
| Record | Same transaction as the business change? |
|---|---|
| Entity audit (interceptor) | Yes — same SaveChanges |
| Security event for an admin action | Yes — UoW transaction |
| Login security events | No — independent, best-effort |
| Future entity history / email outbox | Yes |
| Serilog application logs | No — best-effort, outside |

**Audit package — Approved Session 22 (2026-10-15, DECISION_LOG §72)**, details for D-21 – D-27
(implementation plan needs its own approval):
- **P1 `AuditLogs` (D-21):** `Id` bigint IDENTITY (nothing references it in the same save, so no
  HiLo); `Category` tinyint (1 Entity, 2 Security); `EntityType` nvarchar(100); `EntityId`
  **nvarchar(64)** (int entity ids and string user ids, Q4); `Action` nvarchar(50);
  `ActorUserId` nvarchar(450) NULL (NULL = system); `OccurredAtUtc` datetime2 (`TimeProvider`);
  `CorrelationId` nvarchar(64) NULL (request TraceId = the D-34 Reference ID); `Changes`
  nvarchar(max) JSON. Indexes (EntityType, EntityId, OccurredAtUtc), (ActorUserId,
  OccurredAtUtc), (CorrelationId). Append-only: no soft delete, not a `BaseEntity`. New
  migration after the D-30 baseline. `ICurrentUser` and `TimeProvider` already exist (D-50/D-51).
- **P2 change data (D-21):** Created = every `.Audited()` value; Updated = only changed
  `.Audited()` properties old→new; Deleted = the soft delete (Status → Deleted), key + audited
  snapshot; strings truncated to 1,000 characters; JSON `{"Prop":{"Old":…,"New":…}}`.
- **P3 scope (D-22, D-23):** Phase 0 `IAuditable` = `GeneralConfig`, `Permission`. Audited:
  `Key`, `Value`, `Description`, `Group`, `Name`, `DisplayName`, `Module`, `Status`. NotAudited:
  `Id` (it is `EntityId`), `CreatedAt/By`, `UpdatedAt/By` (duplicate the audit row). **Q1:**
  `GeneralConfig.Value` is audited; **rule: secrets are never stored in `GeneralConfig`** —
  they belong in user-secrets / environment variables (D-40).
- **P4 security events (D-24), Q2 = full list:** same transaction as the change —
  `UserCreated`, `UserUpdated` (changed field names only), `UserActivated`/`UserDeactivated`,
  **`UserDeleted`** (D-53, new), `UserRolesChanged` (added/removed names), `PasswordResetByAdmin`,
  `PasswordChanged` (self-service), `RoleCreated`/`RoleUpdated`/`RoleDeleted`,
  `RolePermissionsChanged` (added/removed). Independent, best-effort (failure → Serilog only,
  login unaffected): `LoginSucceeded`, `LoginFailed` (known user), `LockedOut`, `Logout`;
  unknown-email attempts → Serilog only. Never passwords, hashes or tokens.
- **P5 (D-25):** no `ExecuteUpdate`/`ExecuteDelete`/raw SQL writes on `IAuditable` tables;
  enforced by an architecture test (the `UnitOfWork` `sp_getapplock` call writes nothing).
- **P6 (D-27):** the table above, unchanged.
- **P7 not in Phase 0:** audit viewing UI (later, Admin-only); retention/purge (D-28 Pending).

### D-28 Audit retention/archival — Pending (later phase)
No purge until a retention policy, backup/restore and archive-integrity procedure exist. Expected
volume small (SQL Express 10 GB DB limit is ample). Later options: yearly archive table/DB keyed
by EntityType/EntityId; DB-level `DENY UPDATE, DELETE` on `AuditLogs` (Phase 7). No partitioning.

### D-29 HiLo identifiers for audited entities — **Approved for migration changes (Session 22, option (a) with D-30)**; was conditionally approved (M6 A4)
Problem: for Added entities with SQL identity keys, `EntityId` is unknown during
`SavingChanges`, so the audit row cannot carry it in the same SaveChanges.
Candidate: EF Core HiLo, one sequence per `IAuditable` table, block size 10, **only** on
`IAuditable` entities with DB-generated int keys (Phase 0: `GeneralConfig`, `Permission`;
later `Book`, `BookCopy`, `Member`, `LendingTransaction`, `Category`). Id is assigned at
`AddAsync`; one SaveChanges stays atomic. Identity tables (string keys, not audited) unaffected.
Trade-offs: gaps in ids; ids not strictly insertion-ordered across restarts; `AddAsync`
required (sync `Add` would block on the sequence fetch); SQL Server cannot remove IDENTITY from a
column, so existing identity columns need a table rebuild or a regenerated baseline (D-30).
Alternatives considered: (B) two SaveChanges in an interceptor/UoW transaction
(`acceptAllChangesOnSuccess:false` + retry complexity, most failure-prone); (C) client-generated
sequential GUID keys (larger keys/indexes, unreadable ids).
**Conditions before any migration change (M6 A4):**
1. Demonstrate in an **isolated new database** (never `DefaultTemplate`): HiLo id generation,
   audit rows carrying the new `EntityId`, rollback behavior, and compatibility with
   `EnableRetryOnFailure`. **Database (M8 §2):** a uniquely named, disposable LocalDB database
   `CseLibrary_Test_{guid}` under the D-40 guard — **not** the `CseLibrary` development database.
2. Show the migration and generated SQL script diff.
3. Obtain the user's explicit approval before applying any consequential database change.
**Session 22 (DECISION_LOG §69):** conditions 1 and 2 met — demonstration tests green
(`be033f9`, spike model on a disposable test DB); migration diff shown. The SQL script cannot be
generated for the existing tables ("To change the IDENTITY property of a column, the column
needs to be dropped and recreated"), so applying D-29 needs D-30 (regenerated baseline) or a
table rebuild. **Condition 3 met Session 22 (DECISION_LOG §71):** the user approved option (a) — HiLo (block 10,
`GeneralConfig` + `Permission` now; later audited entities as listed) with a regenerated baseline (D-30).

### D-30 Regenerate baseline migration — **Approved Session 22** (option (a); data confirmed by the user, read-only check DECISION_LOG §70)
Proposed together with D-29 and the rename (D-41): regenerate `InitialCreate` instead of
altering identity columns. Requires user confirmation that **no database created from the
template holds data that must be kept.** The existing `DefaultTemplate` DB is never touched.
M5 §3 / M6 C: before any reset, inspect (read-only) current connection strings, user-secrets,
migrations and existing databases; any reset is limited to a confirmed **new** project database,
is explained before execution, and needs separate explicit approval.

---

## 4. Security and error handling

### D-31 Security baseline — Approved; password + lockout values and access design Approved (Session 19); other default values Proposed
Approved (M3 item 6): secure password policy, configurable lockout, authenticated-by-default
authorization, explicit `[AllowAnonymous]` only where required, effective deactivation,
server-side authorization, safe error responses, security auditing. Values must be configurable
and documented, not hard-coded.
**Approved Session 19 (2026-10-14, DECISION_LOG §52) — password and lockout values:** password
min length **10**, at least **one digit**; lockout after **5** failed attempts for **15 minutes**;
`PasswordSignInAsync(..., lockoutOnFailure: true)`. Still **Proposed** (not covered by this
approval): security-stamp validation interval, cookie settings, global antiforgery.
**Approved Session 20 (2026-10-14, DECISION_LOG §55), after comparison with the current config:**
security-stamp validation interval **5 min** (`Security:StampValidationMinutes`; current default
30 min); cookie **`Secure = Always`**, `HttpOnly = true` and `SameSite = Lax` set explicitly,
8 h sliding unchanged; global **`AutoValidateAntiforgeryToken`** (existing per-action attributes
may stay). Code change (D-31 baseline commit) needs its own execution approval.
**Implemented Session 20 — committed `215d75e` (DECISION_LOG §55):** all approved D-31
values from `appsettings` (`Identity`, `Security:StampValidationMinutes`), cookie flags, global
antiforgery, `FallbackPolicy` + class `[Authorize]`; anonymous: `Home.Index`/`Privacy`/`Error`,
`Account.Login`/`AccessDenied` (AccessDenied until D-55). Status-code page comes with 0.10.
**Approved Session 19 (DECISION_LOG §52) — 0.9 access design** (supersedes the Session 13
Q-M9-10 answer "all pages authenticated except Login, AccessDenied, Error"):
- `Home.Index` is **public** and is the CSE Library landing page, with **Login** and
  **Registration** links in the top-right corner.
- `Home.Privacy` stays **public** (general information page).
- Login and Registration endpoints are reachable **anonymously**.
- All other pages and operations require **authentication and the appropriate
  authorization/permission checks**.
- `FallbackPolicy` + class-level `[Authorize]` applied consistently (approach items 1–3 below);
  explicit `[AllowAnonymous]` **only** on the intended public endpoints.
- Public Registration **must not grant privileged roles automatically**.
- Open (not decided by this approval): Q-S19-1 registration scope, Q-S19-2 anonymous access to
  `Error` / status-code / `AccessDenied` (section 9).
**Approved Session 20 (DECISION_LOG §55) — Q-S19-2:** generic error / status-code responses work
for anonymous requests; the authorization rules of protected resources stay unchanged.
Proposed defaults (bound from `appsettings` `Identity` section to `IdentityOptions`): password
min length 10 + digit; lockout 5 failures / 15 min with `lockoutOnFailure: true`; security-stamp
validation interval 5 min (`Security:StampValidationMinutes`); `FallbackPolicy` =
authenticated user; `[AllowAnonymous]` only on Login, AccessDenied, Error, StatusCode pages;
global `AutoValidateAntiforgeryToken`; cookie HttpOnly, `Secure=Always`, `SameSite=Lax`,
8 h sliding.
**M9 extension (FR-M9-05) — Approved Session 19 (access design above; §52).** Requirement in `requirements.md`.
Code inventory (read-only, Session 4, `DefaultTemplate/DefaultTemplate/`):
- **No controller has a class-level `[Authorize]`; no `FallbackPolicy` is configured**
  (`Program.cs` registers only `PermissionPolicyProvider` + `PermissionAuthorizationHandler`).
- `GeneralConfig`, `Permissions`, `Roles`, `Users` controllers: **every action** carries
  `[HasPermission(...)]` (derives from `AuthorizeAttribute`) — protected today, but only by
  per-action discipline.
- `HomeController`: `Index`, `Privacy`, `Error` have **no attribute → anonymous today**.
  `Error` must stay reachable anonymously (and later the status-code page, step 0.10).
- `AccountController`: `Login` (GET/POST), `AccessDenied` = `[AllowAnonymous]`;
  `ChangePassword` = `[Authorize]`; **`Logout` (POST) has no attribute → anonymous** (low risk —
  antiforgery-protected — but inconsistent).
Proposed approach (both layers; they do not conflict):
1. **Class-level `[Authorize]` on every controller** (explicit, as M9 requires), with
   `[AllowAnonymous]` only on the allowlist: `Account.Login` (GET/POST), `Account.AccessDenied`,
   `Home.Error`, status-code page (0.10). Whether `Home.Index`/`Home.Privacy` are public is
   Q-M9-10 (D-31 default above makes them authenticated). **Session 19: superseded —
   `Home.Index`, `Home.Privacy` and Registration are public (access design above); the
   Error/status-code/AccessDenied entries are Q-S19-2.**
2. **`FallbackPolicy` = authenticated user** stays as the safety net for any endpoint without
   authorization metadata (e.g. a future controller that forgets step 1). Fallback applies only
   where no `[Authorize]`/`[AllowAnonymous]` exists, so it is not redundant with step 1 and does
   not override it.
3. **Permission checks stay at action level** (`[HasPermission]`, D-37). Class `[Authorize]` +
   action `[HasPermission]` combine (both must pass); authentication alone never grants an action.
   Member-portal actions additionally need an **ownership** check in the service (BR-14).
4. Error/status pages show only the Reference ID (D-34; tests B1/B2).
**Verify:** A1, A2, A4, A6–A8, A10, A11 (step 0.9), B1/B2 (step 0.10).

### D-32 Effective deactivation — Approved (principle); mechanism Approved (Session 19)
Set `IsActive=false` + `UpdateSecurityStampAsync` (session invalid within the validation
interval); move the `IsActive` login check from `AccountController` into a
`SignInManager.CanSignInAsync` override. Permission-protected actions are denied immediately via
the per-request `IsActive` check (D-37). Verify: test A4.
**Approved Session 19 (2026-10-14, DECISION_LOG §52):** the proposed design above — an inactive
user cannot sign in again; deactivating a user calls `UpdateSecurityStampAsync`; existing-cookie
validation and protected-page access are verified by automated tests (A4). This approval does
**not** approve any other security setting (the stamp validation interval value stays D-31
Proposed). In code the flag is `ApplicationUser.Status` (`RecordStatus.Active`/`Inactive`).
**Session 20 (DECISION_LOG §56):** not implemented yet; the stamp interval it relies on is now
5 min (D-31, `215d75e`). Implementation plan proposed (section 9, "Session 21 start"); execution
approval pending.
**Session 21 (DECISION_LOG §57): implemented `c63854b`** — `ActiveUserSignInManager`
(`CanSignInAsync` = `Active` only, registered via `AddSignInManager`; check removed from
`AccountController`); `UserService.UpdateAsync` calls `UpdateSecurityStampAsync` on Active →
Inactive. Test A4 `Deactivated_user_loses_existing_sessions`; 65/65; mutation-checked.

### D-33 Permission claims via `UserClaimsPrincipalFactory` — Superseded by D-37
Original proposal: a custom `UserClaimsPrincipalFactory` adds permission claims to the cookie at
sign-in and on security-stamp refresh. M5 §6 approved moving away from per-login DB writes and
*evaluating* a factory; the evaluation (Session 2) found that permission claims in the cookie stay
stale until stamp refresh, so D-37 replaces it. Superseded per M6 A7 now that D-37 documents the
replacement design; **timing confirmed by the user (M8 §1)** on condition that the same change
documents the replacement design, its security rationale and its acceptance tests (all in D-37).
Kept as a historical record — never deleted. **Correction:** the earlier "claims may be lost on stamp refresh" risk was
wrong — the default factory re-reads `AspNetUserClaims`; the real defect is that revoked
permissions stay in the cookie (see D-37).

### D-37 Database-backed permission authorization — Approved in principle (M6 A7)
Finding (Session 2, code-verified): `AccountController.AddPermissionClaimsAsync` writes permission
claims to `AspNetUserClaims` at login; `PermissionAuthorizationHandler` succeeds if the cookie has
the claim and only otherwise queries the DB. Grants therefore apply immediately, but **revocations
do not** — a stale cookie claim keeps granting access. Authorization in the template is
permission-only (no role checks).
**Correction (Session 14, test-verified on .NET 10, DECISION_LOG §34):** `RefreshSignInAsync` is a
no-op inside the login request, and the cookie is built from `AspNetUserClaims` *before* login
rewrites them — so each cookie carries the permissions written by the **previous** login. A revoked
permission is denied on the current cookie (DB fallback), **comes back for exactly one later login**,
then is denied. Pinned by `Revoked_permission_comes_back_for_one_login_from_stale_claim_rows`.
The design below fixes both variants.
**Security rationale:** authorization must reflect the *current* database state. A permission
revocation, role removal or deactivation must take effect on the user's next request, not when
the cookie expires or the security stamp is refreshed; cookie-held permission claims cannot
guarantee that (the reason D-33 is superseded).
Design:
- No permission claims in the authentication cookie; login writes nothing to `AspNetUserClaims`.
- `PermissionAuthorizationHandler` checks on every applicable request, through the service layer,
  **user `IsActive` + current role membership + current role permissions** in one indexed query.
- Result cached **request-locally only** (`HttpContext.Items`); no cross-request cache.
- Security stamp updated on deactivation and password change/reset; `ValidationInterval`
  configurable (D-31).
- Removing existing permission rows from `AspNetUserClaims` is a data change that needs
  **separate explicit approval** (M6 C).
Cost: one small query per authorized request — acceptable for this application's traffic.
**Done only when tests pass:** A4, A7, A8, A9 (grant, revoke, role change, deactivation,
old-cookie behavior), using real SQL and the real authorization pipeline.
**Session 19 (DECISION_LOG §53) — authorization fix implemented (uncommitted, user request):**
handler no longer reads cookie permission claims, checks the DB every request with a
request-local `HttpContext.Items` cache; login no longer writes `AspNetUserClaims`. Tests A7
(revoke + legacy claim row), A8, A9 green (52/52); A4 comes with D-32. Legacy-row cleanup:
criteria reported, **not run** (separate approval).
**Session 20 (DECISION_LOG §55):** fix committed `fab1af1`. Cleanup target `CseLibrary` on
`.\SQLEXPRESS` checked read-only (user-approved): **the database does not exist** — no legacy
rows to delete, cleanup **not needed**. Test DBs are recreated per run. Re-check only if a
pre-`fab1af1` build is ever run against a real database.

### D-34 Error handling — Approved (standard); `Result` type Approved (Session 17); step 0.10 scope Approved and implemented Session 22 (`d0a8a66`)
**Session 22 (DECISION_LOG §67):** the user approved this 0.10 scope: friendly pages for 404 and
other suitable HTTP errors; no "Development Mode" text; users see only the Reference ID;
`TraceId` in the server log; no stack trace / exception details / sensitive data in production
responses; authorization, toast and AJAX-403 behaviour unchanged. Implemented: `UseExceptionHandler`
(outside Development, as before) + `UseStatusCodePagesWithReExecute("/Home/HttpStatus/{0}")`
(AJAX keeps the bare status); Reference ID = request `TraceId` (W3C hex); `{TraceId}` in the
Console and File output templates. Not in the approved scope (still open): dev SQL logging,
and the `WithMachineName`/`WithThreadId` enrichers listed without their packages (the app
starts, so they are silently ignored — they add nothing today).
**Session 17 (DECISION_LOG §45):** user approved `Result { Succeeded, ErrorCode, Error }` /
`Result<T> { Value }` with factories `Success()` / `Failure(code, safeMessage)` and `ErrorCodes`
constants (`COMMON.NOT_FOUND`, `COMMON.DUPLICATE`, `IDENTITY.FAILED`). All four template services
were migrated from tuples in `0d01e4e`. The remaining Proposed items below (exception handler,
status pages, Error view, TraceId) belong to step 0.10.
Approved (M2 Standards §3): centralized exception handling; no blanket try/catch; local catch only
for recovery/translation/retry/cleanup; expected business failures separate from unexpected
exceptions; never expose stack traces/SQL/paths/secrets; unexpected errors show a **Reference
ID** that is logged with full details; stable internal error codes; tests for both paths.
Proposed: `Result`/`Result<T>` with `ErrorCode` (e.g. `LENDING.COPY_UNAVAILABLE`) + safe
message in Core for new code (template tuples migrated when touched); `UseExceptionHandler`
outside Development; `UseStatusCodePagesWithReExecute`; Error view shows only the TraceId
(remove the template's "Development Mode" text); Serilog output template includes TraceId.
Verify at runtime: config lists `WithMachineName`/`WithThreadId` enrichers without their packages.

### D-35 Lending concurrency (Phase 3) — Proposed
`BookCopy.RowVersion` + filtered unique index `UX_Lending_ActiveBookCopy`
(`WHERE TransactionStatus = Issued`); `LendingTransaction.RowVersion` for double returns;
`UPDLOCK` on the member row inside the issue transaction for borrowing-limit races; conflicts
translated to business failures. **Supersedes** the earlier `ExecuteUpdateAsync … WHERE
Status=Available` proposal (bypasses change tracker and the audit interceptor).
**M9:** the borrowing-limit and outstanding-loan races (FR-M9-01/02) reuse this design — see D-44,
D-45. Status unchanged (Proposed).
**M10:** D-44/D-45 are Superseded by D-48, which reuses the same member-row lock. Status
unchanged (Proposed). **M12:** lock hints, isolation assumptions and tests for the member-row lock
are detailed in D-48 §4; this entry is not changed.

### D-36 Email reliability (Phase 4) — Proposed
Notification/outbox row inserted in the **same transaction** as the business change (the
archive's "queue after commit" can lose notifications on a crash). One `EmailNotifications`
table as outbox + log with unique `IdempotencyKey`; `BackgroundService` + `PeriodicTimer`
worker; retry with max attempts; status `Sent`/`AcceptedByProvider`, never claim `Delivered`.

---

## 5. Environment, databases and tests

### D-40 Database safety and test isolation — Approved
Never touch the user's existing databases, specifically the template DB `DefaultTemplate` on
`.\SQLEXPRESS`. Integration tests use an isolated database. **Source:** M3 item 7, M5 §3.
**Approved (M6 B):** separate dev DB `CseLibrary`; tests on LocalDB `(localdb)\MSSQLLocalDB`
with a uniquely named `CseLibrary_Test_{guid}` per run, migrated then dropped; fixture **refuses
to run** unless the DB name starts with `CseLibrary_Test_` (strict guard). No
`EnsureDeleted`/drop against any other name.
**Guard details (M8 §2):** the name is validated **before migration and before cleanup**; cleanup
may only target the exact database name created by that test run (the fixture keeps the generated
name and drops nothing else); disposable tests never use the `CseLibrary` development database.
Any migration reset or consequential schema change: show the proposed migration/script first and
obtain separate approval.
**Code-verified hazard (Session 2):** `Program.cs` runs `DbSeeder.SeedAsync` → `MigrateAsync()`
on every startup, and `appsettings.json` points at `Database=DefaultTemplate` on `.\SQLEXPRESS`;
the project also has a `UserSecretsId` whose secrets may override the connection string.
Therefore the app must **not be started** until the connection string resolves to `CseLibrary`
(verified read-only, incl. `dotnet user-secrets list`) — see step 0.4.
**Approved (Session 13, §29):** dev DB `CseLibrary` on `.\SQLEXPRESS` (new database);
connection string in `appsettings.Development.json` **and** the base `appsettings.json` changed
from `DefaultTemplate` to `CseLibrary`, so no environment can ever resolve to the template DB;
seed admin password in user-secrets (never in a committed file).
**Session 14 (§32–§34):** read-only check — `.\SQLEXPRESS` holds only the 4 system DBs (no
`DefaultTemplate`, no `CseLibrary`); no user-secrets, no env overrides. The user wants **no** DB
username/password (Windows auth already) and set **no** seed password; the app-start check runs in
the test suite instead (env-var connection string + in-memory test password, LocalDB
`CseLibrary_Test_*`). Running the app on `.\SQLEXPRESS` still needs `Seed:Admin:Password` in
user-secrets; nothing has been created there yet. Guard implemented in
`tests/CseLibrary.Tests/Infrastructure/TestDatabase.cs` (server **and** prefix, test H1).

### D-41 Rename to `CseLibrary.*` with `src/` + `tests/` layout — Approved (M6 B)
`src/CseLibrary.{Core,Data,Services,Web}`, `tests/CseLibrary.Tests`, solution at repo root.
Keep the existing `UserSecretsId` (Proposed, so existing local secrets keep working).
**Not covered by the approval — Proposed:** `global.json` (SDK pin) and
`.config/dotnet-tools.json` (local `dotnet-ef`); confirm before step 0.3.
**M9 conflict (not resolved, status unchanged):** M9 FR-M9-06 suggests `LibraryManagement.*`
names. D-41 stays Approved as recorded until the user decides — see D-47; step 0.3 is blocked
until then.
**Resolved (Session 13, §29):** `CseLibrary.*` kept (D-47 Rejected); `global.json` (SDK 10.0,
`rollForward: latestFeature`) and `.config/dotnet-tools.json` (local `dotnet-ef`) **Approved**
for step 0.3. Step 0.3 unblocked.
**Executed (Session 14, commit `3c8d98a`, §31):** `src/CseLibrary.{Web,Services,Data,Core}`,
`CseLibrary.sln` at root, `global.json` 10.0.100/`latestFeature`, `.config/dotnet-tools.json`
(`dotnet-ef` 10.0.12); `tests/CseLibrary.Tests` added in 0.5 (`22d27ae`). `UserSecretsId` and
migration ID kept (verified by test).

### D-42 Upgrade .NET 8 → .NET 10 LTS — Approved (M6 B), subject to compatibility checks
Reasons: .NET 8 support ends 10 Nov 2026; this machine has only .NET 10 runtimes (10.0.7), so
the net8.0 template builds but cannot run here. Condition: package, build and runtime
compatibility verified in step 0.4 (EF Core / Identity / Serilog versions, Dockerfile images);
any incompatibility is reported before working around it.
**Executed (Session 14, commit `468a82c`, DECISION_LOG §32):** net10.0; EF Core / Identity 10.0.12;
Serilog.AspNetCore 10.0.0 forced Serilog.Sinks.File 6 → 7 (NU1605); Docker images 10.0; EF reports
no pending model changes. Open: NU1510 (`Configuration.Abstractions` now in the shared framework).

### D-43 Test strategy — Approved (M6 B)
Order: characterization tests first (step 0.5), then unit, architecture, integration, security
and acceptance tests, with evidence at every review gate; integration tests exercise real SQL and
the real authorization pipeline where behavior depends on them (M6 A8).
One xUnit project (`Unit/`, `Integration/` folders), `Microsoft.AspNetCore.Mvc.Testing`
`WebApplicationFactory`, real SQL Server LocalDB (EF InMemory cannot test transactions/filtered
indexes), SQL command-counting interceptor for N+1 tests, `TimeProvider` for time-dependent
tests. Unverified risk: Serilog bootstrap logger with `WebApplicationFactory`
("logger already frozen").
**Executed (Session 14, `22d27ae`, §34):** risk handled by **one shared host per test run**
(`AppFactory` collection fixture); xUnit 2.9.3; 15 characterization tests green. Gate 1 review
(§35) found test-harness weaknesses (resolved-connection guard, env-var restore, weak drop test,
exact N+1 count) — fix decision open (§9).

---

## 6. Library features and naming (M9, Session 4)

Requirements are owned by `docs/requirements.md` (FR-M9-xx), rules by `docs/business-rules.md`
(BR-M9-xx). Entries here hold only design, conflicts and status. None is Approved. (M10 had
marked "requirement parts" of D-48 Approved; M11 moved them to `requirements.md` as [Confirmed]
requirements — DECISION_LOG §15.)

### D-44 Borrowing-limit enforcement (FR-M9-01) — **Superseded by D-48 (M10)**; was Proposed
> Kept for history. Its lock/count/retry/ambiguous-commit design is carried into D-48 unchanged;
> the single-value storage question (Q-M9-4) is replaced by the per-category policy table.
- Limit resolved by a policy lookup (`ILibraryPolicyService` or equivalent, Phase 3), never a
  literal `3` in code. Storage options: archive `LibraryPolicies` table (admin-editable, audited,
  per member category) vs `appsettings` (simpler, deploy to change). Recommendation: table, as the
  archive (FR-14) requires administrators to edit it — **Pending Q-M9-4**.
- Check at **issue** (and at request submission if requests exist, D-45) inside
  `ExecuteInTransactionAsync` (D-14): lock the member row (`UPDLOCK, HOLDLOCK` read in the
  repository, D-35) → count active loans (BR-M9-02) in the same transaction → insert. The lock
  serializes concurrent issues **for the same member only**. A plain C# count without the lock is
  **not** concurrency-safe (two transactions both see `limit − 1`).
- Retry rule: the lock read and count happen inside the re-runnable delegate (D-14 rule 1).
- Ambiguous commit (D-14 rule 3): a repeated issue after an unknown commit outcome is caught by
  the copy-level filtered unique index (D-35) for the same copy; a different copy for the same
  member is **not** caught by any constraint → needs an idempotency key on the issue form. To be
  assessed at the Lending gate; never claimed "fully proven".
- Affects: D-14, D-35 (reused, unchanged), D-37 (issue permission), D-21 (policy table audited if
  chosen).
- **Not concurrency-safe until I5/I9 pass.**

### D-45 Outstanding-loan restriction and borrowing requests (FR-M9-02) — **Superseded by D-48 (M10)**; was Pending (conflict)
> Kept for history. M10 resolved both points: the conflict disappears because "outstanding loan
> blocks" and "limit 3" are two values of one per-category setting (`MaxActiveLoans`), and the
> overdue variant is its own setting; option (ii) `BorrowRequest` workflow is **not** introduced
> (scope, BR-M10-09) — Q-M9-2 answered (i).
> **M11 correction:** the "two values of one setting" equivalence is withdrawn; "any active loan
> blocks" is its own setting `BlockIssueWhenAnyActiveLoan`, independent of `MaxActiveLoans`.
- **Conflict with FR-M9-01:** "no new request while any unreturned loan exists" ⇒ effective limit
  1, contradicting limit 3. Alternatives (Q-M9-1):
  - (a) **Only overdue loans block** new requests; otherwise up to the limit. Reconciles both
    requirements. *Assistant's recommendation — not approved.*
  - (b) Any active loan blocks ⇒ limit is effectively 1; FR-M9-01 then only matters if the
    value is later lowered/raised — the "3" would never apply.
  - (c) Any active loan blocks **member self-service requests**, while staff may still issue up to
    the limit at the desk.
- **Scope gap with D-02/archive (Q-M9-2):** no member "borrowing request" exists in the approved
  scope (staff issue directly; reservations deferred). Options: (i) FR-M9-02 refers to the
  staff issue attempt → no new entity; (ii) new member-submitted `BorrowRequest` workflow
  (Pending → Approved/Rejected → Issued) → new entity, portal pages, statuses, notifications;
  scope change to D-02 requiring explicit approval.
- Enforcement whichever option is chosen: server-side in the service; re-checked at issue
  (BR-M9-07). Concurrency: same member-row lock as D-44 inside one transaction. If option (b) is
  chosen, a filtered unique index on active loans per member (`UNIQUE (MemberId) WHERE
  ReturnDate IS NULL`) would enforce it in the database itself; for options (a)/(c) no single
  index can express "count < N", so the lock is required. Duplicate request submission (double
  click, ambiguous commit) → filtered unique index "one pending request per member per book"
  (option ii) or an idempotency key.
- Blocks: Lending phase design. Does not block Phase 0.

### D-46 Incremental list loading with jQuery AJAX (FR-M9-04) — Proposed
- Progressive enhancement: the server-rendered page shows the first page plus a **"Load more"**
  link (`?after=<cursor>&<filters>`) that works without JavaScript; jQuery intercepts it,
  requests a partial view (rows only + next cursor) and appends rows.
- Same controller action / service method for both paths (D-15 keyset), so filters, sort,
  authorization and paging cannot diverge (K1, K3).
- Client: button disabled + `aria-busy` while loading (no duplicate requests); rows skipped if
  their `data-id` is already present (duplicate guard); end-of-data hides the button and announces
  "No more results" in an `aria-live="polite"` region; errors show a retry message; changing a
  filter/sort reloads from the first page (never mixes cursors).
- Expired session: cookie auth currently redirects to Login (HTML 302), which would be appended
  as rows. Proposed: for AJAX requests (`X-Requested-With: XMLHttpRequest`) return **401/403**
  via the cookie `OnRedirectToLogin`/`OnRedirectToAccessDenied` events; client shows "Session
  expired — reload". Implemented with the first AJAX list (Phase 1), not in Phase 0.
- UX pattern alternatives (Q-M9-7): **Load More button** (recommended — keyboard/screen-reader
  friendly, footer reachable, explicit user action) · infinite scroll (needs extra a11y work,
  footer unreachable) · numbered pages (requires offset paging, conflicts with keyset).
- No new frontend library (jQuery already shipped in `wwwroot/lib`).

### D-47 Project naming: `LibraryManagement.*` (M9) vs `CseLibrary.*` (D-41) — **Rejected (Session 13): `CseLibrary.*` kept (D-41)**; four-project, namespaces-only structure below stays valid
Current projects (code-verified, Session 4) → layer → D-41 name → M9 suggestion:
| Current | Layer | D-41 (Approved) | M9 (suggested) |
|---|---|---|---|
| `DefaultTemplate` (web, `Program.cs`, `UserSecretsId`) | Web / composition root | `CseLibrary.Web` | `LibraryManagement.Web` |
| `Default.service` | Services | `CseLibrary.Services` | `LibraryManagement.Services` |
| `Default.core` | Core | `CseLibrary.Core` | `LibraryManagement.Core` |
| `Default.repository` | Data | `CseLibrary.Data` | `LibraryManagement.Data` |
| — | Tests | `tests/CseLibrary.Tests` | `tests/LibraryManagement.Tests` |
- Both proposals have the **same four projects, same `src/`+`tests/` layout and same dependency
  direction** (D-10); only the prefix differs. The structure is therefore already agreed; only
  the name needs a decision (Q-M9-9). D-41 stays Approved until the user chooses; if the user
  picks `LibraryManagement.*`, D-41's naming clause becomes Superseded (layout clause stays).
- **No additional projects** for `Domain`, `Entities`, `Repository`, `Service` (Proposed):
  they become namespaces/folders — `*.Core.Entities`, `*.Core.Interfaces`, `*.Data.Repositories`,
  `*.Services.<Feature>`. Extra projects add references and build time without a boundary the
  four projects do not already enforce (E1, E4).
- Dependency direction (unchanged, D-10): Web → Services, Core; Web → Data **only** for DI
  wiring in `Program.cs` (composition root); Services → Core; Data → Core; Core → no EF Core, no
  `Microsoft.AspNetCore.App`. Today Core references `Microsoft.Extensions.Identity.Stores`
  (for `IdentityUser`) — allowed (not EF/ASP.NET Core). Today Services references Data,
  `Microsoft.AspNetCore.App` and `Identity.EntityFrameworkCore` — removed in 0.8 (D-10).
- Rename implications (step 0.3, whichever name): folder + `.csproj` names, `RootNamespace`/
  `AssemblyName`, all `namespace`/`using` lines, `.sln` (moved to repo root), `ProjectReference`
  paths, `Dockerfile` `COPY`/`WORKDIR`/entry DLL, `.dockerignore`, `launchSettings.json` profile
  name, Serilog/appsettings entries naming the assembly (if any), `MigrationsAssembly` option (if
  set) and `dotnet ef --project/--startup-project` paths, migration `.cs` + snapshot namespaces
  (migration IDs and schema unchanged; EF matches migrations by ID, not namespace — to be
  confirmed by `dotnet ef migrations list` against a `CseLibrary_Test_*` DB only), keep
  `UserSecretsId`. Database names (`CseLibrary`, `CseLibrary_Test_*`, D-40) are **independent of
  the project name** and are not renamed by this decision.
- **Verify:** step 0.3 build; E4 + E1 (step 0.8).
- **Blocks:** step 0.3.

### D-48 Borrowing policy system (FR-M10-01, M10; revised M11, M12) — Proposed
Supersedes D-44, D-45. Rules: BR-M10-01..14. Tests: P1–P21 (`requirements.md`).
**Status: Proposed — the whole entry, until the user reviews the architecture (M11 §3).**
**Implemented: no · Verified: no** — in particular the concurrency protection stays Proposed
until P12, P14, P17, P18, P19 and P21 pass. Target: Lending phase (Phase 3), after Members
(Phase 2). Archive reconciliation (what is kept, deferred, conflicting): FR-M10-01 table.
Review timing: before the Lending phase, not before Phase 0 (Q-M10-1).

**Inputs (requirements, not approvals of this design).** The user's stated requirements in
`requirements.md` FR-M10-01 ([Confirmed]): per-category configuration by authorized staff, never
hard-coded; values configurable, logic not; no default values without approval; no reservation /
request workflow; no generic rule engine. *M10 history:* M10 recorded these as "Approved parts"
of D-48; M11 reclassified them as requirements so that no part of this design reads as approved.

**1. Configuration (Proposed).**
- Core entity `LibraryPolicy` → the archive's table `LibraryPolicies` (spec v1.1 §4.3: "lending
  duration and borrowing limits by member type"; SRS §8 page `/Admin/LibraryPolicies`): `Id`,
  `MemberCategory` (**unique**),
  `MaxActiveLoans` (int, `CHECK ≥ 0`), `BlockIssueWhenAnyActiveLoan` (bit),
  `BlockIssueWhenOverdue` (bit), `LoanDurationDays` (int, `CHECK ≥ 1`), `RowVersion`, audit
  fields. **No column `DEFAULT`s.** `IAuditable` + per-property `.Audited()` (D-22, D-23) so the
  change history lives in `AuditLogs` (D-21); no separate history table; no delete operation.
- One row per category, seeded by the Lending migration with values the user has approved
  (Q-M10-3). Until they are approved, the step that adds this migration is blocked — no
  placeholder values are invented.
- Name (M12 correction): M10/M11 proposed renaming the table to `BorrowingPolicies`, claiming
  the archive's `LibraryPolicies` mixed per-category rules with global settings. That was
  inaccurate — spec v1.1 defines `LibraryPolicies` as exactly the per-member-type duration and
  limit table; only SRS FR-14 ("Library Policy Settings") lists broader settings, and the SRS does
  not say which table stores them. The rename is withdrawn; the
  archive name is kept. FR-14's global settings (reminders, email sender, contacts, holidays)
  are not columns of this table; they get their own owner in their phase.
- Seeded rows are written by a migration, which the audit interceptor does not see (D-25): the
  initial values are traceable through the migration and the DECISION_LOG approval, and
  `AuditLogs` history starts with the first edit. No rows are inserted at runtime (no create
  operation), so the D-29 HiLo question does not arise for this table.
- **Values vs logic (M11):** the policy screen edits only these typed values (range-validated in
  the service and by DB `CHECK`s). The meaning and combination of the settings live in the
  evaluator (code). No expression, script or query is accepted or stored (P20). What future
  changes need code or migrations: table "What is configurable" in FR-M10-01. Adding a setting
  is localized (column + evaluator check + snapshot field if new data is needed + screen field +
  tests + migration) but is development work; it is not promised to be "configuration only".
- Admin UI: list (one row per category) + edit, behind permission `LibraryPolicies.Manage`
  (D-37; holder Q-M10-6).

**2. Evaluation (Proposed).**
- `BorrowingEligibility.Evaluate(LibraryPolicyRules policy, MemberLoanSnapshot snapshot)` in
  Services: a **pure static function**, no interface (one implementation), no DB, no clock. The
  snapshot carries every input: `IsActive`, `ActiveLoanCount`, `OverdueLoanCount`. Semantics
  (AND of four independent checks, all reasons returned): FR-M10-01 "Evaluation semantics";
  full combination table: test P16.
- Snapshot counts read by **one** projection query in the lending repository
  (`GetMemberLoanCountsAsync(memberId, now)`: two conditional counts, no entity loading, D-15);
  `now` from `TimeProvider`.
- Defaults/overrides (Proposed, Q-M10-4): exactly one policy per category, no global default, no
  per-member override. If overrides are approved later: "most specific wins" (member override >
  category) per setting — not built now.
- Activation (Proposed, Q-M10-5): edit in place, effective for operations that read it after
  commit; existing loans keep their stored due date (BR-17). Future-dated `EffectiveFrom`
  versions not in v1.

**3. Integration with the lending service (Proposed).**
- `LendingService.GetEligibilityAsync(memberId)` — read-only, no transaction, no lock: policy +
  counts → `Evaluate`. Used by the Issue Book page (FR-06 step 2), optionally the portal.
  Advisory only (BR-M10-08).
- `LendingService.IssueAsync(...)` — the **single entry point that creates loans** (P17). Inside
  `IUnitOfWork.ExecuteInTransactionAsync` (D-14), all reads inside the re-runnable delegate:
  1. `UnitOfWork.Members.LockForIssueAsync(memberId)` — locking read that also returns
     `IsActive` and `MemberCategory` (so the category and status used are the locked ones).
  2. `UnitOfWork.LibraryPolicies.GetByCategoryAsync(category)`; missing → fail closed.
  3. `UnitOfWork.LendingTransactions.GetMemberLoanCountsAsync(memberId, now)` — after the lock
     (property name per D-13).
  4. `Evaluate`; any reason → failed `Result` → rollback.
  5. Copy check + insert loan with `DueDate = issueDate + LoanDurationDays` (copy race: D-35
     `RowVersion` + filtered unique index); outbox row (D-36); one commit.
- Services use only `IUnitOfWork` typed repositories; the lock SQL lives in the Data layer
  repository; no EF query in services; controllers call services only (D-12, D-10).

**4. Member-row locking — assumptions, coverage, tests (Proposed; not Verified).**
- *Mechanism:* `SELECT Id, IsActive, MemberCategory FROM Members WITH (UPDLOCK, HOLDLOCK,
  ROWLOCK) WHERE Id = @id` via `FromSql` in the member repository (EF Core has no lock-hint API).
  The update (U) lock on the member key is held until commit/rollback; a second `UPDLOCK` reader
  of the same row waits; plain readers are not blocked.
- *Required transaction/isolation assumptions:*
  1. The lock read runs inside the explicit transaction of `ExecuteInTransactionAsync` (outside a
     transaction the lock is released at statement end and protects nothing).
  2. Isolation level **READ COMMITTED**, with `READ_COMMITTED_SNAPSHOT` either OFF or ON: the
     count is a new statement that starts after the lock wait, so it sees the other issue's
     committed loan. **SNAPSHOT isolation must not be used** for this transaction (its reads
     would come from the transaction-start snapshot and miss that loan). Proposed: D-14's
     `ExecuteInTransactionAsync` begins transactions with an explicit `ReadCommitted` level.
  3. The lock is the **first** data access in the delegate; policy and counts are read after it.
  4. The whole delegate uses one connection/transaction (EF transaction on the scoped context).
- *Does every issue path acquire the same lock?* **Design intent, not yet a guarantee (M12
  correction of the M11 wording "yes by construction"):** the only permitted issue path is
  `IssueAsync`, which calls `LockForIssueAsync` first. Today there is no code, so nothing
  enforces this. P17 (b) can check the call order; P17 (a) — "no other caller creates loans" —
  needs a method-level reference check whose mechanism is not chosen yet (may need a test
  package → separate approval); until then coverage rests on code review. Future loan-creating
  features (renewal, bulk issue, import) must call the same method.
- *Other writers of the member row:* a category change or member deactivation that is an
  `UPDATE` of the **member row** takes an exclusive lock that conflicts with the issue's update
  lock, so the two serialize (P19). **Assumption:** member category and borrowing status
  (`IsActive`) are columns of the member row — the Members design is Phase 2 and `IsActive` vs
  `IsDeleted` is still open. Deactivating a member's *portal login* (D-32, `AspNetUsers`) is a
  different row and does not serialize with issue; whether it affects borrowing eligibility is
  not decided (Phase 2). Return does **not** take the member lock: it only lowers the count, so
  the worst case is a conservative refusal of a concurrent issue.
- *Lock order and deadlocks:* issue locks member → copy → inserts loan; return touches loan →
  copy of a different (issued) copy. No cycle is expected. A deadlock victim (SQL 1205) is
  expected to be retried by the EF SQL Server execution strategy — **unverified; P21 (a)**.
- *Lock waits:* bounded by the command timeout; a timeout must fail safely (no partial write);
  whether it is retried is observed in **P21 (b)**, not assumed.
- *Limitations:* direct SQL writes or data fixes that bypass `IssueAsync` are not protected; the
  database cannot express "count < N per member". A trigger was considered and rejected
  (duplicates the evaluator logic). Ambiguous commit (D-14 rule 3): same copy → filtered unique
  index; different copy, same member → issue-form idempotency key, assessed at the Lending gate.
- *Tests:* P12 (N parallel issues, exactly one succeeds), P14 (retry re-reads), P17 (coverage),
  P18 (deterministic blocking with RCSI OFF and ON; other members not blocked), P19 (category
  change / deactivation vs issue), P21 (deadlock retry, lock-wait timeout). Every protection in
  this section stays **Proposed** until these pass on LocalDB `CseLibrary_Test_*`; none is
  Verified.

**5. Architecture assessment (M11 §3) — for the user's review; recommendations not approved.**
| Element | Assessment | Main risk | Alternatives considered | Recommendation |
|---|---|---|---|---|
| Typed policy table | DB `CHECK` + unique category, typed reads, audited via existing interceptor, trivial EF mapping | A new setting needs a migration | Key/value table (no types/constraints, string parsing); `appsettings` (deploy to change; fails "staff configure"); JSON column (no DB constraints) | Typed table |
| Pure evaluator | Same code for advisory check and enforcement; full matrix unit-testable without DB (P16) | Snapshot must be extended when a rule needs new data | Rules inside `IssueAsync` (untestable without DB, duplicated in eligibility); rule-object list / strategy (abstraction for four checks) | Pure static function |
| Missing policy | Fail closed: refuse with a configuration error, never unlimited | A missing seed row blocks a whole category | Global default row (hides misconfiguration); unlimited (unsafe) | Fail closed + seed + no delete + unique index + test that every category has a row |
| Category change | Category read inside the locked read; change serializes with issue on the member row | Member above new category's limit is blocked (intended, BR-M9-04) | Re-evaluate existing loans (contradicts BR-17) | Next issue uses new category (BR-M10-12, Q-M10-8) |
| Policy-edit concurrency | `RowVersion` optimistic concurrency → `ConcurrencyConflictException` (D-14) → business error (P13); issues read the committed value, no lock on the policy row | An issue in flight may use the value committed just before an edit | Pessimistic lock on the policy row (makes edits wait for issues; no benefit) | Optimistic `RowVersion` |
| Member-row lock | Serializes only same-member issues; cheap point lock; designed to work with RCSI on/off (P18 to prove) | Every loan-creating path must call it (P17, mechanism open); SNAPSHOT isolation must be avoided; category/status must live on the member row | `SERIALIZABLE` on the count (range locks, deadlock-prone); `sp_getapplock` (string keys, outside EF, easy to forget); optimistic "touch member `RowVersion`" (EF-native but writes the member row and audit noise on every issue) | `UPDLOCK, HOLDLOCK, ROWLOCK` row lock |

**Affects:** D-12, D-14 (explicit `ReadCommitted` level — Proposed), D-15, D-21–D-23 (policy
audited), D-35 (reused, unchanged), D-37 (new permission), D-02 (scope unchanged).

### D-49 Library database design (FR-M15-01..03, M15) — Proposed
Schema, derived availability, open-loan invariant and append-only lending history as written in
[database.md](database.md); open choices O1–O5 there (recommendations: derived count, `Issued`
kept in `CopyStatus`, single `Members` table, live FK in history, `bigint` loan id). Diagram:
`docs/diagrams/database-model.html`. **Source:** M15 (DECISION_LOG §33, §40). Reuses D-35, D-48
unchanged. **Implemented: no · Verified: no.** Tables are built in their phase migrations, not in
Phase 0. **Session 16:** ER diagram `docs/diagrams/database-tables.html` (all columns, crow's-foot
FK lines); naming and status follow D-50, D-51.

### D-50 Naming and row audit columns — Approved (Session 16, user)
- Primary key is `Id`; a foreign key is the singular entity name + `Id` (`BookId`, `CategoryId`);
  two FKs to one table get a role prefix (`IssuedByUserId`).
- Every `BaseEntity` table has `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy` (template names
  kept — user chose option A over `Modified…`); join tables only `CreatedAt`/`CreatedBy`; log
  tables none. Values are UTC and set automatically by a `SaveChanges` interceptor from the current
  user and `TimeProvider` — never by services. They show the last change only; full history is
  `AuditLogs` (D-21). **Source:** user, Session 16 (DECISION_LOG §41). **Implemented: yes —
  `0d01e4e` (Session 17, §46)** via `AuditColumnsInterceptor` + `ICurrentUser` (Core, `UserId` only;
  `HttpCurrentUser` in Web) + `TimeProvider`. `AspNetUsers`: `CreatedAt`/`UpdatedAt`, `AspNetRoles`:
  `CreatedAt` (per ER diagram); `RolePermissions` keeps `AssignedAt`. Open: review CR6 (Identity's own
  writes also bump `UpdatedAt`) — recommendation keep (§47).

### D-51 Single `RecordStatus` instead of `IsActive` + `IsDeleted` — Approved (Session 16, user)
`enum RecordStatus : byte { Active = 1, Inactive = 2, Deleted = 3 }` (0 invalid; new values appended,
never renumbered) in Core; column `Status tinyint` + `CHECK`. Replaces `BaseEntity.IsDeleted` and
`ApplicationUser.IsActive` (login requires `Active`). One global query filter hides `Deleted` for
every `BaseEntity`; the interceptor turns repository `Remove()` into soft delete; unique indexes are
filtered `WHERE Status <> 3`. Business states (`CopyStatus`, `TransactionStatus`) stay separate
columns. User proposed HTTP-style values (200/404); accepted the assistant's 1/2/3 + `Inactive`
instead. Template migration converts `IsDeleted = 1 → 3`, else `1`; `IsActive = 0 → 2`.
**Source:** user, Session 16 (DECISION_LOG §41). **Implemented: yes — `0d01e4e` (Session 17, §46)**;
migration `RecordStatusAndAuditColumns` hand-edited for the data conversion and tested up/down on
existing rows. `AspNetUsers` has the `CHECK` but no query filter (users are deleted by
`UserManager`). Review CR5, CR9 fixed in `8cd5f53` (§48). Resolves the
D-48 open point "`IsActive` vs `IsDeleted`".

### D-52 Unique user email in the database — Approved (Session 18, user)
Members log in with email + password, so `AspNetUsers.EmailIndex` on `NormalizedEmail` becomes a
**unique** index filtered `[NormalizedEmail] IS NOT NULL` (users are hard-deleted, so no `Status`
filter). `RequireUniqueEmail` stays as the friendly pre-check; a duplicate past it is
`ErrorCodes.Duplicate` ("User name or email is already taken.") on user create and update.
No index on the password: only a salted hash is stored, every hash differs even for equal
passwords, and login never searches by password. A migration fails on a database that already
holds duplicate emails; clean them first.
**Source:** user, Session 18 (DECISION_LOG §50); closes the D-14 rule 3 duplicate-email gap (§48).
**Implemented: yes — `70bad0f`, merged into `develop` `e9a0536` (Session 18, §50);** migration
`UniqueUserEmail`, test: two users with one email → `DuplicateKeyException`.
**Session 19 note:** the "users are hard-deleted" premise no longer holds once D-53 is implemented —
a soft-deleted user keeps the email in the unique index (open question Q-S19-16).

### D-53 User records are never physically deleted — Approved (Session 19, user requirement); implementation not approved
- Never physically delete a row from `AspNetUsers`. Deleting a user in the application keeps the
  row and sets `Status = Deleted` (`RecordStatus.Deleted` = 3 already exists; no schema change).
- A deleted user cannot sign in or reach protected functionality (regardless of registration
  type or approval state, D-54); the user's ID and historical relationships are preserved.
- Distinct from the D-37 cleanup of legacy permission rows in `AspNetUserClaims` (no
  `AspNetUsers` row is deleted by that cleanup).
**Findings (read-only inspection, Session 19, DECISION_LOG §53):**
1. `UserService.DeleteAsync` calls `UserManager.DeleteAsync` → **physical `DELETE` of the
   `AspNetUsers` row**; Identity's cascade also removes the user's `AspNetUserRoles`,
   `AspNetUserClaims`, `AspNetUserLogins`, `AspNetUserTokens` rows. Reached from
   `UsersController.DeleteConfirmed` (`Users.Delete` permission). This is the only code path
   that deletes users (seeder and tests do not).
2. `AuditColumnsInterceptor` turns deletes into soft deletes only for `BaseEntity` (D-51);
   `ApplicationUser` is not a `BaseEntity`, so there is no safety net today.
3. `UserService.UpdateAsync` sets `Status = dto.IsActive ? Active : Inactive` — editing a
   soft-deleted user would **reactivate** it; needs a guard.
4. Sign-in: `AccountController.Login` rejects `Status != Active` (so `Deleted` is rejected);
   permission checks require `Status = Active` (`PermissionRepository.GetNamesForUserAsync`).
   An existing cookie still reaches authenticated-only pages until the security stamp changes →
   delete must also call `UpdateSecurityStampAsync` (same mechanism as D-32).
5. `UserReadRepository` lists all users with no status filter — deleted users would appear in the
   Users list (Q-S19-15).
6. Not in scope but same pattern: `RoleService.DeleteAsync` physically deletes roles.
**Approved Session 20 (2026-10-14, DECISION_LOG §55):** Q-S19-15 — `Deleted` users are **hidden
by default** in the normal Users list; an authorized admin view may show them when needed.
Q-S19-16 — a deleted user's email **cannot be reused for now**; the D-52 unique index stays
unchanged. The code change still needs a separate execution approval.
**Implemented Session 20 — committed `8550c00` (DECISION_LOG §55):** `DeleteAsync` sets
`Status = Deleted` and saves it through `UpdateSecurityStampAsync` (one write, stamp changed);
`UpdateAsync`/`DeleteAsync`/`ResetPasswordAsync` treat a deleted user as not found;
`UserReadRepository` hides deleted users from list + details. No admin "show deleted" view yet
(built when needed). Test `Deleted_user_is_kept_but_locked_out_and_hidden`; 53/53; mutation check
(hard delete restored → test fails). Limit: an old cookie on an authenticated-only page (no
permission check) lasts until stamp validation — interval is D-31 (Proposed 5 min, default 30).
**Proposed implementation (original text):** `DeleteAsync` sets `Status = Deleted` + stamp update
via `UserManager.UpdateAsync` in one transaction; `UpdateAsync` refuses to change a deleted user;
tests: row kept, sign-in refused, existing cookie denied, edit cannot resurrect.

### D-54 Self-registration (Q-S19-1) — design Approved (Session 19); implementation not approved
**Source:** user, Session 19 (DECISION_LOG §53). Recorded as a design decision; building it needs
separate approval.
1. **Form:** one Registration form with a radio choice **`Student`** / **`Dept. Fellow`**.
   **Control superseded Session 21 by D-56 (Q-S21-3): a dropdown** populated from the
   `RegistrationType` enum; the two choices themselves are unchanged.
2. **Student:** a successful registration immediately creates an **Active** account, subject to
   the agreed account-security rules (D-31). Choosing `Student` grants **no** privileged role or
   permission; access follows the configured authorization policy.
3. **Dept. Fellow:** a successful submission creates an account/application in **Pending**
   state. A Librarian reviews it manually and **approves** or **rejects** it. `Pending` and
   `Rejected` cannot log in. After approval the account may become `Active` and log in, provided
   it is not `Deleted` and all other sign-in checks pass. Approval grants **no** automatic access
   to all resources; access follows the permissions configured by the System Admin.
4. **Retention:** user rows are never physically deleted (D-53); a `Deleted` account cannot log
   in regardless of registration type or approval state.
5. **Separate workflows, shared logic:** Student and Dept. Fellow registration are separate,
   explicit workflow methods. Both reuse common internal logic where it is genuinely shared
   (validation, identity creation, …); workflow-specific validation, status transitions and
   approval rules stay separate so future Dept. Fellow rules can change without affecting Student
   registration. **No Dept. Fellow special rules are invented now** — discussed later.
6. Selecting a registration type never grants privileged roles or resource permissions;
   authorization stays governed by the configured permissions (D-37).
**Status model gap (decision required, no schema invented):** `ApplicationUser.Status` is
`RecordStatus` {`Active`, `Inactive`, `Deleted`} — the **account** state. It cannot represent
**registration approval** (`Pending`, `Approved`, `Rejected`), and folding those into
`RecordStatus` would mix two meanings (e.g. a rejected fellow later deleted). Registration
approval needs its own representation (e.g. a separate approval-state field on the user, or a
separate registration-application record) — Q-S19-8. Sign-in (`CanSignInAsync`, D-32) must then
require account `Active` **and** approval not `Pending`/`Rejected`.
**Approved details Session 20 (2026-10-14, DECISION_LOG §55)** — implementation still not approved:
- **Q-S19-3 Fields / identity:** the necessary identity and credential fields; **email
  confirmation will be considered** (not decided yet; the exact field list is set at design time).
- **Q-S19-4 Fellow activation:** Librarian approval makes the Fellow account **Active**, subject to
  the other security checks.
- **Q-S19-5 Member link:** if a `Member` record already exists, validate it and link it; **no
  duplicate member record is created automatically**.
- **Q-S19-6 Approvers:** a dedicated **`Registrations.Review`** permission.
- **Q-S19-7 Resubmission:** a rejected applicant **may apply again**, but never as a duplicate
  account or duplicate application.
- **Q-S19-8 Approval state:** kept **separate from `RecordStatus`** (exact schema still needs a
  design + approval).
- **Q-S19-9 Phase:** Registration is implemented **outside Phase 0**.
- **Q-S19-17 Student default access:** a **System Admin-configured default member role**, with no
  privileged permission. **Refined Session 21 by D-56 (Q-S21-1):** separate Student and Fellow defaults.
Items 1–6 above stay in force (Student → Active; Fellow → Pending → Librarian review; separate
workflow methods with shared logic; no physical `AspNetUsers` delete, `Status = Deleted`).

### D-55 UI notifications and error handling — design Approved (Session 19); implemented Session 22 (`42ae3f4`)
**Source:** user, Session 19 (DECISION_LOG §53).
1. **One reusable toast mechanism** for the whole application (shared component/service, not
   per-page logic), top-right:
   - **Success:** green, auto-dismiss ≈ 3 s, close (`×`) button.
   - **Error:** red, auto-dismiss ≈ 5 s, close (`×`) button, safe user-friendly message only.
   - **Warning / info:** distinct accessible styles; durations by importance (values open, Q-S19-12).
2. **Validation:** an overall toast when useful **plus** field-level messages beside the fields;
   important feedback never only in a temporary toast; entered data preserved where appropriate.
3. **Authorization:** the server always rejects an operation without the permission; the user
   sees a red toast with a generic message ("You don't have permission to perform this
   action."), **not** a redirect to a separate AccessDenied page when the current page can handle
   the response. Authentication-required requests still follow the login / session-expiry flow.
   A toast never replaces server-side authentication or authorization.
4. **Errors:** expected application errors → user-friendly notifications; unexpected errors →
   centralized exception handling; never stack traces, DB details, connection strings or other
   sensitive data in user-facing text; diagnostics logged server-side (D-34, Serilog); a fallback
   response stays for cases where the original page cannot be rendered.
5. **Consistency / accessibility:** limit simultaneous toasts; readable, accessible text; never
   colour alone (text or icon too); consistent timers and close buttons.
**Reconciliation with existing decisions:**
- **D-31 access design / Q-S19-2:** public `Home.Index`/`Privacy` unaffected. `AccessDenied`
  stops being the normal way to show a permission failure; it may remain only as the fallback
  when there is no page to return to (e.g. a forbidden URL typed directly) — Q-S19-10.
- **D-34:** the Error page with Reference ID only remains the fallback for unexpected errors;
  whether an error toast also shows the Reference ID is Q-S19-11.
- **Test A2** ("authenticated without permission → AccessDenied/403") and the existing test that
  expects a redirect to `/Account/AccessDenied` will change when D-55 is implemented.
- Today controllers already put messages in `TempData["Success"]` / `TempData["Error"]`; the
  toast mechanism can render these (reuse, no new message channel needed).
**Approved details Session 20 (2026-10-14, DECISION_LOG §55)** — built in step 0.10:
- **Q-S19-10 No page to return to:** go back to a **safe local** return location if there is one,
  otherwise Home, with the toast; AJAX/API requests get the appropriate **401/403**; **open
  redirects are prevented** (local URLs only).
- **Q-S19-11:** the error toast **shows the Reference ID**.
- **Q-S19-12:** warning ≈ **5 s**, info ≈ **3 s**; at most **3** toasts at once; the timer
  **pauses on hover/focus**.
- **Q-S19-13:** verify the existing implementation first, then use **Bootstrap's toast** and the
  **current `TempData` keys**.
- **Q-S19-14:** implemented in **step 0.10**, together with the **test A2** update.

### D-56 Registration type is a closed enum, not a role — design Approved (Session 21); staff-side rules Q-S21-4 … 15 implemented Session 22 (`b1bba18`); registration itself not built (outside Phase 0)
**Source:** user, Session 21 (DECISION_LOG §58). Refines D-54; D-54 items 1–6 and its Session 20
details stay in force. Registration is still built outside Phase 0 (Q-S19-9).
1. **`RegistrationType` enum** with exactly two values: **`Student`**, **`DepartmentFellow`**
   (UI label "Dept. Fellow"). It lives in Core with the other domain enums.
2. **Not an authorization concept:** `RegistrationType` is only the applicant's registration
   choice. It is never a role, claim or permission, and authorization never reads it (D-37).
3. **Choice source:** the public Registration form's choices come from `RegistrationType` only.
   The database role list (`IRoleService` / `RoleReadRepository`) is **never** used to populate
   or validate the public form, so anonymous users can neither see nor submit role names such as
   `Admin`.
4. **Server-side enforcement:** the registration service validates the posted value with
   `Enum.IsDefined` (model binding accepts undefined numbers such as `"7"`) and rejects anything
   else; the form carries no role or permission field, and any extra posted field is ignored by
   the view model. Public registration can never assign `Admin` or any other privileged role.
5. **Workflows (from D-54):** `Student` → Student workflow → `Active` after successful
   registration, subject to all other security checks (D-31). `DepartmentFellow` → Fellow
   workflow → `Pending`; never `Active` until a user with **`Registrations.Review`** approves it
   manually (Q-S19-6). Reject is the same permission. Both workflows are separate service methods
   that share common logic (validation, identity creation); no extra Fellow verification rules
   are invented now.
6. **Access after activation:** only the configured member role/permissions are applied (per-type
   default roles, Q-S19-17 → Q-S21-1); approval never grants all permissions or a privileged role.
7. **Retention / sign-in:** no physical `AspNetUsers` delete; `Status = Deleted` users cannot sign
   in or reach protected functionality (D-53, D-32). Sign-in also requires approval not
   `Pending`/`Rejected` (D-54 status-model gap, Q-S19-8 schema still open).
8. **Layering:** registration rules live in a registration service (Services layer) using
   `IUnitOfWork`; controllers only map the form. `IRoleService` / `RoleService` /
   `RoleReadRepository` remain the path for **staff-side** role operations (e.g. the
   `UsersController` role list on the `Users.Create`/`Users.Edit` pages, which are not public).
**Approved details Session 21 (2026-10-14, DECISION_LOG §59)** — implementation still not approved:
- **Q-S21-1 Default roles:** two separate **System Admin-configured** default roles, one for
  `Student` and one for `DepartmentFellow` (refines Q-S19-17, which named only the Student role).
  Both must be **non-privileged**; the server validates the configuration and refuses a
  privileged role as a registration default. The Student role is applied on successful Student
  registration, the Fellow role only on approval (item 5).
- **Q-S21-2 Storage:** `RegistrationType` is stored on the **registration application record**
  (Q-S19-8). It is a business classification only — never an authorization role, claim or
  permission.
- **Q-S21-3 Form control:** a **dropdown** populated from the `RegistrationType` enum, with
  server-side validation of the submitted value (item 4). Supersedes the radio in D-54 item 1.
- **Q-S21-4 `Admin` role assignment (staff Users page):** `UserService` enforces server-side that
  only an `Admin` user may assign or remove the `Admin` role, on create and edit. This prevents
  privilege escalation, including self-promotion by a holder of `Users.Create`/`Users.Edit`.
  Existing authorization checks stay unchanged (`[HasPermission(Users.*)]`, D-37 database check,
  D-31 fallback policy); the rule is an additional check, not a replacement.
  **Scope extended by Q-S21-8:** the rule covers every privileged role (Q-S21-5), not only `Admin`.
**Approved details Session 21 (2026-10-14, DECISION_LOG §60)** — implementation still not approved:
- **Q-S21-5 Privileged role:** a role is privileged if it has any permission **outside the
  approved member-permission allowlist**. The allowlist is defined when member-portal permissions
  are designed; until then it is empty, so every role holding at least one permission counts as
  privileged (confirmed by Q-S21-10).
- **Q-S21-6 Admin identification:** Admin-specific security checks use the **literal role name
  `"Admin"`**. Conflict with the configurable seed role name resolved by Q-S21-9 (fixed constant).
- **Q-S21-7 Last effective Admin:** the server refuses any operation that would leave no
  effective Admin — removing a privileged role from, deactivating (D-32) or soft-deleting (D-53)
  the last effective Admin, including the acting user themselves. The check and the write must be
  atomic so two concurrent operations cannot each pass the check and together remove the last
  two Admins (mechanism proposed at implementation, within the D-14 transaction rules).
  "Effective Admin" = holds the `Admin` role and `Status = Active`, even while temporarily locked
  out (Q-S21-11).
- **Q-S21-8 Scope and phase:** the assignment/removal restriction of Q-S21-4 applies to **all
  privileged roles** (Q-S21-5): only an `Admin` may assign or remove a privileged role. Built in
  **Gate 3 (0.9)** together with the `UsersController` role-list refactor.
**Approved details Session 21 (2026-10-14, DECISION_LOG §61)** — implementation still not approved:
- **Q-S21-9 Fixed Admin identity (Option A):** the Admin role name is a **fixed constant**
  (`"Admin"`) used by the seeder and every Admin-specific check. `Seed:Admin:Role`
  (`appsettings.json`, `SeedAdminOptions.Role`) is either removed, or kept only with startup
  validation that **fails** when its value differs from the constant. A mismatch is never
  silently allowed. Which of the two: Q-S21-13 (**removed**).
- **Q-S21-10 Interim classification (Option A):** until the member-permission allowlist is
  defined, **every role with one or more permissions is privileged** (Q-S21-5). In Gate 3 a
  non-Admin `Users.Create`/`Users.Edit` holder can therefore assign or remove only roles without
  permissions.
- **Q-S21-11 Lockout (Option B):** an `Admin` with `Status = Active` **counts as an effective
  Admin even during a temporary authentication lockout** (D-31); lockout state is not part of
  the Q-S21-7 count.
- **Q-S21-12 `RoleService` enforcement (Option A):** in **Gate 3 (0.9)**, `RoleService` enforces
  server-side the same rules so role management cannot bypass them: only an `Admin` may change
  a role's permissions so that it becomes privileged (privilege escalation through permission
  changes) or edit/delete a privileged role (Q-S21-8 scope); the **`Admin` role cannot be
  deleted**; and no permission change may undermine the last-effective-Admin protection
  (Q-S21-7). Existing authorization checks (`[HasPermission(Roles.*)]`, D-37, D-31) stay.
  Exact boundaries: Q-S21-14, Q-S21-15 (answered below).
**Approved details Session 21 (2026-10-14, DECISION_LOG §62)** — implementation still not approved:
- **Q-S21-13 Seed setting:** `Seed:Admin:Role` is **removed** (`appsettings.json` and
  `SeedAdminOptions.Role`); the seeder uses the fixed `"Admin"` constant (Q-S21-9). No startup
  validation is needed because no setting remains.
- **Q-S21-14 Admin role permissions:** the `Admin` role **always holds every permission**; any
  change that removes a permission from it is refused server-side in `RoleService` (as
  `DbSeeder` already restores missing ones at startup).
- **Q-S21-15 Role renames:** `RoleService` refuses server-side both renaming the `Admin` role and
  renaming any other role **to** `Admin`, so the fixed identity can be neither broken nor hijacked.
Open: none from Q-S21 (section 9). **Extended Session 22 by D-57** (operations on privileged users).

### D-57 Only an Admin may reset, re-email, deactivate or delete a privileged user — Approved (Session 22); implemented `56bb5f0`
**Source:** user, Session 22 (DECISION_LOG §65), after the Gate 3 code review found that a
non-Admin `Users.Edit` holder could reset an Admin's password and sign in as that Admin,
bypassing D-56. Extends D-56 (Q-S21-4/8); does not replace it.
1. **Non-Admin:** cannot reset the password of, change the email of, deactivate or soft-delete a
   user who holds a privileged role (D-56 Q-S21-5/10: any role with ≥ 1 live permission).
2. **Admin** (effective Admin, D-56 Q-S21-7): may do all of these; the last-effective-Admin
   protection still applies.
3. **Server-side:** enforced in `UserService` (`ErrorCodes.Forbidden`), not only by hiding UI.
4. **Other users** (no privileged role): current permission rules unchanged. Editing a
   privileged user's full name with unchanged email/status/roles stays allowed.
5. **Existing flows must keep working** (tested): sign-in, self-service password change
   (`/Account/ChangePassword`, not routed through `UserService`), Admin password reset.

### D-58 Rename "Member" (the borrower record) to "Library Member" — name and column names **Approved** (user, Session 23); not implemented
**Source:** user, Session 23 (DECISION_LOG §81). A pasted recommendation proposed "Borrower";
the user preferred **"Library Member"**. "Member" and "User" are easy to confuse:
`AspNetUsers` is the login account, `Members` is the library borrower record (0..1 link).
1. **Name (user's choice):** "Library Member" — entity `LibraryMember`, table `LibraryMembers`,
   UI label "Library Member(s)". "Borrower" is **Rejected**.
2. **Column / related names — Approved (user "ok", Session 23; proposed by the assistant):** FKs `MemberId` → `LibraryMemberId`
   (e.g. `LendingTransactions.LibraryMemberId`); keep `MemberCode` and `MemberType` (already
   unambiguous inside `LibraryMembers`, smaller change); `LibraryPolicies.MemberCategory` stays;
   `IdentityUserId` stays.
3. Order: (a) item 2 approved; (b) plan `docs/plans/D-58-library-member-rename.md` **Approved** (Q1–Q6 as recommended); (c) implementation in progress. Originally: (b) a separate, separately approved
   **impact analysis and migration plan**; (c) implementation.
4. **Known impact for (b)** (not yet analysed in full): 33 C# / Razor files reference the
   `Member*` names (entities, EF configurations, repositories, services, controllers, views,
   tests); FKs and indexes on `LendingTransactions`; the `Members.*` permission names (renaming
   them needs a decision on the existing permission rows and role grants); UI labels;
   `requirements.md` / `business-rules.md` / the ER diagram use "Member"; D-54 "Member link"
   wording; error code `MEMBER.INACTIVE` (archive BR-03).
5. Until approved and implemented, code and docs keep "Member".

---

## 9. Open decisions

**Session 21 (2026-10-14, DECISION_LOG §58–§62) — D-56 / staff role assignment.** Q-S21-1 … 15
answered (§59–§62, see D-56); none open:
| Item | Question | Recommendation |
|---|---|---|
| ~~Q-S21-13 `Seed:Admin:Role` (Q-S21-9)~~ **Answered Session 21 (§62): removed** | Remove the setting, or keep it with startup validation that fails on a mismatch? | Remove it (one less setting; a value that must always equal a constant has no purpose) |
| ~~Q-S21-14 Admin role permissions (Q-S21-12)~~ **Answered Session 21 (§62): `Admin` role always holds all permissions; removals refused** | Admin identity is the role **name** (Q-S21-6), so stripping permissions from the `Admin` role leaves an "effective Admin" who can do nothing. Which removals are refused: **any** permission (the `Admin` role always holds every permission, as `DbSeeder` already restores at startup), or only those needed to manage users/roles (`Users.*`, `Roles.*`)? | Any: the `Admin` role always holds all permissions; edits to its permission set are refused |
| ~~Q-S21-15 Renaming roles (Q-S21-9, Q-S21-12)~~ **Answered Session 21 (§62): both renames refused** | `RoleService.UpdateAsync` lets a `Roles.Edit` holder rename a role. Renaming `Admin`, or renaming another role **to** `Admin`, would break or hijack the fixed identity | Refuse both server-side (follows from Q-S21-9); confirm |
| ~~Q-S21-9 **Conflict:** literal `"Admin"` vs configurable seed role (Q-S21-6)~~ **Answered Session 21 (§61): Option A — fixed constant; setting removed or startup-validated (Q-S21-13)** | `appsettings.json` `Seed:Admin:Role` and `SeedAdminOptions.Role` let the seeded admin role be renamed. If it is set to anything but `"Admin"`, the seeded admin is not an Admin for the Q-S21-4/7/8 checks | Make `"Admin"` one Core constant used by the seeder and the checks, and remove `Seed:Admin:Role` (or fail at startup when it differs). Code/config change — needs your approval |
| ~~Q-S21-10 Interim effect of the empty allowlist (Q-S21-5, Q-S21-8)~~ **Answered Session 21 (§61): Option A — every role with ≥ 1 permission is privileged** | Until member permissions exist, every role with any permission is privileged, so in Gate 3 only an `Admin` may assign or remove **any** role that has permissions; a non-Admin `Users.Create`/`Users.Edit` holder can only assign permission-less roles | Accept as intended (safe default); confirm |
| ~~Q-S21-11 Locked-out Admin (Q-S21-7)~~ **Answered Session 21 (§61): Option B — `Active` Admin counts during temporary lockout** | Does an `Admin` who is temporarily locked out (D-31 lockout) still count as an effective Admin? | Yes — lockout is temporary; count `Admin` + `Active` only |
| ~~Q-S21-12 Role-side escalation paths (Q-S21-7, Q-S21-8)~~ **Answered Session 21 (§61): Option A — enforced in `RoleService`, Gate 3 (Q-S21-14, Q-S21-15)** | The rules cover user operations. A `Roles.Edit`/`Roles.Delete` holder can still (a) add staff permissions to a role they or others already hold, making it privileged, or (b) delete/strip the `Admin` role, leaving no effective Admin | Apply the same rules in `RoleService`: only an `Admin` may edit/delete a privileged role or make a role privileged; the `Admin` role cannot be deleted or lose permissions. Gate 3 (0.9) — needs your approval |
| ~~Q-S21-5 "Privileged role" definition (Q-S21-1)~~ **Answered Session 21 (§60): permissions outside the member allowlist (allowlist defined with the member portal)** | Which roles count as privileged when validating registration defaults? Only the `Admin` role, or any role that holds a staff permission (`Users.*`, `Roles.*`, `GeneralConfig.*`, `Registrations.Review`, …)? | Any role holding a permission outside an explicit member-permission list; the list is defined with the member portal (no member permissions exist yet) |
| ~~Q-S21-6 Identifying `Admin` (Q-S21-4)~~ **Answered Session 21 (§60): literal `"Admin"` (conflict: Q-S21-9)** | The admin role name comes from configuration (`Seed:Admin:Role`, default `"Admin"`, `DbSeeder`), not a constant | Use the configured seed role name in both the seeder and `UserService` |
| ~~Q-S21-7 Last Admin (Q-S21-4)~~ **Answered Session 21 (§60): protected server-side against privileged-role removal, deactivation, soft delete; concurrency-safe** | May an `Admin` remove the `Admin` role from the last remaining `Admin` (incl. themselves), or deactivate/delete that user? | Refuse it server-side (avoids locking everyone out); needs your approval as a new rule |
| ~~Q-S21-8 Scope and timing (Q-S21-4)~~ **Answered Session 21 (§60): all privileged roles; Gate 3 (0.9) with the role-list refactor** | Restriction covers the `Admin` role only, or every privileged role (Q-S21-5)? Built in Gate 3 (0.9, with the role-list refactor) or later? | `Admin` only, as decided; Gate 3 0.9 together with the `UsersController` role list |
| ~~Q-S21-1 Fellow member role (D-56)~~ **Answered Session 21 (§59): separate configured non-privileged roles, validated** | Q-S19-17 configures a default member role for **Students**. On approval, does a Fellow get the same configured role, or a separately configured Fellow role? | A separate System Admin setting per registration type (Fellows may later get different limits); both non-privileged |
| ~~Q-S21-2 Store the type (D-56)~~ **Answered Session 21 (§59): on the registration application record; classification only** | Is `RegistrationType` stored? Where? | Yes, on the registration application record (Q-S19-8), so review/re-apply know the workflow; never copied into roles or claims |
| ~~Q-S21-3 Form control (D-54/D-56)~~ **Answered Session 21 (§59): dropdown from the enum, server-validated** | D-54 says **radio** `Student` / `Dept. Fellow`; Session 21 text says **dropdown** | Radio (two options, both visible); either way the values come from the enum |
| ~~Q-S21-4 Privileged roles on the staff Users page (0.9)~~ **Answered Session 21 (§59): option (b), enforced in `UserService`** | `UsersController` Create/Edit lists **all** DB roles, so any holder of `Users.Create`/`Users.Edit` can assign `Admin` to anyone (incl. themselves via Edit) | Decide before the Gate 3 report: (a) document `Users.Create`/`Users.Edit` as Admin-only grants, or (b) only users who are `Admin` may assign/remove the `Admin` role (server-side check in `UserService`). Recommend (b) |

**Session 14 (2026-10-13, DECISION_LOG §30–§35) — open now, ask at the start of Session 15:**
| Item | Question | Recommendation |
|---|---|---|
| ~~Gate 1 findings (§35)~~ **Resolved Session 15 (§37, `8504115`)** | Which of the 10 `/code-review` + 6 `/ponytail-review` findings to fix | One correction commit on `feature/phase-0-gate-1`: code-review 2–5, 8–10 + all ponytail items (incl. NU1510 `Configuration.Abstractions`, direct `Serilog.Sinks.File`); `Program.cs` catch-all → 0.10; stale test-DB sweep optional; Dockerfile restore layer → later Docker work |
| `EF Tools` package | Keep `Microsoft.EntityFrameworkCore.Tools` (only for Visual Studio PMC)? | Remove if nobody uses PMC; CLI uses local `dotnet-ef`. **Session 15: kept** (not removed in `8504115`) until the user confirms nobody uses Visual Studio PMC |
| UI brand text | `_Layout.cshtml` still shows "DefaultTemplate" (title, navbar, footer) | Change with the first UI work (visible change, not part of the 0.3 rename) |
| ~~Old `DefaultTemplate/` folder~~ **Resolved Session 17 (DECISION_LOG §43): deleted** | Holds only ignored `bin/`, `obj/`, `*.csproj.user` | Delete (needs approval; nothing tracked) |
| ~~Gate 2 start~~ **Resolved Session 17 (§44): started; 0.6–0.8 committed (`51e189a`, `48a9acf`, `0d01e4e`)** | Begin steps 0.6–0.8 (incl. D-50, D-51) on `feature/phase-0-gate-2` | — |
| ~~Gate 2 review findings (§47)~~ **Resolved Session 18 (`8cd5f53`, §48)** | Which of CR1–CR9 / P1–P3 to fix before the Gate 2 report | One correction commit: CR1, CR3, CR4, CR5, CR7, CR8, CR9, CR2 permission-lookup filter, P1–P3; CR2 cookie part → 0.9; CR6 keep |
| ~~Gate 2 report~~ **Resolved Session 18: approved, merged `798ccbd` (§49)** | After the correction commit: D-14 per-use-case duplicate-operation assessment, docs, report; then approval + merge → `develop` (separate approvals) | — |
| ~~Session 22 start — Gate 3 (0.9) implementation plan (§63)~~ **Done Session 22 (§64): plan approved, implemented `b1bba18`; lock = `sp_getapplock`, codes `SECURITY.FORBIDDEN` / `SECURITY.LAST_ADMIN`** | Propose one plan for D-56 Q-S21-4 … 15 + the `UsersController` role-list refactor (`IRoleService`), with tests and mutation checks; no code before the plan is approved | Plan → "ok" → code → build/test/mutation → diff → commit "ok" (separate approvals). Then Gate 3 reviews, 0.10–0.12 |
| **Session 23 start — 0.12 smoke, then Gate 3 report (DECISION_LOG §74)** | 0.12 runs on a **disposable `CseLibrary_Test_*` LocalDB database** (user chose option (খ)); env-var connection string + seed password only; drop it afterwards; never create or touch `.\SQLEXPRESS` / `CseLibrary` | Smoke → record results → Gate 3 report → approval → merge into `develop` `--no-ff` (separate approvals) |
| ~~Session 21 start — D-32 execution approval (§56)~~ **Resolved Session 21 (§57): approved, implemented `c63854b`** | Implement D-32: custom `SignInManager.CanSignInAsync` (only `Active`; check moved out of `AccountController`), `UpdateSecurityStampAsync` when `UpdateAsync` turns a user `Inactive`, test A4 (stamp changes, old cookie denied on permission page and — with a test-only interval 0 — on a login-only page, re-login refused) | Done (65/65, mutation-checked). Next: `UsersController` role list, Gate 3 `/code-review` + `/ponytail-review`, 0.10 |
| ~~Session 19 work review (§53, §54)~~ **Session 20 (§55): Q-S19-2…17 answered; still separate approvals: D-37 commit, D-37 cleanup (target DB → read-only count → rows + rollback → delete), D-53 code, D-31 stamp/cookie/antiforgery values** | Review the uncommitted D-37 fix + docs; then separately: commit, D-37 cleanup target DB + read-only count, D-53 implementation, open values below | Commit the D-37 fix first (tests 52/52) |
| ~~Gate 3 start~~ **Resolved Session 19 (§52): started on `feature/phase-0-gate-3`** | Begin steps 0.9–0.12 on `feature/phase-0-gate-3` from `develop` | Includes CR2 cookie part (D-32/D-37) and the `UsersController` role list (0.9) |
| ~~Q-S19-1 Registration scope (Session 19, §52)~~ **Design answered Session 19 (§53) → D-54**; details open as Q-S19-3 … Q-S19-9, Q-S19-17 | Public Registration is approved as an anonymous endpoint that grants no privileged role — but who may register (students/fellows only? institutional email?), what the account gets (no role / a member role), link to the member record, staff approval/activation, and the phase it is built in are not decided. No registration endpoint exists today | Decide before Registration is implemented; 0.9 only reserves it in the anonymous allowlist |
| ~~Q-S19-2 Infrastructure pages (Session 19, §52; see D-55)~~ **Answered Session 20 (§55) — see D-31 / D-53 / D-54 / D-55** | The approved access design names Login, Registration, `Home.Index`, `Home.Privacy` as public; the earlier allowlist also had `Account.AccessDenied`, `Home.Error`, status-code page (0.10) | Keep `Error` + status-code page anonymous (an anonymous visitor's error would otherwise redirect to Login); `AccessDenied` only as D-55 fallback (Q-S19-10). Needs the user's confirmation |
| ~~D-37 cleanup (Session 19, §53)~~ **Closed Session 20 (§55): target `CseLibrary` does not exist on `.\SQLEXPRESS` — nothing to delete** | Delete legacy rows `AspNetUserClaims.ClaimType = 'Permission'` (exact, case-sensitive); other claim types kept. Which database, row count (read-only `SELECT COUNT` needs approval to run), backup table + rollback | Report given §53; **nothing deleted** until the user approves that specific operation |
| ~~Q-S19-3 Registration fields / identity (D-54)~~ **Answered Session 20 (§55) — see D-31 / D-53 / D-54 / D-55** | Required fields per type (student ID, session, designation, phone, …) and how identity is verified (institutional email domain, email confirmation — `RequireConfirmedEmail` is `false` today, staff check) | Decide before Registration is built |
| ~~Q-S19-4 Fellow activation (D-54)~~ **Answered Session 20 (§55) — see D-31 / D-53 / D-54 / D-55** | Does Librarian approval make the account `Active` directly, or is a further activation step needed (e.g. email confirmation, admin role assignment)? | Decide with Q-S19-8 |
| ~~Q-S19-5 Member link (D-54)~~ **Answered Session 20 (§55) — see D-31 / D-53 / D-54 / D-55** | How a registered Student / approved Fellow is linked to a `Member` record (created at registration, at approval, or linked to an existing staff-entered member; matching rule) | Before the Members phase |
| ~~Q-S19-6 Approvers (D-54)~~ **Answered Session 20 (§55) — see D-31 / D-53 / D-54 / D-55** | Which Librarian role(s)/permission approve or reject Fellow applications | A dedicated permission (e.g. `Registrations.Review`) assigned by the System Admin — not approved |
| ~~Q-S19-7 Resubmission (D-54)~~ **Answered Session 20 (§55) — see D-31 / D-53 / D-54 / D-55** | Can a rejected applicant resubmit? Same account or new application; any waiting period | — |
| ~~Q-S19-8 Approval-state model (D-54)~~ **Answered Session 20 (§55) — see D-31 / D-53 / D-54 / D-55** | Represent registration approval (`Pending`/`Approved`/`Rejected`) separately from account status `RecordStatus` — field on the user vs separate application record | Separate from `RecordStatus`; exact schema needs a design + approval |
| ~~Q-S19-9 Registration phase + tests (D-54)~~ **Answered Session 20 (§55) — see D-31 / D-53 / D-54 / D-55** | Which phase implements Registration and its tests | Not Phase 0 (0.9 only allowlists the endpoint when it exists) |
| ~~Q-S19-10 Forbidden page without a "current page" (D-55)~~ **Answered Session 20 (§55) — see D-31 / D-53 / D-54 / D-55** | A forbidden URL opened directly (no page to return to): keep `AccessDenied` as fallback, or redirect to `Home.Index`/referrer with the red toast? Also AJAX: 401/403 status instead of a redirect | Redirect to a safe local page (referrer if local, else Home) with the toast; AJAX gets 401/403 |
| ~~Q-S19-11 Reference ID in error toast (D-55, D-34)~~ **Answered Session 20 (§55) — see D-31 / D-53 / D-54 / D-55** | Should the red toast for an unexpected error show the Reference ID? | Yes (D-34 shows only the Reference ID) |
| ~~Q-S19-12 Toast values (D-55)~~ **Answered Session 20 (§55) — see D-31 / D-53 / D-54 / D-55** | Warning/info durations; maximum simultaneous toasts; pause on hover/focus | Warning ≈ 5 s, info ≈ 3 s, max 3, pause on hover/focus (WCAG 2.2.1) |
| ~~Q-S19-13 Toast implementation (D-55)~~ **Answered Session 20 (§55) — see D-31 / D-53 / D-54 / D-55** | Bootstrap's built-in toast component (already shipped) + `TempData` vs a custom script | Bootstrap toast + existing `TempData["Success"/"Error"]` keys |
| ~~Q-S19-14 Toast timing in the plan (D-55)~~ **Answered Session 20 (§55) — see D-31 / D-53 / D-54 / D-55** | Built in Phase 0 (0.9 permission response, 0.10 error handling) or Phase 1 (first UI work) | 0.10, with test A2 updated |
| ~~Q-S19-15 Deleted users in the Users list (D-53)~~ **Answered Session 20 (§55) — see D-31 / D-53 / D-54 / D-55** | Hide `Deleted` users, or show them with their status (filter) | Hide by default |
| ~~Q-S19-16 Email of a deleted user (D-52, D-53)~~ **Answered Session 20 (§55) — see D-31 / D-53 / D-54 / D-55** | Unique email index still covers soft-deleted users → the same email cannot register again. Keep (email belongs to the retained record) or filter the index by `Status <> Deleted` | Keep for now |
| ~~Q-S19-17 Student default access (D-54)~~ **Answered Session 20 (§55) — see D-31 / D-53 / D-54 / D-55** | Does a newly registered Student get a default role (configured by the System Admin) or no role until assigned? | A configurable default member role with no privileged permissions |
| CR5 cascade part (§48) | Soft delete of a principal still hard-deletes tracked non-`BaseEntity` dependents | Decide when the first such relation is built (Phase 2/3); no current code path |
| D-49 O1–O5 | Available count, `Issued` in `CopyStatus`, member subtypes, history name, loan id type (`database.md` §6) | Before the phase that builds the tables (Phase 2/3); recommendations in `database.md` |
| ~~Merged branch~~ **Resolved Session 18 (§50): `gate-1` and `gate-2` deleted with `-d`** | Delete `feature/phase-0-gate-1` (merged into `develop`)? | Any time; needs the user's OK (Session 17: asked together with the Gate 2 start; the bare "ok" counted for Gate 2 only) |
| ~~Gate 1 approval~~ **Resolved Session 16 (`bbee8dc`, DECISION_LOG §39)** | Merge `feature/phase-0-gate-1` → `develop` (`--no-ff`) | After the correction commit — **correction done Session 15 (`8504115`); ask at the start of Session 16** |
| M15 (§33) | Overall-architecture + DB/ER `archify` diagrams; multi-copy + lending-history DB design | Before Gate 2 (user: after the Gate 1 report); design stays **Proposed** until approved |
| M15 design point | Title "available count": stored counter vs derived from copy status | Derived (COUNT of copies with status Available) + filtered unique index "one active loan per copy" (D-35); show both with trade-offs |

**Resolved in Session 13 (DECISION_LOG §29) — the rows below are kept as history:** D-07 (v7
Approved), D-07 T1 scope (`.gitattributes` + hash check), D-07 T1-EOL (LF), D-07 T4 (replaced by
R1 recovery source), step 0.1 (authorized), D-47 / Q-M9-9 (`CseLibrary.*`), D-41 tooling, D-40
connection string, Q-M9-8 (keep offset paging), Q-M9-10 (all pages authenticated except Login,
AccessDenied, Error — **Superseded Session 19 (§52): `Home.Index`, `Home.Privacy`, Registration
public; see D-31 access design**), Q-M9-11 (yes), D-14 `ReadCommitted` (deferred to Phase 3), D-16 (Approved).
**Still open, decided by review at their gate (no discussion round needed):** D-13 signatures,
D-15/D-34 types (0.7), D-14 per-use-case assessment (0.8), D-31 values + D-32 (0.9), D-37 claims
cleanup (separate data approval), D-21/24/25/27 details + D-29/D-30 (0.11). Lending-phase items
(D-48, Q-M10-*) unchanged.

**Required now (before step 0.1):** the documentation package was approved 2026-10-13 (§18);
still needed: the D-07 decision (rows "D-07 (M14, v6)", "D-07 T1 scope", "D-07 T4" below) and a
**separate explicit approval of step 0.1**; "D-07 T1-EOL" is needed before step 0.2. Other rows are asked only before the step in their
"Blocks" column. _(Corrected Session 6: this line previously still said "only the approval of the
documentation package", which was stale after §18.)_

### Needed for Phase 0 (ask before the affected step)
| ID | Decision | Blocks |
|---|---|---|
| D-07 (M14, v6.1) | Git branching workflow — **approval (B)**: the v6.1 wording (applied Session 12 via approval (A): Variant A + §25 G1, G2, M1, M9; DECISION_LOG §28), the D-06 partial-supersession wording and the documentation impact list (DECISION_LOG §21; §21 item 4 `CLAUDE.md` "byte-for-byte" line to be worded consistently with F1); decisions T1–T5; received-files copy R1/R2/R3 (before 0.1; R1-like copy exists, §26). Then (E) step 0.1 start, (F) staged tree hash + message | 0.1 |
| D-07 T1 scope | `.gitattributes` (`* -text` + `text` for `/.gitignore`, `/LICENSE`) added to step 0.1's scope — needed so received files are stored byte-for-byte (`core.autocrlf=true` here); inventory generation (`I_before`, `L_disk`, `L_excluded`, `L_staged`, `D_step`), pre-commit checks B1–B6, post-commit checks C1–C4 (Proposed). Blocks 0.1 even if D-07 is rejected | 0.1 |
| D-07 T1-EOL | Line endings of new authored text files: LF — **Pending**, separate from D-07; approving D-07 does not confirm it (`.gitattributes` bytes are approved with step 0.1's tree only) | 0.2 |
| D-07 T4 | `<backup-root>` = `H:/Internship Project/DLMS-Backups/` with write authorization — **named by the user (approval (D), Session 12, DECISION_LOG §28)**; nothing written there until step 0.1 is approved. Verified = (a) integrity + (b) patch-application + (c) bundle-based reconstruction (Proposed, part of D-07). Independent received-files backup at `received_20261013-152404/` (verified, §26). T4 level (c) not yet exercised | 0.1 |
| D-07 (later) | Remote pushes / GitHub PRs; `main` protection (owner setting) — deferred by the user | first push |
| D-14 (M11) | `ExecuteInTransactionAsync` begins transactions with an explicit `ReadCommitted` level (needed by D-48 §4; Proposed) | 0.8 — optional; if not decided then, added in Phase 3 |
| D-47 | Project name prefix `CseLibrary.*` (D-41, Approved) vs `LibraryManagement.*` (M9) — Q-M9-9 | 0.3 |
| D-41 | `global.json` + local `dotnet-ef` tool manifest (not covered by the layout approval) | 0.3 |
| D-40 | Where the dev connection string lives (proposed: `appsettings.Development.json` → `CseLibrary`; seed admin password in user-secrets) | 0.4 |
| D-13 | Exact repository signatures (contract approved; review at Gate 2) | 0.7 |
| D-14 | Per-use-case ambiguous-commit assessment: risk, mechanism, tests, limitations (approach approved M8 §3; assessment presented at Gate 2) | 0.8 |
| D-15 | Proposed details: `PageRequest`/`PagedResult<T>`, page size max 100, bounded-table exception | 0.7 |
| D-34 | `Result`/`Result<T>` + `ErrorCode` type | 0.7 |
| D-31 | Default security values: ~~password, lockout~~ (**Approved Session 19**, §52); stamp interval, cookie, global antiforgery still Proposed | 0.9 |
| ~~D-31 (M9)~~ | **Approved Session 19 (§52):** controller-level `[Authorize]` + `FallbackPolicy`; `Home.Index`/`Privacy`/Registration public (Q-M9-10 superseded); open: Q-S19-1, Q-S19-2 | 0.9 |
| ~~D-32~~ | **Approved Session 19 (§52):** deactivation mechanism (`CanSignInAsync` override + stamp update) | 0.9 |
| D-37 | Cleanup of existing permission rows in `AspNetUserClaims` (data change — separate approval; deletion criteria reported Session 19 §53, not run). Authorization fix implemented Session 19 (uncommitted) | 0.9 |
| D-53 | Soft delete of users: implementation approval (findings + proposal Session 19 §53) | 0.9 (recommended, with D-32) |
| D-21/D-24/D-25/D-27 | Audit schema, security event list, bulk-write rule, consistency table (Proposed details) | 0.11 |
| D-29 | HiLo: isolated demonstration + migration/script diff, then approval | 0.11 |
| D-30 | Confirm no template-created DB holds data to keep; regenerate baseline migration (new DB only) | 0.11 |

### M9 questions for the user (Session 4)
| # | Question | Alternatives / assistant recommendation (not approved) | Blocks |
|---|---|---|---|
| Q-M9-1 | ~~Which outstanding loans block a new borrowing request?~~ **Superseded (M10):** both readings are independent configuration settings — `BlockIssueWhenAnyActiveLoan`, `BlockIssueWhenOverdue`, alongside `MaxActiveLoans` (D-48, M11); values → Q-M10-3 | ~~(a) only overdue · (b) any active loan · (c) self-service requests only — D-45~~ | — |
| Q-M9-2 | ~~Is "borrowing request" a new member-submitted workflow or the staff issue attempt?~~ **Answered (M10 item 3): (i)** — staff issue, no request workflow (BR-M10-09) | ~~(i) staff issue · (ii) new `BorrowRequest` workflow — D-45~~ | — |
| Q-M9-3 | How do lost and cancelled/voided loans count toward the limit (pending requests: none exist, M10)? Is the BR-M9-02 "active loan" definition correct? | BR-M9-02..05 | Lending phase |
| Q-M9-4 | ~~Limit per member category or one value? Table or `appsettings`? Initial 3 for both?~~ **Answered in part (M10):** per category; storage → Q-M10-1; initial values → Q-M10-3 | ~~table per category — D-44~~ | — |
| Q-M9-5 | Lowering the limit: existing loans unaffected (BR-M9-04 assumption)? Still open; used by BR-M10-07, test P8 | — | Lending phase |
| Q-M9-6 | Default/maximum page size 20/100? | D-15 M9 extension | Phase 1 |
| Q-M9-7 | Listing UX: Load More button, infinite scroll or numbered pages? | Load More *(recommended)* — D-46 | Phase 1 |
| Q-M9-8 | Keep the Users list on offset count-and-page (Approved D-15/D1) rather than keyset? | keep *(recommended)* | 0.8 |
| Q-M9-9 | Project prefix: keep `CseLibrary.*` (D-41) or switch to `LibraryManagement.*`? | — (both valid; structure identical) — D-47 | **0.3** |
| Q-M9-10 | Should `Home.Index` / `Home.Privacy` be public? **Answered Session 19 (§52): both public** (supersedes the Session 13 answer) | authenticated *(D-31 default)* | 0.9 |
| Q-M9-11 | Approve seeding `requirements.md` / `business-rules.md` now (before step 0.2)? | — | 0.2 |

### M10/M11 questions — borrowing policy (D-48): can wait until the Lending phase
**No answer is needed now.** None blocks Phase 0 or step 0.1; they are asked again at the start
of the Lending phase design (Phase 3). Recorded here only so they are not lost.
| # | Question | Alternatives / assistant recommendation (not approved) | Blocks |
|---|---|---|---|
| Q-M10-1 | Approve the D-48 architecture after review (§5 assessment): typed per-category table, pure evaluator, single issue entry point with member-row lock, no rule engine? | *recommended* · alternatives in D-48 §5 | Lending phase design |
| Q-M10-2 | v1 settings: `MaxActiveLoans`, `BlockIssueWhenAnyActiveLoan`, `BlockIssueWhenOverdue`, `LoanDurationDays`? Renewal settings deferred until a renewal workflow is approved (scope)? | these four + defer renewals *(recommended)* · add renewal settings now (stored but unused) | Lending phase design |
| Q-M10-3 | Initial values of every setting per category (no defaults chosen by the assistant; M9 mentioned limit 3) | — user/department decision (archive §14 items 2–4) | Lending migration seed |
| Q-M10-9 | Cross-setting constraints: none (screen shows effective behaviour) or reject `AnyActive = on` with `Max > 1`, and/or `Max = 0`? | none + effective-behaviour notice *(recommended)* — FR-M10-01 | Lending phase design |
| Q-M10-4 | Defaults/overrides: one policy per category, no global default, no per-member override, missing policy fails closed? | *recommended* · alt: global default + category overrides · per-member exceptions | Lending phase |
| Q-M10-5 | Policy change activation: edit in place, effective immediately for new issues, history in `AuditLogs`? | *recommended* · alt: future-dated `EffectiveFrom` versions | Lending phase |
| Q-M10-6 | Who may change policies: Admin only, or Admin and Librarian? **Conflict (M12):** M10 says "authorized Admin/Librarian"; the archive roles table says Admin only ("Manage staff accounts and policies: Librarian No") | Admin only *(recommended, matches the archive; Librarian sees values read-only)* | Lending phase |
| Q-M10-7 | May the librarian change the prefilled due date at issue, and within which bounds? Archive FR-06 step 5 lets the librarian set it; **any restriction amends FR-06** and needs explicit approval (M12) | keep FR-06 as is (free edit, BR-06 still applies) · allow earlier only · no edit — *no recommendation until the department's practice is known* | Lending phase |
| Q-M10-8 | Category change of a member: new category's policy applies from the next issue (BR-M10-12)? | *recommended* | Members/Lending phase |

### Can wait (not blocking Phase 0)
`IsActive` vs `IsDeleted` for library entities and nullable Category (Phase 1); Fellow
definition, borrowing-policy values and renewals (Phase 2–3, Q-M10-2/3); `LendingStatus.Lost`
(Phase 3); issue-confirmation email, overdue repeat frequency, editable email templates, SMTP
provider (Phase 4); portal account creation flow (Phase 5); Librarian audit access, audit UI
(Phase 6); hosting (Windows/IIS vs Docker), DB-level audit protection, backups, retention
(Phase 7); storing IP address in login events; data import; barcode; holiday rules.
