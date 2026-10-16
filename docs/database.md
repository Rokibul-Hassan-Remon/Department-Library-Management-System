# Database design

**Status: Proposed (M15, Session 16): not approved, not implemented.** This file owns the target
schema for the library tables. The binding rules it builds on live in
[PROJECT_DECISIONS.md](PROJECT_DECISIONS.md); the source spec is
[archive spec v1.1 §4–§5](archive/1st%20discussion.md) (tables `Books`, `BookCopies`, `LibraryMembers`,
`LendingTransactions`, supporting tables). Requirements: FR-M15-01..03 in
[requirements.md](requirements.md). Tables are added in their phase migrations (catalog Phase 2,
members Phase 2, lending Phase 3, notifications Phase 4), never in Phase 0.

Diagrams: ER diagram [diagrams/database-tables.html](diagrams/database-tables.html); table overview
[diagrams/database-model.html](diagrams/database-model.html) (overall system:
[diagrams/target-architecture.html](diagrams/target-architecture.html)).

## 1. Core idea: title, copy, loan

| Question the user asked (M15) | Answer in the schema |
|---|---|
| One title, many physical copies? | `Books` (title) 1 — N `BookCopies` (one row per physical copy, unique `AccessionNumber`) |
| Issuing a copy reduces the available count by 1? | Issue sets that copy's `CopyStatus` to `Issued`, so the derived count of `Available` copies drops by 1 (§3) |
| Who holds a copy now? | The copy's one open loan: `LendingTransactions` row with `TransactionStatus = Issued` → `MemberId` |
| Full history of who, which copy, when issued, when returned? | Every loan is one `LendingTransactions` row; return fills `ReturnDateUtc` and closes it; rows are never deleted or reused |

## 2. Tables

**ER diagram (open in a browser): [diagrams/database-tables.html](diagrams/database-tables.html)**:
every table with all columns, types and PK/FK/UQ keys, FK → PK lines in crow's-foot notation
(click a table to highlight its relationships), each relationship in words, and per-column
notes. That page owns the column list; this file owns the reasoning.

### Conventions (Approved, Session 16: D-50, D-51)

| Rule | Example |
|---|---|
| Primary key is always `Id` (`int` identity unless noted; HiLo is the D-29 candidate, decided separately) | `Books.Id`, `BookCopies.Id` |
| Foreign key is the **singular entity name + `Id`** | `BookCopies.BookId`, `Books.CategoryId`, `LibraryMembers.IdentityUserId` |
| Two FKs to the same table get a role prefix | `LendingTransactions.IssuedByUserId`, `ReturnedByUserId` |
| Every business table (`BaseEntity`) has `Status` + `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy` | template names kept (option A); join tables only `CreatedAt`/`CreatedBy` |
| Those five columns are **set automatically** by a `SaveChanges` interceptor (user from the current login, time from `TimeProvider`, UTC); services never set them | — |
| Log tables have none of them | `AuditLogs`, `EmailNotificationLogs`, `OutboxMessages` |
| Timestamps are UTC `datetime2`; `DueDate` is a library-local `date` (`Asia/Dhaka`) | — |

The four audit columns show only the **last** change, for quick troubleshooting. The full history
(who, when, old → new) is `AuditLogs` (D-21): catalog, member and lending tables are `IAuditable`
with per-property `.Audited()` (D-21–D-23).

### Record status (D-51)

One column replaces both `IsActive` and `IsDeleted`:

```csharp
public enum RecordStatus : byte { Active = 1, Inactive = 2, Deleted = 3 }   // 0 is never valid
```

- `Active`: normal. `Inactive`: visible but blocked (a member cannot borrow, a user cannot log in).
  `Deleted`: soft-deleted, hidden everywhere.
- Stored as `tinyint` with `CHECK (Status IN (1,2,3))`. New states are appended (`4`, `5`, …) with
  a migration that widens the `CHECK`; existing numbers never change.
- **One global query filter** in `ApplicationDbContext` hides `Deleted` rows for every
  `BaseEntity`; per-table filters are removed.
- **Delete = soft delete, centrally:** the interceptor turns a repository `Remove()` of a
  `BaseEntity` into `Status = Deleted`; services do not set it by hand.
- **Unique indexes ignore deleted rows:** `WHERE Status <> 3`, so a deleted name can be reused.
- `RecordStatus` is the row's lifecycle only. Business states stay separate: `CopyStatus`
  (Available, Issued, …) and `TransactionStatus` (Issued, Returned). `LendingTransactions` is
  never deleted, so it never uses `Deleted`.
- Applies to the template too (`BaseEntity.IsDeleted`, `ApplicationUser.IsActive`): implemented in
  Gate 2, step 0.8 (PHASE_0_PLAN).

`LibraryMembers` is shown as one table (open choice O3). Template tables (`AspNet*`, `Permissions`,
`RolePermissions`, `GeneralConfigs`) keep their structure apart from D-50/D-51.

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
- **No delete operation** in the repository or the UI. Foreign keys to `BookCopies`, `LibraryMembers`
  and `AspNetUsers` use `ON DELETE NO ACTION`. A copy or member with history cannot be deleted; it
  is set `Inactive` (`Status = 2`) or retired (`CopyStatus = Retired`) instead (BR-13).
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
| O3 | Member subtypes | (a) archive: `LibraryMembers` + `Students` + `DepartmentFellows` (1:1); (b) one `LibraryMembers` table with nullable type columns + `CHECK` per `MemberType` | **(b)**: only 3–4 columns differ, no joins, and D-48 locks a single member row. (a) is cleaner if the subtypes grow. Decide in Phase 2 |
| O4 | Member name in history | Live FK (history shows the current name) / snapshot name columns on the loan | **Live FK**; name changes are in `AuditLogs` |
| O5 | `LendingTransactions.Id` type | `bigint` (archive) / `int` | **`bigint`**: costs 4 bytes per row and never runs out |

Nothing here changes an Approved decision. D-35 and D-48 are reused unchanged.
