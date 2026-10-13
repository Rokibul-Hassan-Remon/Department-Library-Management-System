# Business Rules

Authoritative list of business rules (BR). Requirements: `docs/requirements.md`. Design and
enforcement mechanisms: `docs/PROJECT_DECISIONS.md`.

**Status of this file:** seeded in Session 4 (M9) with the borrowing-policy rules only;
borrowing rules revised in Session 5 (M10, M11, M12) — superseded rules are kept and marked. Step 0.2
(D-05) consolidates the archive rules (spec v1.1 BR-01..BR-18, SRS §4) here. The archive rules
remain **Proposed** (not department-approved) until then (D-02).

Tags as in `requirements.md`: [Confirmed] · [Assumption] · [Proposed] · [Open].
Open questions `Q-M9-n` and `Q-M10-n` are listed in `PROJECT_DECISIONS.md` §9.

---

## Borrowing policy (FR-M10-01, M10; revised M11, M12) — Implemented: no · Verified: no

Supersedes BR-M9-01 and BR-M9-05..08 (marked below). BR-M9-02..04 stay in force and are used
by these rules. Design: D-48. Questions `Q-M10-n`: `PROJECT_DECISIONS.md` §9.

| ID | Rule | Tag |
|---|---|---|
| BR-M10-01 | Borrowing rules are stored as **one policy per member category** (Student, Department Fellow) and edited by authorized staff; no rule value is hard-coded in logic. | [Confirmed] (M10; archive BR-05, FR-14) |
| BR-M10-02 | A member may hold at most their category's `MaxActiveLoans` active loans (active loan = BR-M9-02). *(M11: the M10 sentence "`MaxActiveLoans = 1` expresses 'an unreturned loan blocks another borrowing'" is withdrawn — see BR-M10-13.)* | Principle [Confirmed]; setting [Proposed]; values [Open] Q-M10-3 |
| BR-M10-03 | If the category's `BlockIssueWhenOverdue` is true, a member with at least one overdue unreturned loan cannot be issued another loan until it is returned. | [Proposed] — Q-M10-2, Q-M10-3 |
| BR-M10-04 | The default due date is issue date + the category's `LoanDurationDays` at the time of issue; it is stored on the loan and later policy changes never alter it (archive BR-17). Due date never before issue date (archive BR-06). The computed date is a **prefill**: archive FR-06 step 5 (librarian sets the due date) stays in force (M12). | [Proposed]; librarian change and bounds Q-M10-7 (a restriction would amend FR-06) |
| BR-M10-05 | **Conflict resolution:** a new issue is allowed only if **every** applicable rule passes (member active BR-03; limit BR-M10-02; any-active-loan block BR-M10-13; overdue BR-M10-03; copy available BR-04). Exact semantics and combination table: `requirements.md` FR-M10-01, test P16. Rules never override each other, so no priority order is needed; all failing reasons are reported together. | [Proposed] |
| BR-M10-06 | Exactly one policy per category. A category without a policy **fails closed** (issue refused with a configuration error, never "unlimited"). No global default and no per-member override in v1. | [Proposed] — Q-M10-4 |
| BR-M10-07 | A policy change applies to issue operations that read the policy after the change is committed; existing loans are never changed (BR-17, BR-M9-04). No future-dated activation in v1. | [Proposed] — Q-M10-5 |
| BR-M10-08 | **Eligibility check ≠ enforcement.** The eligibility check (Issue Book page, optional portal display) is read-only and advisory. The issue operation re-evaluates every rule inside its transaction and is the only authoritative decision. | [Proposed] |
| BR-M10-09 | No member reservation / borrowing-request workflow; borrowing happens only through the staff-operated issue (archive FR-06). | [Confirmed] (M10 item 3; D-02) |
| BR-M10-10 | Only users with the policy-management permission may change policies; every change is audited with old/new values; policy rows are never hard-deleted. | [Proposed] — who holds it: Q-M10-6 (archive roles table: Admin only; M10: Admin/Librarian — conflict) |
| BR-M10-11 | Concurrent issue operations for the same member can never together exceed any policy limit. | Principle [Confirmed] (M10 item 6); mechanism [Proposed] D-48 |
| BR-M10-12 | If a member's category changes, the new category's policy applies from the next issue; existing loans keep their due dates. | [Assumption] — Q-M10-8 |
| BR-M10-13 | If the category's `BlockIssueWhenAnyActiveLoan` is on, a member with at least one active loan (overdue or not) cannot be issued another loan until all are returned. Independent of `MaxActiveLoans` and `BlockIssueWhenOverdue`; each produces its own refusal reason. | [Proposed] (M11) — Q-M10-2, Q-M10-9 |
| BR-M10-14 | Authorized staff configure only the **values** of supported settings (typed, range-validated); the meaning and combination of settings is application logic. No arbitrary code or free-form expressions. No setting has a default or initial value until the user approves one. | [Confirmed] (M11) |

---

## Borrowing limit (FR-M9-01) — superseded in part by BR-M10 (M10)

| ID | Rule | Tag |
|---|---|---|
| BR-M9-01 | A member may hold at most `MaxActiveLoans` active loans at the same time. Initial intended value 3; the value is configuration. | **Superseded by BR-M10-02 (M10)** — was [Confirmed] (M9) |
| BR-M9-02 | **Active loan** = a lending transaction that has been issued and has no return recorded (`ReturnDate` null), whether or not it is overdue. Overdue is derived from due date + current time + return status (archive SRS §4 rule 9), not a separate "closed" state. | [Assumption] — confirm with Q-M9-3 |
| BR-M9-03 | Returned loans never count, including a copy returned **damaged** (the loan is closed; the copy's condition is a separate matter, archive BR-08). | [Assumption] |
| BR-M9-04 | Lowering the limit does not affect loans already issued; a member above the new limit simply cannot borrow until below it (by analogy with archive BR-17). | [Assumption] — Q-M9-5 |
| BR-M9-05 | Counting of exceptional cases is **not decided**: lost (no `Lost` status exists yet — D-02 open item), cancelled/voided issue, pending (unapproved) borrowing requests, per-category limits. | **Superseded in part (M10):** per-category limits → BR-M10-01; pending requests → none exist (BR-M10-09); lost and cancelled/voided counting still [Open] — Q-M9-3 |

## Outstanding-loan restriction (FR-M9-02) — superseded by BR-M10 (M10)

| ID | Rule | Tag |
|---|---|---|
| BR-M9-06 | Operations are distinct: **(a) submit a borrowing request** (member intent; no copy reserved, no loan) — exists only if Q-M9-2 introduces it; **(b) approve/issue** (staff creates the loan, copy → Issued; the authoritative point where all eligibility rules are re-checked); **(c) return** (staff closes the loan; only staff, archive §9 portal rules); **(d) concurrent requests** (two or more (a) or (b) operations for the same member/copy overlapping in time). | **Superseded by BR-M10-08, BR-M10-09 (M10)** — was [Proposed] |
| BR-M9-07 | Eligibility is evaluated **on the server** at submission (if requests exist) **and again at issue**; a request accepted earlier never bypasses the check at issue. | **Superseded by BR-M10-08 (M10)** — was [Proposed] |
| BR-M9-08 | Which outstanding loans block a new request — any active loan, or only overdue loans — is **not decided**. Literal reading conflicts with BR-M9-01 (limit 3). | **Superseded by BR-M10-02, BR-M10-03, BR-M10-13 (M10, M11)** — now independent configuration settings — was [Open] Q-M9-1 |

Related existing rules (archive, still Proposed): only active members may borrow (BR-03); one
active loan per physical copy (BR-04); atomic issue/return (BR-15); concurrent issue of one copy →
only one succeeds (BR-16).
