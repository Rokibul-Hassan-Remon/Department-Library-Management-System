# Database design

**Status: Proposed (M15, Session 16): not approved, not implemented.** This file owns the target
schema for the library tables. The binding rules it builds on live in
[PROJECT_DECISIONS.md](PROJECT_DECISIONS.md); the source spec is
[archive spec v1.1 §4–§5](archive/1st%20discussion.md) (tables `Books`, `BookCopies`, `Members`,
`LendingTransactions`, supporting tables). Requirements: FR-M15-01..03 in
[requirements.md](requirements.md). Tables are added in their phase migrations (catalog Phase 2,
members Phase 2, lending Phase 3, notifications Phase 4), never in Phase 0.

Diagram: [diagrams/database-model.html](diagrams/database-model.html) (overall system:
[diagrams/target-architecture.html](diagrams/target-architecture.html)).

## 1. Core idea: title, copy, loan

| Question the user asked (M15) | Answer in the schema |
|---|---|
| One title, many physical copies? | `Books` (title) 1 — N `BookCopies` (one row per physical copy, unique `AccessionNumber`) |
| Issuing a copy reduces the available count by 1? | Issue sets that copy's `CopyStatus` to `Issued`, so the derived count of `Available` copies drops by 1 (§3) |
| Who holds a copy now? | The copy's one open loan: `LendingTransactions` row with `TransactionStatus = Issued` → `MemberId` |
| Full history of who, which copy, when issued, when returned? | Every loan is one `LendingTransactions` row; return fills `ReturnDateUtc` and closes it; rows are never deleted or reused |

## 2. Tables

Keys are `int` identity unless noted (HiLo is the D-29 candidate for audited tables and is decided
separately). Timestamps are UTC `datetime2`; `DueDate` is a library-local `date` (`Asia/Dhaka`).
"Audit" = `IAuditable` + per-property `.Audited()` (D-21–D-23).

| Table | Key columns and constraints | Notes |
|---|---|---|
| `Categories` | `CategoryId`, `Name` UQ, `IsActive` | Book subject list |
| `Authors` | `AuthorId`, `Name` | |
| `BookAuthors` | PK (`BookId`, `AuthorId`), both FK | Many-to-many |
| `Books` | `BookId`, `Title` (250) required, `ISBN` (20) filtered UQ when not null, `CategoryId` FK, `IsActive`, `RowVersion` | Title metadata only; no copy counters. Audit |
| `BookCopies` | `BookCopyId`, `BookId` FK, `AccessionNumber` (50) UQ, `CopyStatus` tinyint + `CHECK` (1–6), `ShelfLocation`, `AcquisitionDate`, `RowVersion` | One row per physical copy. Index (`BookId`, `CopyStatus`). Audit |
| `Members` | `MemberId`, `MemberCode` (30) UQ, `FullName`, `MemberType` tinyint (Student/Fellow) + `CHECK`, `Email`, `Phone`, `IsActive`, `IdentityUserId` FK filtered UQ when not null, `RowVersion` | `MemberType` is the D-48 policy category. Audit (email/phone included, D-23) |
| `Students` / `DepartmentFellows` | PK = FK `MemberId`, type-specific columns | Archive subtype tables; see open choice O3 |
| `LibraryPolicies` | per D-48 §1 (`MemberCategory` UQ, limits, `LoanDurationDays`, `CHECK`s) | Unchanged from D-48 |
| `LendingTransactions` | `LendingTransactionId` bigint, `BookCopyId` FK, `MemberId` FK, `IssueDateUtc`, `DueDate`, `ReturnDateUtc` null, `TransactionStatus` tinyint (1 Issued, 2 Returned), `IssuedByUserId` FK, `ReturnedByUserId` FK null, `ReturnCondition` null, `RowVersion` | Business history (D-20, D-26). Audit |
| `EmailNotificationLogs`, `OutboxMessages` | archive §7.2, `IdempotencyKey` UQ | Phase 4 (D-36) |
| `AuditLogs` | per D-21 | No FK (`EntityType`, `EntityId`) so it outlives any row |
| Template tables | `AspNet*` (Identity), `Permissions`, `RolePermissions`, `GeneralConfigs` | Kept as they are |

## 3. Available count: derived, not stored

**Recommendation: derive the counts from `BookCopies`; do not keep a counter column on `Books`.**

```sql
SELECT BookId,
       COUNT(*)                                       AS TotalCopies,
       SUM(CASE WHEN CopyStatus = 1 THEN 1 ELSE 0 END) AS AvailableCopies,
       SUM(CASE WHEN CopyStatus = 2 THEN 1 ELSE 0 END) AS IssuedCopies
FROM BookCopies GROUP BY BookId;   -- one repository projection, served by IX (BookId, CopyStatus)
```

| | Derived (recommended) | Counter column `Books.AvailableCopies` |
|---|---|---|
| Correctness | Always equals the copy rows; nothing can drift | Must change in every issue, return, lost, damaged, retire and add-copy path; a missed path or direct SQL fix drifts silently |
| Concurrency | Issue touches only the copy row it issues | Every issue of a popular title updates the same `Books` row: hot row, extra lock, more conflicts |
| Audit noise | None | An audit entry on `Books` for every loan |
| Read cost | Grouped index seek; thousands of copies is trivial for SQL Server | Single column read |
| If reads ever get slow | Indexed view on the same query (DB-maintained, cannot drift) | n/a |

This also matches the archive ("calculate from `BookCopies` rather than editable counters").

## 4. Copy holder and the open-loan invariant

- **Invariant:** a copy has `CopyStatus = Issued` **if and only if** it has exactly one loan with
  `TransactionStatus = Issued`.
- **Database guard:** filtered unique index `UX_Lending_ActiveBookCopy` on
  `LendingTransactions (BookCopyId) WHERE TransactionStatus = 1`. Two open loans for one copy are
  impossible, even through a race or a direct SQL write. The same index makes "who holds copy X"
  a single index seek.
- **Application guard (D-35):** issue and return change the copy row and the loan row in **one**
  `SaveChanges`/transaction; `BookCopies.RowVersion` makes a concurrent issue, or a concurrent
  "mark lost/damaged", fail as a conflict instead of overwriting.
- **No `CurrentHolderMemberId` column on `BookCopies`:** it would duplicate the open loan and could
  disagree with it.
- **Check:** an integration test asserts the invariant after the issue/return tests (Phase 3).

## 5. Lending history: append-only by design

- **Insert** on issue. On return, the only update sets `TransactionStatus = Returned`,
  `ReturnDateUtc`, `ReturnedByUserId`, `ReturnCondition`. This happens once; `RowVersion` rejects a
  second return (D-35).
- **No delete operation** in the repository or the UI. Foreign keys to `BookCopies`, `Members`
  and `AspNetUsers` use `ON DELETE NO ACTION`. A copy or member with history cannot be deleted; it
  is deactivated (`IsActive = 0`) or retired (`CopyStatus = Retired`) instead (BR-13).
- **`CHECK` constraints** make a bad row impossible:
  - `(TransactionStatus = 1 AND ReturnDateUtc IS NULL AND ReturnedByUserId IS NULL AND ReturnCondition IS NULL)`
    `OR (TransactionStatus = 2 AND ReturnDateUtc IS NOT NULL AND ReturnedByUserId IS NOT NULL AND ReturnCondition IS NOT NULL)`
  - `DueDate >= CAST(IssueDateUtc AS date)` (BR-06) and `ReturnDateUtc >= IssueDateUtc`.
- **Overdue is not stored.** It is `Issued AND DueDate < today` (library-local), so it can never be
  stale.
- **Who changed what:** every insert or update is also recorded in `AuditLogs` (D-21). A database
  `DENY UPDATE, DELETE` for history tables remains a Phase 7 hardening option (D-28).
- **Indexes for the history screens:** (`MemberId`, `IssueDateUtc` DESC) for "my history";
  (`BookCopyId`, `IssueDateUtc` DESC) for copy history; filtered (`DueDate`) `WHERE
  TransactionStatus = 1` for the reminder job and the overdue list.

## 6. Open choices (need the user's decision before the phase that builds them)

| # | Choice | Options | Recommendation |
|---|---|---|---|
| O1 | Available count | Derived (§3) / counter column | **Derived** |
| O2 | `Issued` inside `CopyStatus` | (a) keep `Issued` (archive), changed in the same transaction as the loan; (b) `CopyStatus` = physical state only, "issued" read from the open loan | **(a)**: one `RowVersion` on the copy serializes issue against lost/damaged changes, counts are one `GROUP BY`, and the filtered unique index still backs it up. (b) removes the duplicate but needs a lock or extra check to stop "mark lost" during a concurrent issue |
| O3 | Member subtypes | (a) archive: `Members` + `Students` + `DepartmentFellows` (1:1); (b) one `Members` table with nullable type columns + `CHECK` per `MemberType` | **(b)**: only 3–4 columns differ, no joins, and D-48 locks a single member row. (a) is cleaner if the subtypes grow. Decide in Phase 2 |
| O4 | Member name in history | Live FK (history shows the current name) / snapshot name columns on the loan | **Live FK**; name changes are in `AuditLogs` |
| O5 | `LendingTransactionId` type | `bigint` (archive) / `int` | **`bigint`**: costs 4 bytes per row and never runs out |

Nothing here changes an Approved decision. D-35 and D-48 are reused unchanged.
