# Requirements

Authoritative list of functional requirements (FR). Business rules live in
`docs/business-rules.md`; design decisions in `docs/PROJECT_DECISIONS.md`; Phase 0 tests in
`docs/PHASE_0_PLAN.md`. Each fact has one owning file; other files link here.

**Status of this file:** seeded in Session 4 (M9) with the M9 requirements only; FR-M10-01 added
in Session 5 (M10, revised M11, M12). Step 0.2 (D-05) consolidates the archive requirements (SRS
v1.0 FR-01..FR-14, spec v1.1) into this file. Until then the archive FRs are **not** traced here
(known, planned gap).

**Traceability (interim — moves to `status.md` in step 0.2, D-05).** Status of every row:
Implemented no · Verified no.
| Requirement | Confirmed content | Business rules | Design | Acceptance tests | Phase |
|---|---|---|---|---|---|
| FR-M10-01 | per-category configuration by authorized staff, never hard-coded | BR-M10-01, -10 | D-48 (Proposed) | P2, P3, P10, P11, P15 | 3 Lending |
| FR-M10-01 | values configurable, logic not; no expressions | BR-M10-14 | D-48 §1 | P20 | 3 |
| FR-M10-01 | no default/initial value without approval | BR-M10-14 | D-48 §1 | review check at the Lending gate (approved values recorded in DECISION_LOG before the seed migration) — not automatable | 3 |
| FR-M10-01 | server-side enforcement (carried from FR-M9-02) | BR-M10-08 | D-48 §3 | P9, P10 | 3 |
| FR-M10-01 | concurrent issues never exceed a limit (M10 item 6) | BR-M10-11 | D-48 §4 | P12, P14, P17, P18, P19, P21 | 3 |
| FR-M10-01 | no reservation/request workflow; no rule engine | BR-M10-09 | D-48 | review check (absence) | — |
| FR-M10-01 | configurable limit; outstanding-loan and overdue restrictions (carried from FR-M9-01/02) | BR-M10-02, -03, -13, -05 | D-48 §2 | P1, P4, P5, P6, P8, P16 | 3 |
| FR-M9-03 | server-side paging, keyset preferred | — (design rule) | D-15 M9 ext. | J1–J8 | 1 and later |
| FR-M9-04 | jQuery incremental loading | — | D-46 | K1–K4 | 1 |
| FR-M9-05 | class-level authorization | — | D-31 M9 ext., D-37 | A1, A2, A4, A6–A8, A10, A11, B1, B2 | 0 (0.9–0.10) |
| FR-M9-06 | library-specific naming (intent; names Open) | — | D-41, D-47 | 0.3 build, E1, E4 | 0 (0.3, 0.8) |
Assumptions used by FR-M10-01 (not confirmed): BR-M9-02, BR-M9-03, BR-M9-04, BR-M10-12,
member status on the member row (D-48 §4), due-date/overdue time semantics (below).

Tags: **[Confirmed]** stated by the user · **[Assumption]** inferred, needs confirmation ·
**[Proposed]** assistant recommendation · **[Deferred]** later release · **[Open]** unresolved
(question listed in `PROJECT_DECISIONS.md` §9).

**ID note:** the archive SRS already uses `FR-01`..`FR-14` for different requirements. The M9
requirements are therefore numbered `FR-M9-01`..`FR-M9-06` (= M9 "FR-01".."FR-06") to avoid a
collision. Step 0.2 may renumber all FRs once; any renumbering keeps a mapping table.
Session 5 (M10) requirements use `FR-M10-nn`; their tests use the `P` series (borrowing policy).

**M10 note (Session 5):** FR-M9-01 and FR-M9-02 were recorded in M9 as two competing fixed
rules. M10 clarified that the requirement is one **configurable borrowing-policy system**; both
are **Superseded by FR-M10-01** and kept below unchanged for history (DECISION_LOG §14).

---

## FR-M10-01 Configurable borrowing policy by member category — [Confirmed] requirement; settings and details [Proposed]/[Open]
Supersedes FR-M9-01 and FR-M9-02. Design: D-48 (Proposed). Rules: `business-rules.md`
BR-M10-01..14. Revised in M11 (independent settings, values vs logic, no defaults) and M12
(archive reconciliation, traceability, test set), Session 5, 2026-10-13.
Status: **Implemented: no · Verified: no** (target: Lending phase, Phase 3).

**Requirement (user, M10):**
- Borrowing rules are **configuration, not code**: an authorized Admin/Librarian configures them,
  and no rule value is hard-coded in application logic. [Confirmed, M10; archive BR-05, FR-14]
- Rules are configured **per member category** (Student, Department Fellow), so the two
  categories can have different rules. [Confirmed, M10; archive BR-05, FR-14]
- The system is **extensible**: supported settings are explicit and isolated, so adding a rule is
  a localized change. [Confirmed intent, M10] Adding a new *kind* of rule still requires
  development work (code, usually a migration, tests, deployment) — see "What is configurable".
  [M11] It is **not** a generic rule engine (D-48). [Confirmed, M10 item 8]
- The M10 example list (limit, duration/renewals, outstanding-loan block, overdue block,
  per-category rules, activation/future changes) is **examples, not approved rules**. [Confirmed, M10]
- No setting gets a default or initial value without the user's approval. [Confirmed, M11]
- Eligibility is enforced **on the server**; client-side validation is only a convenience.
  [Confirmed, M9 FR-M9-02 — carried forward explicitly in M12]

**What is configurable — values, not logic [Confirmed, M11]:**
- **Configurable (by authorized staff through the policy screen, no development work):** the
  *values* of the supported settings for each member category — numbers within validated
  ranges and on/off switches.
- **Not configurable (application code, reviewed and tested):** which settings exist, what each
  one means, how they combine (evaluation semantics below), what counts as an "active" or
  "overdue" loan, error codes and messages. Administrators cannot enter code, scripts, formulas,
  queries or free-form expressions; the screen only accepts typed values for the fixed settings.
- **Future changes and the work they need** (indicative; each still needs its own approval):

| Change | Admin screen only | Code change | DB migration | Scope approval |
|---|---|---|---|---|
| Change a value (e.g. limit 3 → 4, switch overdue blocking on) | **Yes** | No | No | No |
| New setting of an existing kind (e.g. `MaxRenewals` number, another on/off block) | No | Yes (evaluator + screen + tests) | Yes (column) | Yes, if it enables a new workflow |
| Change of meaning (e.g. lost loans stop counting as active, overdue grace days) | No | Yes | Maybe | Yes |
| New member category | No | Yes (category list) | Yes (seed a policy row) | Yes |
| Per-member overrides, future-dated policy versions | No | Yes | Yes (new table/columns) | Yes |
| New workflow-based rules (renewals, reservations, fines, per-book-category limits) | No | Yes (new feature) | Yes | Yes (outside D-02 today) |

**Reconciliation with the original archive (M12 — replaces the M10/M11 claim that BR-05/FR-14 were
"fully covered", which was inaccurate).** Nothing below is discarded; archive items stay in force
with their archive status (SRS: "proposed, pending department approval"; spec v1.1 BRs Proposed,
D-02) until consolidated in step 0.2 or explicitly changed by the user.
| Archive item | What it says | Relation to FR-M10-01 / D-48 |
|---|---|---|
| BR-05 (spec v1.1) | Lending duration and maximum borrowing limit configurable **per member category** | Covered: `LoanDurationDays`, `MaxActiveLoans` per category |
| SRS §4 rule 11 | Borrowing limits and due-date policies must be configurable | Covered (same settings) |
| `LibraryPolicies` table (spec v1.1 §4.3: "lending duration and borrowing limits by member type"; SRS §5.6 table list) and SRS §8 page `/Admin/LibraryPolicies` | Per-member-type policy table and admin page | **Kept**: D-48 now uses this name and route (M12; the M10/M11 rename to `BorrowingPolicies` is withdrawn). New columns (`BlockIssueWhen…`) extend it — Proposed |
| FR-14 "maximum active books per member category", "lending duration by member category" | Admin-configurable | Covered |
| FR-14 "whether renewals are permitted" | Admin-configurable | **Not covered in v1 — deferred, not discarded:** renewals are an open department decision (archive §14 item 4) and not in D-02's scope; the setting is added with the renewal feature (Q-M10-2) |
| FR-14 "whether overdue fines are applicable" | Admin-configurable | **Not applicable in v1:** D-02 (Approved) excludes fines; kept for a later release |
| FR-14 reminder intervals, holiday/closure rules, email sender, institutional contacts | Admin-configurable | **Not borrowing rules**; remain FR-14 items for their phases (Phase 4 email, holiday rules "can wait") |
| FR-06 step 5 "Set the issue date and due date" (librarian) | Librarian sets the due date at issue | **Preserved:** the policy computes a **prefilled** due date (issue date + `LoanDurationDays`); the librarian still sets/confirms it. Whether the librarian may change it, and within which bounds, is Q-M10-7; any restriction would **amend FR-06** and needs explicit approval |
| Initial draft business-rules table (archive §4 before the SRS, line 123) "Due date: determined by the configurable lending policy" | Policy determines the due date | Consistent with the prefill; tension with FR-06 step 5 is resolved only by Q-M10-7 |
| BR-06 | Due date not before issue date | Kept (BR-M10-04) |
| BR-17 | Policy change does not alter existing due dates | Kept (BR-M10-07, P7) |
| BR-03, BR-04, BR-16 | Only active members borrow; one active loan per copy; concurrent issue of a copy → one succeeds | Kept (evaluator `MEMBER.INACTIVE`; D-35 copy protection) |
| Spec v1.1 roles table "Manage staff accounts and policies: Admin Yes, Librarian No" | Only Admin manages policies | **Conflict** with M10 wording "authorized Admin/Librarian" — not resolved here; Q-M10-6 |
- M9 value: "intended initial value 3" (FR-M9-01) is recorded as the user's M9 input only. It is
  **not** a selected default; initial values for every setting and category are chosen by the
  user/department before the Lending phase (Q-M10-3). [M11]

**Scope (M10 item 3):**
- **No member reservation / borrowing-request workflow** — it is outside the approved scope
  (D-02; archive SRS §1.3, §13 defer reservations). [Confirmed, M10] This answers Q-M9-2 as
  option (i).
- "Borrowing operation" = the **staff-operated issue** (archive FR-06). Two distinct uses of the
  policy (BR-M10-08):
  1. **Eligibility check** (read-only, advisory): shown on the staff Issue Book page (FR-06 step
     2 "view the member's eligibility") and, optionally, in the member portal. Changes nothing.
  2. **Enforcement** (authoritative): inside the issue transaction. Only this decides.

**Candidate policy settings for v1 — [Proposed] (Q-M10-2); no default values selected:**
| Setting | Meaning | Covers |
|---|---|---|
| `MaxActiveLoans` (int ≥ 0) | Maximum number of simultaneously active loans | FR-M9-01; archive BR-05 |
| `BlockIssueWhenAnyActiveLoan` (bool) | If on, a member with **any** active loan cannot be issued another | FR-M9-02 literal reading; M10/M11 |
| `BlockIssueWhenOverdue` (bool) | If on, a member with **any overdue** loan cannot be issued another | FR-M9-02 overdue reading; M10/M11 |
| `LoanDurationDays` (int ≥ 1) | Default due date = issue date + N days; stored on the loan | archive BR-05, FR-06 |
- **Independent settings [M11].** The three borrowing checks are separate settings with separate
  meanings and separate refusal reasons. *Correction of M10:* M10 said "any active loan blocks"
  equals `MaxActiveLoans = 1`. That holds only under today's assumptions (all checks count the
  same loan set, one copy per issue, no overrides) and is not a property the design relies on.
  They can diverge, e.g. if Q-M9-3 excludes some loans (lost, disputed) from the limit count but
  not from "outstanding", if a multi-copy issue is ever allowed, or simply in the reason shown to
  staff and members. The M9 conflict is still resolved: both readings of FR-M9-02 and the limit
  of FR-M9-01 coexist as configurable settings, with no value chosen.

**Evaluation semantics [Proposed] (BR-M10-05):**
```
reasons = []
if not member.IsActive                                  → add MEMBER.INACTIVE
if ActiveLoanCount  >= MaxActiveLoans                    → add LENDING.LIMIT_REACHED
if BlockIssueWhenAnyActiveLoan and ActiveLoanCount  > 0  → add LENDING.ACTIVE_LOAN_BLOCK
if BlockIssueWhenOverdue       and OverdueLoanCount > 0  → add LENDING.OVERDUE_BLOCK
eligible = reasons is empty        (copy availability BR-04 is checked separately at issue)
```
- `ActiveLoanCount` = loans issued and not returned (BR-M9-02); `OverdueLoanCount` = those whose
  due date is before `now` (archive SRS §4 rule 9). Overdue loans are a subset of active loans.
  [Open, Lending phase] Whether the due date is a calendar date (overdue from the next day) or
  a timestamp, and which time zone defines "today". The archive is itself inconsistent: SRS data
  dictionary "DueDate: due date/time"; spec v1.1 §7.1 proposes timestamps in UTC and the due
  date as a library-local calendar date (`Asia/Dhaka`). Neither is approved (M12).
  Codes are placeholders until Phase 3.
- Every check is evaluated and **all** failing reasons are returned (AND semantics): no check
  overrides another, so no precedence order exists.
- Consequences of AND semantics (documented, not constraints): with `BlockIssueWhenAnyActiveLoan`
  on, `BlockIssueWhenOverdue` adds a reason but never changes the outcome, and `MaxActiveLoans`
  values above 1 have no further effect; `MaxActiveLoans = 0` blocks every issue for that
  category.
- **Cross-setting constraints — [Proposed], Q-M10-9:** recommendation is **no hard constraint**:
  every combination has a defined outcome, so rejecting one would only remove a choice. Instead
  the policy screen shows the *effective* behaviour (e.g. "any active loan blocks → at most one
  loan at a time"; "limit 0 → this category cannot borrow") and asks for confirmation.
  Alternative: reject `BlockIssueWhenAnyActiveLoan = on` with `MaxActiveLoans > 1` as
  misleading, and/or reject `MaxActiveLoans = 0`.
- **Deferred, not in v1:** renewal settings (renewal is a workflow not in the approved scope;
  archive lists `BookRenewals` as optional future — Q-M10-2), fines (D-02 excludes them),
  future-dated policy activation (Q-M10-5), per-member overrides (Q-M10-4).

**Acceptance criteria (Lending phase; real SQL on LocalDB `CseLibrary_Test_*`, `TimeProvider`
for time).** Numbers and switches below are **test fixtures**, not proposed defaults. None of
these tests exists yet; concurrency protection stays **Proposed, not Verified**, until P12, P14,
P17, P18, P19 and P21 pass on LocalDB `CseLibrary_Test_*` (M12: one test set used everywhere).
| ID | Test |
|---|---|
| P1 | Configurable limit: Student policy `MaxActiveLoans = 3`; member with 2 active loans is issued one more; at 3 the issue is refused with a business error (`LENDING.LIMIT_REACHED`, final code at Phase 3); no loan row, copy unchanged |
| P2 | Category-specific: Student limit 2, Fellow limit 5 → each member is checked against **their own** category's policy; the due date uses their category's `LoanDurationDays` |
| P3 | Policy change without code change: admin changes `MaxActiveLoans` 3 → 4 through the policy service; the next issue for a member at 3 succeeds (no restart, no deploy) |
| P4 | Any-active-loan block (M11): `MaxActiveLoans = 3`, `BlockIssueWhenAnyActiveLoan = on` → member with one unreturned, not overdue loan is refused with `LENDING.ACTIVE_LOAN_BLOCK` (not `LIMIT_REACHED`); with the switch off → allowed; after the return is recorded → allowed |
| P5 | Overdue restriction: `BlockIssueWhenOverdue = true`, member below the limit with one overdue loan → refused (`LENDING.OVERDUE_BLOCK`); with `false` → allowed; after that loan is returned → allowed. Overdue is evaluated with `TimeProvider` (due date passed, no return) |
| P6 | Returned loans never count, including a copy returned damaged (BR-M9-03) |
| P7 | Due-date snapshot: changing `LoanDurationDays` changes only new loans; existing loans keep their due date (BR-17) |
| P8 | Lowering the limit below a member's current count: existing loans untouched; new issues refused until the count drops below the new limit (BR-M9-04, Q-M9-5) |
| P9 | Eligibility check vs enforcement: the read-only eligibility result uses the same evaluator and reports all failing reasons; if state changes after the check (another loan issued), the issue itself still refuses |
| P10 | Server-side and authorization: a direct POST that bypasses the UI is still evaluated and refused; a user without the policy-management permission gets 403 on the policy edit endpoints; an unauthorized edit changes nothing |
| P11 | Validation and fail-closed: negative limit / duration 0 rejected by the service; a member whose category has **no** policy row is refused with a configuration error — never treated as unlimited |
| P12 | Concurrency, same member: N parallel issues of **different** copies for a member at `limit − 1` → exactly one succeeds, the rest get `LENDING.LIMIT_REACHED`; active count never exceeds the limit |
| P13 | Concurrency, policy edits: two admins edit the same policy from the same version → one succeeds, the other gets a concurrency conflict (`RowVersion`), no lost update |
| P14 | Rollback/retry: a failed or rolled-back issue leaves no loan and no copy change and does not count; a transient-retried issue re-reads policy and count inside the retried delegate (D-14 rule 1) |
| P15 | History: every policy change writes an `AuditLogs` entry with actor, time, old and new values (D-21); no policy row is ever hard-deleted |
| P16 | Combination matrix (unit tests on the pure evaluator, every row; a representative subset also as integration tests through `IssueAsync`) — see table below |
| P17 | Lock coverage: every code path that creates a loan goes through the single issue entry point that takes the member lock — (a) loan creation (`AddAsync` on the lending-transaction repository) is called only from `LendingService.IssueAsync`; (b) the lock method is called before the policy/count reads (unit test with a recording fake UoW checks the call order). M12: (a) needs a method-level reference check; the mechanism (IL inspection, a Roslyn-based test, or code review as fallback) is chosen in Phase 3, and any new test package needs separate approval. Until (a) exists, coverage rests on review |
| P18 | Lock behaviour (deterministic, LocalDB): transaction A takes the member lock and pauses; issue B for the same member waits until A commits, then sees A's loan and is refused at the limit; issue C for a **different** member is not blocked by A. Run once with `READ_COMMITTED_SNAPSHOT` OFF and once ON on the test database |
| P19 | Category change / deactivation vs issue: a member's category change, or member deactivation, runs concurrently with an issue for that member → they serialize on the member row; the issue is evaluated entirely under the old or entirely under the new state (category policy, `IsActive`), never mixed. Precondition: both values live on the member row (assumption, D-48 §4) |
| P20 | Configurable values only: the policy endpoints accept only the typed fields; unknown fields, out-of-range values and non-numeric input are rejected; no input is evaluated as code or expression |
| P21 | Retry, lock waits and safe failure (M12, refined M13): (a) deadlock victim (SQL 1205, forced): the victim attempt is rolled back — no loan, copy change or outbox row from it; the strategy retries; the retry re-takes the member lock and re-reads policy and counts; final outcome is exactly one committed loan or a business refusal from re-evaluation — never two loans; (b) lock wait > command timeout: transaction rolled back, member lock released (a following issue for the same member proceeds), no loan/copy/outbox change; reported as a technical failure with a Reference ID (D-34), not a business refusal; whether it is retried is recorded as observed; (c) retries exhausted: same safe failure as (b). Passing P21 covers only the tested scenarios. This is a test specification only; the behaviour it describes is Proposed (D-48 §4) and not Verified until the test exists and passes |

**P16 combination matrix** (outcome: ✓ allowed, or the refusal reasons; L = `LIMIT_REACHED`,
A = `ACTIVE_LOAN_BLOCK`, O = `OVERDUE_BLOCK`). Member states: **S0** no loans · **S1** 1 active,
not overdue · **S2** 1 active, overdue · **S3** 3 active, none overdue.
| Max | AnyActive | Overdue | S0 | S1 | S2 | S3 |
|---|---|---|---|---|---|---|
| 3 | off | off | ✓ | ✓ | ✓ | L |
| 3 | on | off | ✓ | A | A | L, A |
| 3 | off | on | ✓ | ✓ | O | L |
| 3 | on | on | ✓ | A | A, O | L, A |
| 1 | off | off | ✓ | L | L | L |
| 1 | on | on | ✓ | L, A | L, A, O | L, A |
| 0 | off | off | L | L | L | L |
Plus: inactive member → `MEMBER.INACTIVE` in addition to any other reason, in every row.

---

## FR-M9-01 Configurable borrowing limit — **Superseded by FR-M10-01 (M10)**
> Kept for history. M9 interpretation: one fixed configurable limit. Replaced by the
> per-category policy system FR-M10-01. Tests I1–I6 are superseded by P1–P3, P6, P8, P11, P12, P14.

_Original M9 text, unchanged — original status: [Confirmed] requirement; details [Open]:_
- The library supports a configurable maximum number of simultaneously **active loans** per
  member. [Confirmed, M9]
- Intended initial value: **3**. The value is configuration, never hard-coded in logic. [Confirmed, M9]
- Consistent with archive BR-05 / SRS FR-06 / FR-14 ("maximum borrowing limit … configurable").
- Rules and open cases: `business-rules.md` BR-M9-01..BR-M9-05. Design: D-44.
- [Open] Per member category (archive BR-05, FR-14) or one value for all members (M9 wording
  "per member")? Where the value is stored (admin-editable `LibraryPolicies` table per archive
  vs `appsettings`)?

**Acceptance criteria (target: Lending phase, Phase 3 — phase plan not yet written):**
| ID | Test |
|---|---|
| I1 | Member with `limit − 1` active loans can be issued one more; member at `limit` is refused with a business error (`LENDING.LIMIT_REACHED`, final code at Phase 3) |
| I2 | Limit is read from configuration: changing the configured value changes the outcome of I1 without code change |
| I3 | A returned loan no longer counts; the member becomes eligible again |
| I4 | Server-side enforcement: a direct POST bypassing the UI/client validation is still refused |
| I5 | Concurrency: N parallel issue/request attempts for one member at `limit − 1` → exactly one succeeds (real SQL, LocalDB `CseLibrary_Test_*`) |
| I6 | A failed/rolled-back issue transaction leaves no loan, no copy status change and does not count toward the limit; the next attempt evaluates correctly |

## FR-M9-02 Outstanding-loan restriction — **Superseded by FR-M10-01 (M10)**
> Kept for history. M9 read this as a fixed rule competing with FR-M9-01 and as a possible new
> request workflow. M10: it is configurable policy, and no request workflow is introduced.
> M11: expressed by the independent settings `BlockIssueWhenAnyActiveLoan` and
> `BlockIssueWhenOverdue` (M10's "= `MaxActiveLoans = 1`" equivalence is withdrawn). Tests
> I7–I10 are superseded by P4, P5, P9, P10, P12, P16.

_Original M9 text, unchanged — title "Outstanding-loan restriction on new borrowing requests";
original status: [Confirmed] requirement; meaning [Open]:_
- A member must not be able to submit a new borrowing request while they have an outstanding,
  unreturned loan, "according to the intended policy". [Confirmed, M9]
- Enforced on the **server**; client-side validation is only a convenience. [Confirmed, M9]
- **Conflict (not resolved by the assistant):** read literally ("any unreturned loan blocks a new
  request"), this caps every member at **one** active loan, which contradicts FR-M9-01 (limit 3).
  Alternatives are listed in D-45 and `PROJECT_DECISIONS.md` §9 (Q-M9-1).
- **Scope gap:** the approved scope (D-02) and the archive have **no member-submitted borrowing
  request**: staff issue books directly (SRS FR-06), members cannot self-serve lending, and
  online reservations are a future enhancement (SRS §1.3, §13). FR-M9-02 either introduces a new
  "borrowing request" workflow (scope change) or means the staff issue attempt. (Q-M9-2.)
- Distinct operations (definitions in `business-rules.md` BR-M9-06): submitting a request ·
  approving/issuing a loan · returning a book · concurrent requests.
- [Open] Is an **overdue** loan treated differently from an ordinary outstanding loan? (Q-M9-1)
- Rules: BR-M9-06..BR-M9-08. Design and concurrency assessment: D-45.

**Acceptance criteria (Phase 3 or the phase that introduces requests):**
| ID | Test |
|---|---|
| I7 | Member with a blocking outstanding loan (per the approved meaning) → request/issue refused server-side, incl. a direct POST |
| I8 | After the blocking loan is returned → request/issue allowed |
| I9 | Two concurrent requests from the same member that would each be valid alone but together violate the policy → at most one succeeds |
| I10 | Overdue vs non-overdue outstanding loan behave as the approved policy states (test written after Q-M9-1 is answered) |

## FR-M9-03 Efficient server-side pagination for listing pages — [Confirmed] requirement; mechanism [Proposed]
- All relevant Index pages use server-side pagination — especially Books, Students/Members and
  Department Fellows, plus other large lists (Lending, Audit, portal history). [Confirmed, M9]
- Filtering, sorting and paging run in the database; listing queries project only the displayed
  fields; deterministic ordering with a unique tie-breaker; no unnecessary `Include`, no
  over-fetching, no N+1. [Confirmed, M9; already Approved as D-15]
- Prefer **keyset (cursor)** pagination for large lists over loading everything or relying only on
  large-offset paging; justified exceptions documented. [Confirmed preference, M9]
- Configurable default and maximum page size. [Confirmed, M9; values Proposed in D-15]
- Indexes assessed from the actual filters/sorts; cursor limitations (no direct jump to page N)
  documented. Design, consistency model and exceptions: D-15 "M9 extension".

**Acceptance criteria (Phase 1 Books; Members/Fellows phase; Lending/Audit phases):**
| ID | Test |
|---|---|
| J1 | Stable order: two identical requests return identical pages; ties on the sort key are broken by `Id` |
| J2 | Walking all pages with the cursor returns every row exactly once (no duplicates, no gaps) on an unchanged dataset |
| J3 | Rows inserted/deleted between requests: no duplicates on the next page; behavior matches the documented consistency model |
| J4 | Empty result and final page: correct "no more data" signal; no extra request needed to detect the end (page size + 1 fetch) |
| J5 | Each supported sort × filter combination produces a correct cursor; a cursor from a different sort/filter is rejected (→ first page or validation error, never wrong data) |
| J6 | SQL-side paging: captured SQL contains `TOP`/`OFFSET…FETCH` and the keyset `WHERE`; command count constant for 50 vs 5,000 rows (no N+1); selected columns limited to the listing projection |
| J7 | Performance smoke with representative data (e.g. 50k Books / 10k Members in LocalDB): page query time recorded; execution plan uses the intended index (evidence, not a hard threshold until agreed) |
| J8 | Requested page size above the maximum is clamped (extends D2) |

## FR-M9-04 Dynamic loading of subsequent listing data with jQuery AJAX — [Confirmed] requirement; UX pattern [Open]
- Subsequent pages of a listing may be loaded via jQuery AJAX where appropriate. jQuery is
  already in the template (`wwwroot/lib/jquery`); no new frontend framework. [Confirmed, M9]
- Initial render and AJAX requests share the same server-side filtering, sorting, authorization
  and paging code path. [Confirmed, M9]
- Loading indicator, error handling, empty/end-of-data state, no duplicate requests or rows,
  preserved filters/sort, expired-session handling, accessible feedback, server authoritative.
  [Confirmed, M9]
- [Open] Load More button vs infinite scroll vs numbered pages (Q-M9-7). Recommended default:
  **Load More button** (D-46) — [Proposed], not approved.

**Acceptance criteria (with the first AJAX list, Phase 1):**
| ID | Test |
|---|---|
| K1 | AJAX next-page endpoint returns the same rows as the server-rendered path for the same cursor/filter/sort |
| K2 | AJAX request with an expired/absent auth cookie → 401 (no HTML login page injected into the table); without permission → 403 |
| K3 | Same endpoint enforces the same permission/ownership rules as the full page (e.g. a member cannot page through another member's history) |
| K4 | Manual/UI check: button disabled while loading (no double request), end-of-data state, error message with retry, `aria-live` announcement, filters/sort preserved |

## FR-M9-05 Explicit controller-level authentication and authorization — [Confirmed] requirement; approach [Proposed]
- Every controller declares its authentication/authorization requirement at class level, so no
  controller is unprotected by accident. [Confirmed, M9]
- Public endpoints are explicit `[AllowAnonymous]`; permission-based authorization (D-37)
  stays; authentication is never treated as permission. [Confirmed, M9; consistent with D-31]
- Exception/status-code endpoints expose no sensitive information (D-34).
- Code inventory, fallback-policy evaluation and proposed approach: D-31 "M9 extension".

**Acceptance criteria:** Phase 0 step 0.9 — A1, A2, A4, A6–A8, **A10, A11** (new) and B1/B2
(step 0.10). See `PHASE_0_PLAN.md`.

## FR-M9-06 Library-specific project and solution naming — [Confirmed] intent; names [Open]
- Rename the generic template naming (`Default.*`, `DefaultTemplate`) to clear library naming.
  [Confirmed intent, M9 — both M9 and the approved D-41 agree on renaming]
- M9 suggests `src/LibraryManagement.{Web,Services,Core,Data}` + `tests/LibraryManagement.Tests`
  as a **proposed direction**, not an authorization. This **conflicts** with the Approved D-41
  (`CseLibrary.*`). Not changed; see D-47 and Q-M9-9.
- Agreed dependency direction is unchanged (D-10).

**Acceptance criteria:** step 0.3 "done when" (build) + test **E4** (step 0.8) + E1.
