# D-58 rename plan: `Member` → `LibraryMember`

Status: **plan Approved** (user, Session 23: "recommendation approved"; Q1–Q6 as recommended). Implementation in progress. Naming is Approved in D-58
(entity `LibraryMember`, table `LibraryMembers`, FK `LibraryMemberId`; keep `MemberCode`,
`MemberType`, `IdentityUserId`, `LibraryPolicies.MemberCategory`). Items marked **Q** need the
user's decision before implementation.

## 1. Impact inventory (measured on `develop` @ `47488bd`)

### 1.1 Code — 36 files
| Layer | Files |
|---|---|
| Core (9) | `Entities/Member.cs`, `DTOs/MemberDto.cs`, `DTOs/BookDto.cs`, `Interfaces/IUnitOfWork.cs`, `Interfaces/Repositories/IMemberRepository.cs`, `Interfaces/Repositories/IBookRepository.cs`, `Interfaces/Services/IMemberService.cs`, `Constants/Permissions.cs`, `Results/ErrorCodes.cs` |
| Data (6) | `Data/ApplicationDbContext.cs`, `Data/Configurations/MemberConfiguration.cs`, `Data/DbSeeder.cs`, `Repositories/MemberRepository.cs`, `Repositories/BookRepository.cs`, `Repositories/UnitOfWork.cs` |
| Services (4) | `DependencyInjection.cs`, `Services/MemberService.cs`, `Services/LendingService.cs`, `Services/RegistrationService.cs` |
| Web (13) | `Controllers/MembersController.cs`, `LoansController.cs`, `PortalController.cs`; `Views/Members/{Index,Form,Details}.cshtml`, `Views/Loans/Index.cshtml`, `Views/Policies/Index.cshtml`, `Views/Portal/{MyLoans,Pending,Register}.cshtml`, `Views/Shared/{_Layout,_LoanTable}.cshtml` |
| Tests (4) | `Integration/{AuditTests,LendingTests,PortalTests}.cs`, `Unit/ArchitectureTests.cs` |

Identifier occurrences (whole word): `Member` 64, `Members` 53, `MemberId` 19, `MemberDto` 21,
`MemberService` 13, `MemberDetailsDto` 12, `IMemberService` 11, `Permissions.Members` 8,
`MemberRepository` 6, `IMemberRepository` 4, `MemberInactive` 3; unchanged by design:
`MemberCode` 49, `MemberType` 58.

### 1.2 Rename map
| Now | After |
|---|---|
| `Member` (entity), `Member.Loans` | `LibraryMember`, `LibraryMember.Loans` |
| `Loan.MemberId`, `Loan.Member` | `Loan.LibraryMemberId`, `Loan.LibraryMember` |
| `MemberDto`, `MemberDetailsDto` | `LibraryMemberDto`, `LibraryMemberDetailsDto` |
| `LoanDto.MemberId` | `LoanDto.LibraryMemberId` (`MemberCode`, `MemberName` stay — they mirror columns) |
| `IMemberRepository` / `MemberRepository`, `IUnitOfWork.Members` | `ILibraryMemberRepository` / `LibraryMemberRepository`, `IUnitOfWork.LibraryMembers` |
| `IMemberService` / `MemberService` | `ILibraryMemberService` / `LibraryMemberService` |
| `MemberConfiguration` | `LibraryMemberConfiguration` |
| `ApplicationDbContext.Members` | `ApplicationDbContext.LibraryMembers` |
| `MembersController`, `Views/Members/*`, URL `/Members/...` | `LibraryMembersController`, `Views/LibraryMembers/*`, URL `/LibraryMembers/...` (Q3) |
| `Permissions.Members.*` = `"Members.View"` … | `Permissions.LibraryMembers.*` = `"LibraryMembers.View"` … (Q2) |
| UI labels "Member(s)", "Member code" | "Library Member(s)", "Member code" |
| unchanged | `MemberCode`, `MemberType` enum, `RegistrationType`, `IdentityUserId`, `MemberCategory`, `ErrorCodes.MemberInactive` / `MEMBER.INACTIVE` (Q4) |

### 1.3 Database objects (all created by `AddLibraryCore`)
| Now | After |
|---|---|
| table `Members`, `PK_Members` | `LibraryMembers`, `PK_LibraryMembers` |
| sequence `Members_HiLo` | `LibraryMembers_HiLo` |
| `IX_Members_MemberCode`, `IX_Members_FullName`, `IX_Members_IdentityUserId` | `IX_LibraryMembers_…` |
| `CK_Members_Status`, `CK_Members_MemberType`, `CK_Members_Approval` | `CK_LibraryMembers_…` |
| `FK_Members_AspNetUsers_IdentityUserId` | `FK_LibraryMembers_AspNetUsers_IdentityUserId` |
| `LendingTransactions.MemberId` | `LendingTransactions.LibraryMemberId` |
| `FK_LendingTransactions_Members_MemberId` | `FK_LendingTransactions_LibraryMembers_LibraryMemberId` |
| `IX_LendingTransactions_MemberId_TransactionStatus` | `IX_LendingTransactions_LibraryMemberId_TransactionStatus` |
| unchanged | `IX_LibraryPolicies_MemberCategory`, `MemberCode`, `MemberType` columns |

### 1.4 Data that already exists
- **No non-test database has these tables.** `.\SQLEXPRESS` has no user database; LocalDB only
  `Practice` plus the running smoke database (`CseLibrary_Test_*`, dropped after the smoke).
  `AddLibraryCore` has never been applied outside `CseLibrary_Test_*` databases.
- `AuditLogs.EntityType = 'Member'` rows exist only in test databases. Audit rows are history and
  are never rewritten (D-21); new rows will say `LibraryMember` (Q5).
- Permission rows `Members.*` and their role grants exist only in test databases (Q2).

### 1.5 Docs
Living docs using the term: `requirements.md` (6), `database.md` (6), `diagrams/database-model.*`,
`diagrams/database-tables.html`, `diagrams/target-architecture.*`, `PROJECT_DECISIONS.md` (28,
mostly historical decisions), `SESSION_HANDOFF.md`, `PHASE_0_PLAN.md` (1). The archive
(`docs/archive/1st discussion.md`) is byte-locked and is **not** edited.

## 2. Decisions needed

| Q | Question | Recommendation |
|---|---|---|
| Q1 | Migration: (A) regenerate `AddLibraryCore` with the new names, or (B) keep it and add a rename migration (`RenameTable`/`RenameColumn`/`RenameIndex`, `sp_rename` for the sequence, drop/re-add FKs and CHECKs) | **(A)** — nothing is deployed, so a clean baseline costs less and leaves no rename noise. (B) becomes mandatory the moment any real database has these tables. Deleting/regenerating a migration needs explicit approval (CLAUDE.md). |
| Q2 | Permission names `Members.*` → `LibraryMembers.*`? | **Yes.** With (A) there are no rows to migrate. (Under (B) the migration would `UPDATE Permissions SET Name, Module` so role grants keep their ids.) |
| Q3 | URL `/Members` → `/LibraryMembers` (follows the controller name)? | **Yes**, no redirect — no external links exist yet. |
| Q4 | Error code `MEMBER.INACTIVE` | **Keep.** Error codes are stable contracts and the archive (BR-03) uses it. |
| Q5 | Rewrite old audit rows `EntityType = 'Member'`? | **No** (D-21: history is never rewritten; only test rows exist). |
| Q6 | Living docs and diagrams | Update `requirements.md`, `database.md`, the three diagrams (regenerate from JSON) and `SESSION_HANDOFF.md` to "Library Member"; historical decision text in `PROJECT_DECISIONS.md` stays, with a note under D-58. |

## 3. Implementation steps (each step = one reviewed commit on `feature/d58-library-member-rename`)

1. **Code rename** (Core → Data → Services → Web → tests), mechanical, using the rename map
   above with whole-word replacements on an explicit identifier list (never a blind
   `Member` → `LibraryMember`, which would also hit `MemberCode`/`MemberType`). Move
   `Views/Members` → `Views/LibraryMembers` with `git mv`. Update `asp-controller="Members"`.
2. **Migration (Q1 = A):** `dotnet ef migrations remove --force` for `AddLibraryCore`, then
   `dotnet ef migrations add AddLibraryCore` (design-time connection string pointed at a
   never-created `CseLibrary_Test_*` name). Check `Up()` is additive only. Update
   `MigrationTests` expected sequences (`LibraryMembers_HiLo`).
3. **Docs** (Q6) and D-58 status → Implemented with the commit ids.

## 4. Verification (evidence required before each commit)
- `dotnet build` 0 errors / 0 warnings; `dotnet test` all green (currently 122).
- `dotnet ef migrations has-pending-model-changes` → none.
- `grep -rw "Member\|Members\|MemberId\|IMemberService\|MemberDto"` over `src`/`tests` returns
  only the intended leftovers (`MemberCode`, `MemberType`, `MemberCategory`, `MemberName`,
  `MemberInactive`, `MEMBER.INACTIVE`).
- Smoke on a disposable `CseLibrary_Test_*` database: Library Members list/create/edit, issue,
  return, My books, Fellow approval; the menu shows "Library Members".

## 5. Risks
| Risk | Mitigation |
|---|---|
| Over-replacement (e.g. `MemberCode` → `LibraryMemberCode`) | explicit identifier list + grep check (§4) + compile |
| Razor strings not caught by the compiler (`asp-controller="Members"`, labels) | grep `.cshtml` for `"Members"`; smoke test every page |
| Permission constant renamed but menu/`HasPermission` miss one | `ArchitectureTests` + access tests; grep `Permissions.Members` = 0 |
| Model snapshot drift | regenerate via `migrations remove`/`add`, never hand-edit; `has-pending-model-changes` |

Estimated size: ~36 files changed, 1 regenerated migration, ~6 docs.
