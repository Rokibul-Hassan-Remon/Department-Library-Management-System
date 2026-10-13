তোমার draft-টাকে আমরা একটি proper Software Requirements Specification (SRS) + Functional Specification + Database Design + Development Roadmap-এ রূপ দিতে পারি, যাতে CSE Department-এর Library Management System বাস্তবে develop, test এবং maintain করা যায়।

তোমার মূল requirement অনুযায়ী system-টিতে শুধু Book CRUD থাকবে না; বরং Book Registration → Book Lending → Due Date Tracking → Book Return → Email Notification → Overdue Reminder — পুরো lifecycle থাকবে।

# CSE Department Library Management System

[Espacios de la biblioteca | Biblioteca Dr. Jorge Villalobos Padilla, SJ](https://images.openai.com/static-rsc-4/Tl9SDHJcX9FR6h-LcKyCCsOnzuDsCvw6wadjt2bI3wmW_PqM5XDuBSBJ1OoTyIZQHz1UdhcME-T-8I6brKPGZJt161lc042L6O6M9V-oj2HNClfKyuObuMz0Lt9qInDHaF7uGeIMwpBsKYFzsDMWZyrYvPEwYTOAyHk_wgoyVTg?purpose=inline)

[biblio.iteso.mx](https://biblio.iteso.mx/servicios/espacios)

Proposed system: Departmental library management for books, students, departmental fellows and lending operations.

## 1. Project overview

| Item           | Specification                                                                |
| -------------- | ---------------------------------------------------------------------------- |
| Project name   | CSE Department Library Management System                                     |
| Primary users  | Librarian, Department Admin, Students, Department Fellows                    |
| Platform       | Web-based application                                                        |
| Core modules   | Books, Members, Lending, Returns, Notifications, Reports                     |
| Main objective | বইয়ের inventory, borrowing history, due dates এবং return process ডিজিটাল করা |

প্রস্তাবিত technology stack: ASP.NET Core MVC, Entity Framework Core এবং SQL Server — তোমার .NET development background-এর সঙ্গে এটি মানানসই।

## 2. Complete module breakdown

Module 01 — Book Management

- New Book Registration, including book cover/photo
- Book Index/List with search, filter and pagination
- Add, Edit, Delete
- ISBN, title, author, publisher, edition, category, shelf location
- Initial quantity, available quantity and total copies
- Multiple physical copies হলে প্রতিটি copy-এর unique accession number

Module 02 — Student Management

- Student Registration and Index
- Add, Edit, Delete/Deactivate
- Student ID, name, department, session, email, phone
- Active/inactive status
- Student-এর borrowing history

Module 03 — Department Fellow Management

- Student module-এর মতো Registration, Index, Edit
- Fellow ID, name, designation, email, contact
- Fellow-specific lending eligibility and borrowing limit
- Student থেকে আলাদা member type ও policy

Module 04 — Book Lending / Rental

- Available books list
- Initial/total copies এবং available copies
- Select member and select book copy
- Issue date, due date and issued-by librarian
- Lending history ও currently borrowed books
- Availability validation before issuing

Module 05 — Book Return / Restore

- Search by student ID, book title or accession number
- Show current borrower and due date
- Confirm return
- Record actual return date and receiving librarian
- Restore book availability
- Successful return confirmation email
- Overdue duration and optional fine calculation

Module 06 — Email Notification

- Issue confirmation email
- Return success email
- Upcoming due-date reminder
- Overdue notification
- Email delivery status and retry handling

Module 07 — Dashboard & Reports

- Total book titles and total physical copies
- Available, issued and overdue copies
- Total registered students and fellows
- Currently borrowed books
- Member-wise lending history
- Overdue list and date-range reports

## 3. Complete system workflow

এই workflow-টাই system-এর core business logic।

1\. Register Book

Book details + individual copy records

2\. Register Member

Student or Department Fellow

3\. Issue Book

Validate availability, member eligibility and due date

4\. Track Lending

Borrowing history + scheduled email reminders

5\. Return Book

Record return → restore availability → send confirmation

## 4. গুরুত্বপূর্ণ business rules

তোমার draft-এর কয়েকটি জায়গায় এখনই সঠিক design decision নেওয়া দরকার।

| বিষয়               | Proposed rule                                                      |
| ------------------ | ------------------------------------------------------------------ |
| Initial Book (N)   | শুরুতে নিবন্ধিত মোট physical copies                                |
| Available Book (N) | বর্তমানে issue করা হয়নি এবং lending-এর উপযোগী এমন copies           |
| Issued Book (N)    | বর্তমানে borrower-এর কাছে থাকা copies                              |
| Delete Book        | Historical lending record থাকলে hard delete নয়; archive/deactivate |
| Delete Student     | Borrowing history থাকলে hard delete নয়; deactivate                 |
| Return             | একই lending record দ্বিতীয়বার return করা যাবে না                   |
| Due date           | Configurable lending policy অনুযায়ী নির্ধারিত হবে                  |
| Email              | Database transaction সফল হওয়ার পর notification process শুরু হবে    |
| Overdue            | Due date পার হয়েছে এবং book এখনো return হয়নি                       |
| Book identity      | Book title এবং physical copy-কে আলাদা entity হিসেবে রাখতে হবে      |

বিশেষ গুরুত্বপূর্ণ: একটি বইয়ের title-এর ৫টি physical copy থাকলে শুধু `Book.Quantity = 5` রাখলে কোন copy কার কাছে আছে, সেটা নির্ভরযোগ্যভাবে track করা কঠিন হবে। তাই `Book` এবং `BookCopy` আলাদা table হওয়া উচিত।

এখন নিচের document-টি project-এর মূল specification হিসেবে ব্যবহার করতে পারো।

# CSE Department Library Management System

## Software Requirements Specification (SRS)

**Document Version:** 1.0\
**Project Type:** Web-based Library Management System\
**Target Organization:** Department of Computer Science and Engineering (CSE)\
**Status:** Proposed Requirements — pending approval by the department

---

## 1. Introduction

### 1.1 Purpose

The purpose of this project is to develop a web-based Library Management System for the CSE Department to digitize and manage library books, physical book copies, registered members, lending transactions, book returns, overdue tracking, email notifications, and reports.

The system will reduce manual record-keeping, improve inventory accuracy, and make it easier to identify which member has borrowed which book and when it should be returned.

### 1.2 Project Objectives

- Maintain a centralized book catalog.
- Track individual physical copies of books.
- Manage student and departmental fellow registrations.
- Record all book issue and return transactions.
- Automatically calculate and track due dates.
- Send email confirmations and overdue reminders.
- Generate inventory, lending, and overdue reports.
- Maintain an auditable history of library operations.

### 1.3 Scope

The initial release will cover:

1. Authentication and role-based access.
2. Book and book-copy management.
3. Student management.
4. Department Fellow management.
5. Book lending and return processing.
6. Email notifications and overdue reminders.
7. Dashboard, search, and reports.
8. Audit logs and application settings.

Features such as online reservations, fines, barcode scanning, and self-service member portals may be introduced in later releases.

### 1.4 Definitions

- **Book:** The bibliographic record representing a title or edition.
- **Book Copy:** An individual physical copy of a book.
- **Member:** A registered student or departmental fellow eligible to borrow books.
- **Lending Transaction:** A record of issuing a physical copy to a member.
- **Due Date:** The date by which a borrowed book should be returned.
- **Overdue:** A lending transaction whose due date has passed without a recorded return.
- **Accession Number:** A unique identifier assigned to an individual physical book copy.
- **Soft Delete:** Marking a record inactive without removing its historical data.

---

## 2. User Roles and Permissions

### 2.1 Administrator

- Manage librarian and administrator accounts.
- Configure library policies and notification settings.
- Access all modules and reports.
- Review audit logs.
- Manage system-level settings.

### 2.2 Librarian

- Register and update books and book copies.
- Register and manage students and departmental fellows.
- Issue and return books.
- View member borrowing histories.
- View overdue records and operational reports.

### 2.3 Student

In the initial release, student information will be managed by authorized library staff.

A future member portal may allow students to view their own borrowed books, due dates, and borrowing history.

### 2.4 Department Fellow

Departmental fellows will have a separate member category and may have different lending policies.

Their records and borrowing histories will be managed by authorized library staff.

### 2.5 Authorization Rules

- Only authenticated, authorized users may access protected operations.
- Only permitted roles may issue or return books.
- Only administrators may modify system-level settings and privileged accounts.
- Members must not access other members' private borrowing information through any future self-service portal.
- Important administrative actions must be logged.

---

## 3. Functional Requirements

### FR-01: Authentication and User Management

The system shall:

- Provide secure login and logout.
- Support administrator and librarian roles.
- Restrict access according to role permissions.
- Allow authorized administrators to create, update, deactivate, and reactivate staff accounts.
- Support password change and secure password storage.
- Record relevant login and administrative activity.

### FR-02: Book Management

The system shall provide a Book Registration form containing:

- Book ID — automatically generated.
- ISBN or other standard identifier, where available.
- Book Title.
- Author or authors.
- Publisher.
- Edition.
- Publication Year.
- Category or Subject.
- Language, if required.
- Description, optional.
- Cover Image, optional.
- Created Date.
- Record Status.

The system shall support:

- Registering a new book title.
- Viewing all books through an Index page.
- Editing book metadata.
- Searching by title, author, ISBN, or accession number.
- Filtering by category, publisher, or availability.
- Pagination and sorting.
- Viewing the physical copies associated with a book.
- Deactivating a book that should no longer be circulated.

Validation rules:

- Book Title is required.
- Input lengths must respect the database schema.
- Duplicate ISBNs should be detected where ISBN is available.
- Image uploads must have permitted file types and size limits.
- Existing lending history must not be deleted when book metadata changes.
- A book title cannot be permanently removed while referenced by historical transactions.

### FR-03: Book Copy / Inventory Management

Each physical copy shall have its own record.

Fields:

- Book Copy ID.
- Book ID.
- Accession Number.
- Acquisition Date, optional.
- Shelf or Rack Location.
- Copy Status.
- Notes, optional.

Supported copy statuses:

- Available.
- Issued.
- Lost.
- Damaged.
- Maintenance or unavailable.
- Retired.

The system shall:

- Add one or more copies to an existing book title.
- Assign a unique accession number to each copy.
- Update copy details.
- Display total copies, available copies, issued copies, and unavailable copies.
- Prevent an unavailable copy from being issued.
- Preserve copy and lending history.

Inventory calculation:

**Total Copies = Available + Issued + Unavailable**

Here, Unavailable includes lost, damaged, maintenance, and retired copies. The report should distinguish these categories where useful.

### FR-04: Student Management

The system shall provide a Student Registration form with:

- Student ID — unique institutional identifier.
- Full Name.
- Department.
- Academic Session.
- Email Address.
- Phone Number, optional.
- Registration Date.
- Membership Status.
- Notes, optional.

The system shall support:

- New Student Registration.
- Student Index page.
- Search by student ID, name, or email.
- Editing student information.
- Viewing a student's borrowing history.
- Deactivating or reactivating a membership.
- Identifying currently borrowed and overdue books.

Validation rules:

- Student ID is required and unique.
- Email must follow valid email formatting when provided.
- A member with active lending transactions must not be deleted in a way that breaks historical records.
- Inactive members cannot borrow new books.

### FR-05: Department Fellow Management

This module will follow the Student Management structure while maintaining a distinct member type.

Fields:

- Fellow ID.
- Full Name.
- Designation.
- Department.
- Institutional Email.
- Contact Number, optional.
- Registration Date.
- Membership Status.

The system shall support registration, listing, searching, editing, activation/deactivation, and borrowing-history viewing.

Student and Department Fellow records may share common member information through a common Member model, while retaining their category-specific information.

### FR-06: Book Lending / Issue Management

The system shall provide an Issue Book page.

The librarian shall be able to:

1. Search for and select a registered member.
2. View the member's eligibility and active lending transactions.
3. Search for an available book.
4. Select a specific physical copy.
5. Set the issue date and due date.
6. Confirm the issue.
7. Generate a lending transaction record.

The system shall record:

- Lending Transaction ID.
- Book Copy ID.
- Member ID.
- Issue Date.
- Due Date.
- Issued By.
- Transaction Status.
- Notes, optional.

After a successful issue:

- The physical copy's status shall change to Issued.
- The transaction shall be recorded.
- The copy shall no longer appear as available for another issue.
- The system may send an issue confirmation email.

Validation rules:

- The member must be active.
- The selected copy must be available.
- The member must not exceed the configured borrowing limit.
- The due date must not precede the issue date.
- A physical copy must not have more than one active lending transaction.
- Concurrent issue attempts must not cause the same copy to be issued twice.

### FR-07: Lending Index and Borrowing History

The system shall provide a Lending Index page containing:

- Transaction ID.
- Member name and ID.
- Member category.
- Book title.
- Accession number.
- Issue date.
- Due date.
- Return date, when applicable.
- Current transaction status.

The system shall support:

- Searching by member, book title, transaction ID, or accession number.
- Filtering by active, returned, and overdue transactions.
- Filtering by date range and member category.
- Viewing all books currently borrowed by a particular member.
- Viewing historical borrowing records.
- Exporting authorized reports where required.

### FR-08: Book Return / Restore Process

The librarian shall be able to return a book by searching for its transaction, member, or accession number.

The return page shall display:

- Borrower details.
- Book details.
- Accession number.
- Issue date.
- Due date.
- Current status.
- Overdue duration, if applicable.

The return process shall:

1. Verify that the lending transaction is active.
2. Record the actual return date and time.
3. Record the staff member processing the return.
4. Update the transaction status to Returned.
5. Update the physical copy status according to its condition.
6. Restore availability if the copy is fit for circulation.
7. Trigger a return confirmation email.

Validation rules:

- Only an active lending transaction can be returned.
- A return transaction must be processed only once.
- If a book is damaged or lost, it must not automatically become available.
- The system shall retain the original issue date and due date.
- Failed return operations must not leave the lending and inventory records inconsistent.

### FR-09: Email Notification Management

The system shall support configurable email notifications.

**A. Issue Confirmation**

Sent after a successful book issue.

Suggested contents:

- Member name.
- Book title and accession number.
- Issue date.
- Due date.
- Library contact information.

**B. Return Confirmation**

Sent after a successful return.

Suggested contents:

- Member name.
- Book title.
- Return date.
- Transaction reference.
- Outstanding overdue information, if applicable.

**C. Upcoming Due-Date Reminder**

Sent before the due date according to the configured reminder schedule.

**D. Overdue Reminder**

Sent to members whose books remain unreturned after the due date.

Suggested contents:

- Member name.
- Book title.
- Accession number.
- Original due date.
- Number of overdue days.
- Instructions for returning the book.

The system shall:

- Use a configurable email service.
- Keep email templates editable by authorized administrators.
- Log notification type, recipient, related transaction, timestamp, and delivery status.
- Support retrying temporary delivery failures.
- Avoid sending duplicate reminders for the same scheduled reminder event.
- Never mark a book as returned or issued based only on email delivery status.

An email failure shall not undo an otherwise successful lending or return transaction.

### FR-10: Automated Due-Date and Overdue Processing

The system shall run scheduled background processing to identify:

- Books due soon.
- Books due today.
- Books overdue.
- Notifications requiring retry.

The reminder schedule shall be configurable, for example:

- Three days before the due date.
- On the due date.
- After the due date, at a configured interval.

These intervals are examples and must be approved by the department.

The system shall:

- Use the configured institutional time zone.
- Exclude returned transactions from future overdue reminders.
- Avoid repeated reminders beyond the configured frequency.
- Maintain a record of each reminder attempt.
- Handle application restarts without permanently losing scheduled work.

### FR-11: Dashboard

The dashboard shall display:

- Total book titles.
- Total physical copies.
- Available copies.
- Currently issued copies.
- Unavailable copies.
- Registered students.
- Registered departmental fellows.
- Active lending transactions.
- Overdue transactions.
- Recent issues and returns.

The dashboard should provide links to the corresponding filtered lists.

### FR-12: Reports

The system shall support:

1. Book Inventory Report.
2. Available Books Report.
3. Currently Issued Books Report.
4. Member-wise Borrowing History.
5. Overdue Books Report.
6. Issue and Return Report by Date Range.
7. Student Membership Report.
8. Department Fellow Membership Report.
9. Lost and Damaged Books Report.
10. Email Notification Status Report.

Reports should support relevant filters and printable output. CSV or Excel export may be included based on departmental requirements.

### FR-13: Audit Logs

The system shall record important actions, including:

- Book and copy registration or modification.
- Member registration and status changes.
- Book issue and return.
- Policy changes.
- Staff account and permission changes.

Audit records should include the actor, action, timestamp, affected record, and relevant change information where appropriate.

Ordinary users must not be able to alter audit history.

### FR-14: Library Policy Settings

Authorized administrators shall be able to configure:

- Maximum active books per member category.
- Lending duration by member category.
- Reminder intervals.
- Whether renewals are permitted.
- Whether overdue fines are applicable.
- Holiday and closure rules, if necessary.
- Email sender configuration.
- Institutional contact details.

The actual values must be approved by the department rather than hard-coded into application logic.

---

## 4. Business Rules

1. Only active members may borrow books.
2. Only available physical copies may be issued.
3. Every lending transaction must reference one member and one physical book copy.
4. A physical copy may have only one active lending transaction.
5. Returning a book must update the transaction and inventory consistently.
6. A returned book may become available only if its condition permits circulation.
7. A member's historical lending records must remain available after deactivation.
8. Books and members referenced by historical transactions must not be hard-deleted.
9. Overdue status is derived from the due date, current time, and return status.
10. Email delivery failure must not reverse a completed issue or return.
11. Borrowing limits and due-date policies must be configurable.
12. All issue and return operations must be traceable to the responsible staff member.

---

## 5. Proposed Database Design

The initial database may contain the following tables.

### 5.1 Books

| Column          | Purpose                      |
| --------------- | ---------------------------- |
| BookId          | Primary key                  |
| ISBN            | Optional standard identifier |
| Title           | Book title                   |
| Publisher       | Publisher                    |
| Edition         | Edition                      |
| PublicationYear | Publication year             |
| CategoryId      | Foreign key                  |
| CoverImagePath  | Image reference              |
| Description     | Optional details             |
| IsActive        | Record status                |
| CreatedAt       | Creation timestamp           |

Author relationships should support multiple authors per book through separate `Authors` and `BookAuthors` tables if required.

### 5.2 BookCopies

| Column          | Purpose                          |
| --------------- | -------------------------------- |
| BookCopyId      | Primary key                      |
| BookId          | Foreign key to Books             |
| AccessionNumber | Unique physical-copy identifier  |
| ShelfLocation   | Rack or shelf                    |
| AcquisitionDate | Optional                         |
| CopyStatus      | Available, Issued, Damaged, etc. |
| Notes           | Optional                         |

### 5.3 Members

| Column     | Purpose                         |
| ---------- | ------------------------------- |
| MemberId   | Primary key                     |
| MemberCode | Unique institutional identifier |
| FullName   | Member name                     |
| MemberType | Student or Department Fellow    |
| Email      | Email address                   |
| Phone      | Optional                        |
| IsActive   | Membership status               |
| CreatedAt  | Registration timestamp          |

A separate `Students` table and `DepartmentFellows` table may store category-specific information linked to `Members`.

### 5.4 LendingTransactions

| Column               | Purpose                     |
| -------------------- | --------------------------- |
| LendingTransactionId | Primary key                 |
| BookCopyId           | Foreign key                 |
| MemberId             | Foreign key                 |
| IssueDate            | Issue date/time             |
| DueDate              | Due date/time               |
| ReturnDate           | Nullable actual return time |
| TransactionStatus    | Issued or Returned          |
| IssuedByUserId       | Staff user                  |
| ReturnedByUserId     | Nullable staff user         |
| Notes                | Optional                    |

The database should enforce the rule that a physical copy cannot have multiple active transactions, using an appropriate database constraint or transactional application logic.

### 5.5 Users and Roles

Recommended tables:

- `Users`
- `Roles`
- `UserRoles`

These will manage authenticated administrators and librarians, using a proven authentication framework where appropriate.

### 5.6 Supporting Tables

- `Categories`
- `Authors`
- `BookAuthors`
- `LibraryPolicies`
- `EmailTemplates`
- `EmailNotificationLogs`
- `AuditLogs`

Optional future tables:

- `Reservations`
- `BookRenewals`
- `Fines`
- `Payments`

### 5.7 Database Relationships

- One Book has many BookCopies.
- One Book may have many Authors.
- One Author may be associated with many Books.
- One Member has many LendingTransactions.
- One BookCopy has many historical LendingTransactions.
- One LendingTransaction may have many EmailNotificationLogs.
- One User may process many lending and return transactions.

**Important design decision:** Do not store the current available quantity as an independently editable number without synchronization rules. Prefer deriving inventory counts from copy statuses or maintaining a carefully synchronized summary.

---

## 6. Non-Functional Requirements

### 6.1 Security

- Enforce authentication and authorization.
- Validate inputs on the server.
- Protect against SQL injection, cross-site scripting, and cross-site request forgery where applicable.
- Store passwords using a secure password-hashing mechanism.
- Store email credentials and other secrets outside source control.
- Restrict access to uploaded files.
- Use HTTPS in production.

### 6.2 Performance

- Use database indexes for frequently searched fields.
- Apply pagination to large lists.
- Avoid unnecessary database queries.
- Use efficient queries for dashboards and reports.
- Set performance targets based on expected membership and inventory sizes.

### 6.3 Reliability

- Use database transactions for issue and return operations.
- Handle simultaneous issue attempts safely.
- Log application and notification failures.
- Implement reliable background scheduling for reminders.
- Provide database backup and recovery procedures.

### 6.4 Usability

- Provide clear validation messages.
- Use consistent navigation and page layouts.
- Support desktop and mobile browser layouts.
- Provide search, sorting, and filtering on relevant Index pages.
- Confirm potentially destructive actions.

### 6.5 Maintainability

- Separate business logic from controllers and views.
- Use dependency injection.
- Apply consistent naming conventions.
- Maintain migrations and version-controlled source code.
- Write automated tests for important business rules.

### 6.6 Privacy and Data Retention

- Collect only necessary member information.
- Restrict access to personal data and borrowing history.
- Define retention and archival policies with the department.
- Avoid exposing member information in publicly accessible reports.

---

## 7. Proposed Technology Stack

For an ASP.NET-based implementation:

- **Frontend:** ASP.NET Core MVC with Razor Views, HTML, CSS, Bootstrap, and JavaScript.
- **Backend:** ASP.NET Core.
- **Database:** Microsoft SQL Server.
- **ORM:** Entity Framework Core.
- **Authentication:** ASP.NET Core Identity or an equivalent secure authentication solution.
- **Email:** SMTP or a suitable transactional email provider.
- **Background Jobs:** Hosted background service or a reliable scheduling library.
- **Testing:** xUnit, with mocking where appropriate.
- **Version Control:** Git.
- **Deployment:** Department-approved server or hosting environment.

A Clean Architecture approach may be adopted if the expected project size and maintenance requirements justify the additional structure.

---

## 8. Proposed Application Pages

### Dashboard

- Summary cards.
- Recent activities.
- Overdue books.
- Quick links to issue and return books.

### Books

- `/Books/Index`
- `/Books/Create`
- `/Books/Edit/{id}`
- `/Books/Details/{id}`
- `/Books/Copies/{id}`

### Students

- `/Students/Index`
- `/Students/Create`
- `/Students/Edit/{id}`
- `/Students/Details/{id}`

### Department Fellows

- `/DepartmentFellows/Index`
- `/DepartmentFellows/Create`
- `/DepartmentFellows/Edit/{id}`
- `/DepartmentFellows/Details/{id}`

### Lending

- `/Lending/Index`
- `/Lending/Issue`
- `/Lending/Details/{id}`
- `/Lending/MemberHistory/{memberId}`

### Returns

- `/Returns/Index`
- `/Returns/Process/{transactionId}`

### Reports

- `/Reports/Inventory`
- `/Reports/IssuedBooks`
- `/Reports/OverdueBooks`
- `/Reports/MemberHistory`
- `/Reports/Transactions`

### Administration

- `/Admin/Users`
- `/Admin/LibraryPolicies`
- `/Admin/EmailTemplates`
- `/Admin/EmailLogs`
- `/Admin/AuditLogs`

These are proposed routes; the final URL structure may differ according to implementation conventions.

---

## 9. Email Processing Workflow

### Successful Return

1. Librarian confirms the return.
2. The application validates the active lending transaction.
3. A database transaction records the return and updates copy status.
4. The database transaction commits.
5. A notification job is queued.
6. The email worker sends the return confirmation.
7. The notification log records success or failure.
8. A failed email may be retried without repeating the book return.

### Overdue Reminder

1. A scheduled background job runs at configured intervals.
2. It queries active lending transactions whose due dates qualify for reminders.
3. It excludes transactions already returned.
4. It checks whether the same reminder has already been processed.
5. It queues eligible notifications.
6. The email worker sends the messages.
7. The system records each attempt and retries eligible failures.

Email sending should not be performed as an uncontrolled side effect inside a database transaction.

---

## 10. Acceptance Criteria

The initial release will be considered functionally acceptable when:

- Authorized staff can register, edit, search, and deactivate books.
- Multiple physical copies of one title can be tracked independently.
- Students and Department Fellows can be registered and managed.
- A valid lending transaction records the correct member, copy, issue date, and due date.
- An unavailable copy cannot be issued.
- Concurrent requests cannot successfully issue the same copy twice.
- A returned book updates the correct lending transaction and copy status.
- Duplicate return operations are rejected.
- Successful returns trigger the configured email notification process.
- Eligible overdue transactions are identified by scheduled processing.
- Reminder duplication is prevented according to the configured policy.
- Dashboard counts and reports reflect the underlying records.
- Historical lending information remains available after a member or book is deactivated.
- Access restrictions are enforced on protected operations.
- Database backup and recovery procedures are documented and tested.

---

## 11. Testing Strategy

### Unit Testing

Test individual business rules, such as:

- Lending eligibility.
- Due-date calculations.
- Borrowing limits.
- Return validation.
- Overdue determination.
- Notification eligibility.

### Integration Testing

Test:

- Database persistence.
- Foreign-key relationships.
- Issue and return transactions.
- Inventory updates.
- Email queue and notification logs.

### Functional Testing

Test complete user journeys:

- Register a book and its copies.
- Register a student.
- Issue a copy to the student.
- Verify the active lending record.
- Return the copy.
- Verify the copy's new status.
- Verify notification processing.

### Negative and Concurrency Testing

Test:

- Issuing a copy that is already issued.
- Issuing to an inactive member.
- Returning an already returned transaction.
- Invalid or duplicate identifiers.
- Email service failures.
- Simultaneous attempts to issue the same copy.
- Unauthorized access to restricted operations.

---

## 12. Development Roadmap

### Phase 1 — Foundation

- Confirm requirements.
- Create database schema.
- Configure authentication and roles.
- Establish application architecture.

### Phase 2 — Catalog and Members

- Implement Book Management.
- Implement Book Copy Management.
- Implement Student Management.
- Implement Department Fellow Management.

### Phase 3 — Lending and Returns

- Implement book issue.
- Implement lending history.
- Implement return processing.
- Add availability and transaction validation.

### Phase 4 — Notifications

- Configure email delivery.
- Implement return confirmation emails.
- Implement due-date reminders.
- Implement overdue processing and retries.

### Phase 5 — Dashboard and Reports

- Build dashboard summaries.
- Add inventory and overdue reports.
- Add date-range filtering and export where required.

### Phase 6 — Quality and Deployment

- Complete automated testing.
- Test security and concurrent lending.
- Configure backups and logging.
- Deploy to the approved environment.
- Prepare administrator and librarian documentation.

---

## 13. Future Enhancements

Potential future features include:

- Barcode or QR-code-based issue and return.
- Book reservations and waiting lists.
- Book renewal requests.
- Fine calculation and payment tracking, if approved.
- Member self-service portal.
- Importing book and member data from CSV or Excel.
- PDF or Excel report generation.
- Advanced audit reporting.
- Integration with the university's student information system.

---

## 14. Open Decisions Requiring Department Approval

Before final development, confirm the following:

1. Whether Department Fellows include teachers, faculty members, research fellows, or another defined group.
2. Maximum borrowing limits for each member category.
3. Lending duration for students and fellows.
4. Whether borrowing renewals are permitted.
5. Whether overdue fines apply.
6. Which email reminders are required and how frequently they should be sent.
7. Whether students and fellows need login accounts or only librarian-managed records.
8. Whether the library requires barcode scanning.
9. Which staff members may issue and return books.
10. Where the system will be hosted and who will maintain it.
11. Whether existing book and member data need to be imported.
12. How long historical records must be retained.

---

## 15. Conclusion

The CSE Department Library Management System will provide a centralized, traceable workflow for book inventory, member management, lending, returns, due-date tracking, email notifications, and reporting.

The initial implementation should prioritize data integrity, reliable lending and return operations, accurate physical-copy tracking, and secure access. Additional features should be introduced only after the department approves the relevant policies and requirements.

## 5. Recommended database architecture

এখানে relationship-গুলো একবার visualise করে নাও।

\#chatgpt-mermaid-\_r_e9\_{font-family:-apple-system-body,ui-sans-serif,-apple-system,system-ui,"Segoe UI",Helvetica,"Apple Color Emoji",Arial,sans-serif,"Segoe UI Emoji","Segoe UI Symbol";font-size:16px;fill:rgb(237, 237, 237);}@keyframes edge-animation-frame{from{stroke-dashoffset:0;}}@keyframes dash{to{stroke-dashoffset:0;}}#chatgpt-mermaid-\_r_e9\_ .edge-animation-slow{stroke-dasharray:9,5!important;stroke-dashoffset:900;animation:dash 50s linear infinite;stroke-linecap:round;}#chatgpt-mermaid-\_r_e9\_ .edge-animation-fast{stroke-dasharray:9,5!important;stroke-dashoffset:900;animation:dash 20s linear infinite;stroke-linecap:round;}#chatgpt-mermaid-\_r_e9\_ .error-icon{fill:rgb(48, 48, 48);}#chatgpt-mermaid-\_r_e9\_ .error-text{fill:rgb(237, 237, 237);stroke:rgb(237, 237, 237);}#chatgpt-mermaid-\_r_e9\_ .edge-thickness-normal{stroke-width:1px;}#chatgpt-mermaid-\_r_e9\_ .edge-thickness-thick{stroke-width:3.5px;}#chatgpt-mermaid-\_r_e9\_ .edge-pattern-solid{stroke-dasharray:0;}#chatgpt-mermaid-\_r_e9\_ .edge-thickness-invisible{stroke-width:0;fill:none;}#chatgpt-mermaid-\_r_e9\_ .edge-pattern-dashed{stroke-dasharray:3;}#chatgpt-mermaid-\_r_e9\_ .edge-pattern-dotted{stroke-dasharray:2;}#chatgpt-mermaid-\_r_e9\_ .marker{fill:rgb(175, 175, 175);stroke:rgb(175, 175, 175);}#chatgpt-mermaid-\_r_e9\_ .marker.cross{stroke:rgb(175, 175, 175);}#chatgpt-mermaid-\_r_e9\_ svg{font-family:-apple-system-body,ui-sans-serif,-apple-system,system-ui,"Segoe UI",Helvetica,"Apple Color Emoji",Arial,sans-serif,"Segoe UI Emoji","Segoe UI Symbol";font-size:16px;}#chatgpt-mermaid-\_r_e9\_ p{margin:0;}#chatgpt-mermaid-\_r_e9\_ .entityBox{fill:rgb(9, 23, 44);stroke:rgb(31, 78, 148);}#chatgpt-mermaid-\_r_e9\_ .relationshipLabelBox{fill:rgb(48, 48, 48);opacity:0.7;background-color:rgb(48, 48, 48);}#chatgpt-mermaid-\_r_e9\_ .relationshipLabelBox rect{opacity:0.5;}#chatgpt-mermaid-\_r_e9\_ .labelBkg{background-color:rgba(48, 48, 48, 0.5);}#chatgpt-mermaid-\_r_e9\_ .edgeLabel{background-color:rgb(0, 0, 0);}#chatgpt-mermaid-\_r_e9\_ .edgeLabel .label rect{fill:rgb(0, 0, 0);}#chatgpt-mermaid-\_r_e9\_ .edgeLabel .label text{fill:rgb(237, 237, 237);}#chatgpt-mermaid-\_r_e9\_ .edgeLabel .label{fill:rgb(31, 78, 148);font-size:14px;}#chatgpt-mermaid-\_r_e9\_ .label{font-family:-apple-system-body,ui-sans-serif,-apple-system,system-ui,"Segoe UI",Helvetica,"Apple Color Emoji",Arial,sans-serif,"Segoe UI Emoji","Segoe UI Symbol";color:rgb(237, 237, 237);}#chatgpt-mermaid-\_r_e9\_ .edge-pattern-dashed{stroke-dasharray:8,8;}#chatgpt-mermaid-\_r_e9\_ .node rect,#chatgpt-mermaid-\_r_e9\_ .node circle,#chatgpt-mermaid-\_r_e9\_ .node ellipse,#chatgpt-mermaid-\_r_e9\_ .node polygon{fill:rgb(9, 23, 44);stroke:rgb(31, 78, 148);stroke-width:1px;}#chatgpt-mermaid-\_r_e9\_ .relationshipLine{stroke:rgb(175, 175, 175);stroke-width:1px;fill:none;}#chatgpt-mermaid-\_r_e9\_ .marker{fill:none!important;stroke:rgb(175, 175, 175)!important;stroke-width:1;}#chatgpt-mermaid-\_r_e9\_ [data-look=neo].labelBkg{background-color:rgba(48, 48, 48, 0.5);}#chatgpt-mermaid-\_r_e9\_ .node .neo-node{stroke:rgb(31, 78, 148);}#chatgpt-mermaid-\_r_e9\_ [data-look="neo"].node rect,#chatgpt-mermaid-\_r_e9\_ [data-look="neo"].cluster rect,#chatgpt-mermaid-\_r_e9\_ [data-look="neo"].node polygon{stroke:url(#chatgpt-mermaid-\_r_e9\_-gradient);filter:drop-shadow( 1px 2px 2px rgba(185,185,185,1));}#chatgpt-mermaid-\_r_e9\_ [data-look="neo"].swimlane.cluster rect{filter:none;}#chatgpt-mermaid-\_r_e9\_ [data-look="neo"].node path{stroke:url(#chatgpt-mermaid-\_r_e9\_-gradient);stroke-width:1px;}#chatgpt-mermaid-\_r_e9\_ [data-look="neo"].node .outer-path{filter:drop-shadow( 1px 2px 2px rgba(185,185,185,1));}#chatgpt-mermaid-\_r_e9\_ [data-look="neo"].node .neo-line path{stroke:rgb(31, 78, 148);filter:none;}#chatgpt-mermaid-\_r_e9\_ [data-look="neo"].node circle{stroke:url(#chatgpt-mermaid-\_r_e9\_-gradient);filter:drop-shadow( 1px 2px 2px rgba(185,185,185,1));}#chatgpt-mermaid-\_r_e9\_ [data-look="neo"].node circle .state-start{fill:#000000;}#chatgpt-mermaid-\_r_e9\_ [data-look="neo"].icon-shape .icon{fill:url(#chatgpt-mermaid-\_r_e9\_-gradient);filter:drop-shadow( 1px 2px 2px rgba(185,185,185,1));}#chatgpt-mermaid-\_r_e9\_ [data-look="neo"].icon-shape .icon-neo path{stroke:url(#chatgpt-mermaid-\_r_e9\_-gradient);filter:drop-shadow( 1px 2px 2px rgba(185,185,185,1));}#chatgpt-mermaid-\_r_e9\_ :root{--mermaid-font-family:-apple-system-body,ui-sans-serif,-apple-system,system-ui,"Segoe UI",Helvetica,"Apple Color Emoji",Arial,sans-serif,"Segoe UI Emoji","Segoe UI Symbol";}BOOKSintBookIdPKstringISBNstringTitleboolIsActiveBOOK_COPIESintBookCopyIdPKintBookIdFKstringAccessionNumberstringCopyStatusBOOK_AUTHORSAUTHORSMEMBERSintMemberIdPKstringMemberCodeUKstringFullNamestringMemberTypestringEmailboolIsActiveLENDING_TRANSACTIONSintLendingTransactionIdPKintBookCopyIdFKintMemberIdFKdatetimeIssueDatedatetimeDueDatedatetimeReturnDatestringTransactionStatusUSERSEMAIL_NOTIFICATION_LOGSintNotificationIdPKintLendingTransactionIdFKstringNotificationTypestringDeliveryStatuscontainshaswritesborrowsappears_inprocessestriggers

## 6. আমার মতে development-এর সঠিক order

একসঙ্গে সব module তৈরি করতে যেও না। এই ক্রমে এগোলে debugging এবং database design অনেক সহজ হবে।

1. Database + Authentication — `Books`, `BookCopies`, `Members`, `Users` এবং relationships।
2. CRUD Modules — Books, Students, Department Fellows।
3. Lending Module — copy availability, issue date, due date এবং borrowing history।
4. Return Module — transaction validation এবং inventory restoration।
5. Email + Background Job — return confirmation, due-date reminder এবং overdue reminder।
6. Dashboard + Reports + Testing — সবশেষে reporting এবং edge cases।

### 7. এখন তোমার কাছ থেকে যে ৫টি সিদ্ধান্ত দরকার

SRS-টি আরও নির্ভুল করতে নিচের বিষয়গুলো select করো। যেগুলো এখনো ঠিক করোনি, সেগুলো ফাঁকা রাখতে পারো।

1\. Technology stack

ASP.NET Core MVC + SQL Server

ASP.NET Core Web API + separate frontend

এখনো সিদ্ধান্ত নিইনি

2\. Student / Fellow access

শুধু Librarian/Admin পরিচালনা করবে

Student/Fellow login করে নিজের বই ও due date দেখবে

দুটিই থাকবে

3\. Book tracking

প্রতিটি physical copy-এর আলাদা Accession Number থাকবে (recommended)

শুধু মোট quantity track করব

4\. Fine policy

প্রথম version-এ fine থাকবে না

Overdue fine calculation লাগবে

পরে সিদ্ধান্ত নেব

5\. Email reminder schedule

Due date-এর ৩ দিন আগে + due date + overdue reminder

Due date + overdue reminder

Admin নিজে reminder schedule configure করবে

Finalize requirements

তোমার জন্য সবচেয়ে গুরুত্বপূর্ণ design principle হলো: Book হলো title, BookCopy হলো physical copy, আর LendingTransaction হলো ধার দেওয়ার ইতিহাস। এই তিনটি entity ঠিকভাবে design করতে পারলে পরবর্তী CRUD, return এবং email modules অনেক বেশি নির্ভরযোগ্য হবে।


## 8. Here is my Ans:
Refine the CSE Department Library Management System SRS and create the next implementation-ready specification based on these decisions:
Technology: ASP.NET Core MVC + SQL Server
Member access: Both
Book tracking: Individual copies
Fine policy: No fine initially
Email reminders: 3 days before + due date + overdue
Include finalized business rules, database schema, entity relationships, validation rules, and implementation sequence.

## 9. Work Flow:
# CSE Department Library Management System

Implementation-Ready Specification · Version 1.1 · ASP.NET Core MVC + SQL Server

তোমার নির্বাচিত সিদ্ধান্ত অনুযায়ী এখন system-টিকে এমনভাবে design করব, যাতে সরাসরি Database Schema → Entity Models → EF Core Migrations → Services → Controllers → Razor Views → Background Jobs → Testing পর্যায়ে development শুরু করা যায়।

প্রথম সংস্করণে থাকবে:

- Librarian/Admin-এর জন্য complete management system।
- Student ও Department Fellow-এর জন্য self-service portal।
- প্রতিটি physical book copy-এর unique Accession Number।
- Book issue, return এবং borrowing history।
- Return confirmation email।
- Due date-এর ৩ দিন আগে, due date-এ এবং overdue অবস্থায় email reminder।
- Fine calculation ছাড়াই overdue tracking।
- Secure authentication, role-based authorization এবং audit trail।

## 1. Finalized system architecture

Presentation Layer

ASP.NET Core MVC · Razor Views · Bootstrap

Admin/Librarian Dashboard + Member Portal

Application / Business Layer

Book · Member · Lending · Return · Notification · Reporting Services

Data Layer

Entity Framework Core · SQL Server

Transactions · Constraints · Indexes · Migrations

Background Processing

Scheduled due-date checks · Email queue · Retry · Notification logs

Recommended architecture: ASP.NET Core MVC + EF Core, with a service layer for business rules. Email scheduling can initially use a hosted background service; for reliable retries and persistent jobs, use a SQL-backed job scheduler or a transactional outbox design.

## 2. Final user roles and access permissions

| Feature                            | Admin | Librarian             | Student | Fellow |
| ---------------------------------- | ----- | --------------------- | ------- | ------ |
| Manage books and copies            | Yes   | Yes                   | No      | No     |
| Manage member registrations        | Yes   | Yes                   | No      | No     |
| Issue and return books             | Yes   | Yes                   | No      | No     |
| View all lending records           | Yes   | Yes                   | No      | No     |
| View own borrowing history         | Yes   | Yes                   | Yes     | Yes    |
| View own due dates                 | Yes   | Yes                   | Yes     | Yes    |
| View own notification history      | Yes   | Yes                   | Yes     | Yes    |
| Manage staff accounts and policies | Yes   | No                    | No      | No     |
| View audit logs                    | Yes   | Limited if authorized | No      | No     |

### Member portal requirements

Students and fellows will be able to:

- Sign in securely.
- View their profile.
- See currently borrowed books and due dates.
- View returned books in their own history.
- See whether any book is overdue.
- View notification status for their own transactions.
- Update permitted profile fields, such as contact details, if enabled by the department.

They will not be able to mark a book as returned themselves. Only authorized staff can complete the official return transaction.

Member accounts must be linked to their registered member records. A user must never gain access to another member's information simply by changing a URL or supplying a different member ID.

## 3. Finalized business rules

এগুলো হবে system-এর authoritative business rules। Controller-এ আলাদা আলাদা করে logic না লিখে service layer-এ enforce করবে।

| ID    | Rule                                                                                                   |
| ----- | ------------------------------------------------------------------------------------------------------ |
| BR-01 | প্রতিটি book title-এর এক বা একাধিক physical copy থাকতে পারে।                                           |
| BR-02 | প্রতিটি physical copy-এর unique `AccessionNumber` থাকবে।                                               |
| BR-03 | শুধু active Student/Fellow নতুন book borrow করতে পারবে।                                                |
| BR-04 | একটি physical copy একই সময়ে সর্বোচ্চ একজন member-এর কাছে issue থাকতে পারবে।                            |
| BR-05 | Lending duration ও maximum borrowing limit member category অনুযায়ী configurable হবে।                   |
| BR-06 | Due date issue date-এর আগে হতে পারবে না।                                                               |
| BR-07 | Return হলে transaction-এ `ReturnDate` সংরক্ষিত হবে; ইতিহাস মুছে যাবে না।                               |
| BR-08 | বই ভালো অবস্থায় ফেরত এলে copy আবার `Available` হবে। Damaged হলে `Damaged` থাকবে।                       |
| BR-09 | Overdue হলে fine তৈরি হবে না; শুধু overdue status ও reminder প্রযোজ্য হবে।                             |
| BR-10 | Due date-এর ৩ দিন আগে, due date-এ এবং overdue অবস্থায় reminder যাবে।                                   |
| BR-11 | একই transaction-এর একই reminder stage স্বাভাবিকভাবে একবারই পাঠানো হবে; failed delivery retry করা যাবে। |
| BR-12 | Email failure হলেও issue/return transaction rollback হবে না।                                           |
| BR-13 | Active lending record থাকা অবস্থায় copy বা member delete করা যাবে না। Deactivate/archive করতে হবে।     |
| BR-14 | Member portal কেবল authenticated member-এর নিজের তথ্য দেখাবে।                                          |
| BR-15 | Issue/return-এর database updates atomic হবে; আংশিক update গ্রহণযোগ্য নয়।                               |
| BR-16 | একই copy নিয়ে concurrent issue request এলে কেবল একটি request সফল হবে।                                  |
| BR-17 | Lending policy পরিবর্তন করলে existing transaction-এর due date স্বয়ংক্রিয়ভাবে পরিবর্তন হবে না।          |
| BR-18 | Return date-এর পরে overdue reminder তৈরি করা যাবে না।                                                  |

### Book availability গণনা

প্রতিটি `Book`-এর জন্য:

- Total Copies: ওই title-এর সব physical copy।
- Available Copies: যেসব copy বর্তমানে lending-এর উপযোগী ও issue করা হয়নি।
- Issued Copies: যেসব copy-এর active lending transaction রয়েছে।
- Unavailable Copies: Lost, Damaged, Maintenance বা Retired copy।

অর্থাৎ:

\\[ \text{Total Copies}=\text{Available}+\text{Issued}+\text{Unavailable} \\]

এগুলো শুরুতে database-এ আলাদা editable counter হিসেবে না রেখে `BookCopies` থেকে গণনা করা নিরাপদ।

## 4. Database schema — SQL Server

প্রস্তাবিত schema-তে Identity authentication এবং library business data আলাদা থাকবে। ASP.NET Core Identity ব্যবহার করলে নিজে থেকে password management তৈরি করতে হবে না।

নিচে মূল tables, গুরুত্বপূর্ণ columns এবং constraints দেওয়া হলো। এটি implementation blueprint; final SQL migrations তৈরি করার আগে .NET/EF Core version ও namespace নির্ধারণ করতে হবে।

### 4.1 Entity inventory

Catalog

`Books`, `BookCopies`, `Categories`, `Authors`, `BookAuthors`

Book metadata, category, authors, cover image and copy-level inventory.

Members

`Members`, `Students`, `DepartmentFellows`

Common identity plus category-specific institutional information.

Circulation

`LendingTransactions`, `LibraryPolicies`

Issue/return lifecycle, due dates and category-based borrowing policies.

Background processing

`EmailNotificationLogs` and, preferably, `OutboxMessages`

Scheduled reminders, email delivery tracking, retries and reliable dispatch.

Security and administration

ASP.NET Identity tables, `AuditLogs`

Accounts, roles, authorization, audit trail and staff activity.

### 4.2 Core tables and columns

#### `Books`

| Column            | SQL type         | Rule                                              |
| ----------------- | ---------------- | ------------------------------------------------- |
| `BookId`          | `int`            | Primary key, identity                             |
| `ISBN`            | `nvarchar(20)`   | Nullable                                          |
| `Title`           | `nvarchar(250)`  | Required                                          |
| `Publisher`       | `nvarchar(200)`  | Nullable                                          |
| `Edition`         | `nvarchar(50)`   | Nullable                                          |
| `PublicationYear` | `smallint`       | Nullable, validated                               |
| `CategoryId`      | `int`            | Foreign key, nullable if uncategorized is allowed |
| `CoverImagePath`  | `nvarchar(500)`  | Nullable                                          |
| `Description`     | `nvarchar(2000)` | Nullable                                          |
| `IsActive`        | `bit`            | Required, default true                            |
| `CreatedAtUtc`    | `datetime2`      | Required                                          |
| `UpdatedAtUtc`    | `datetime2`      | Nullable                                          |

#### `BookCopies`

| Column            | SQL type        | Rule                 |
| ----------------- | --------------- | -------------------- |
| `BookCopyId`      | `int`           | Primary key          |
| `BookId`          | `int`           | Required foreign key |
| `AccessionNumber` | `nvarchar(50)`  | Required, unique     |
| `ShelfLocation`   | `nvarchar(100)` | Nullable             |
| `AcquisitionDate` | `date`          | Nullable             |
| `CopyStatus`      | `tinyint`       | Required enum        |
| `Notes`           | `nvarchar(500)` | Nullable             |
| `CreatedAtUtc`    | `datetime2`     | Required             |

Suggested `CopyStatus` values:

```
public enum BookCopyStatus : byte
{
    Available = 1,
    Issued = 2,
    Damaged = 3,
    Lost = 4,
    Maintenance = 5,
    Retired = 6
}
```

A copy cannot be issued if its status is anything other than `Available`.

#### `Members`

| Column           | SQL type        | Rule                          |
| ---------------- | --------------- | ----------------------------- |
| `MemberId`       | `int`           | Primary key                   |
| `MemberCode`     | `nvarchar(30)`  | Required, unique              |
| `FullName`       | `nvarchar(150)` | Required                      |
| `MemberType`     | `tinyint`       | Student or Fellow             |
| `Email`          | `nvarchar(254)` | Required for portal/email use |
| `Phone`          | `nvarchar(30)`  | Nullable                      |
| `IsActive`       | `bit`           | Required                      |
| `IdentityUserId` | `nvarchar(450)` | Nullable, unique when present |
| `CreatedAtUtc`   | `datetime2`     | Required                      |

`IdentityUserId` will reference the Identity user account. One member may have at most one linked portal account.

#### `Students`

| Column            | SQL type        | Rule                          |
| ----------------- | --------------- | ----------------------------- |
| `MemberId`        | `int`           | Primary key and FK to Members |
| `Department`      | `nvarchar(100)` | Required                      |
| `AcademicSession` | `nvarchar(30)`  | Required                      |
| `EnrollmentDate`  | `date`          | Nullable                      |

#### `DepartmentFellows`

| Column            | SQL type        | Rule                          |
| ----------------- | --------------- | ----------------------------- |
| `MemberId`        | `int`           | Primary key and FK to Members |
| `Designation`     | `nvarchar(100)` | Required                      |
| `Department`      | `nvarchar(100)` | Required                      |
| `InstitutionalId` | `nvarchar(50)`  | Nullable, unique if supplied  |

Use a common `Members` record for identity and access, with a matching row in exactly one subtype table. Enforce this invariant through the application service and, if needed, database-level validation.

#### `LendingTransactions`

| Column                 | SQL type        | Rule                         |
| ---------------------- | --------------- | ---------------------------- |
| `LendingTransactionId` | `bigint`        | Primary key                  |
| `BookCopyId`           | `int`           | Required FK                  |
| `MemberId`             | `int`           | Required FK                  |
| `IssueDateUtc`         | `datetime2`     | Required                     |
| `DueDate`              | `date`          | Required                     |
| `ReturnDateUtc`        | `datetime2`     | Nullable                     |
| `TransactionStatus`    | `tinyint`       | Issued or Returned           |
| `IssuedByUserId`       | `nvarchar(450)` | Required FK to Identity user |
| `ReturnedByUserId`     | `nvarchar(450)` | Nullable FK to Identity user |
| `ReturnCondition`      | `tinyint`       | Nullable until returned      |
| `CreatedAtUtc`         | `datetime2`     | Required                     |

Suggested transaction status:

```
public enum LendingStatus : byte
{
    Issued = 1,
    Returned = 2
}
```

Overdue is not a transaction status that needs to be saved separately. It is a derived condition:

```
bool isOverdue =
    transaction.TransactionStatus == LendingStatus.Issued
    && transaction.DueDate < today;
```

Here `today` must use the library's configured local date, not a random server time zone.

### 4.3 Supporting tables

| Table                   | Purpose                                                          |
| ----------------------- | ---------------------------------------------------------------- |
| `Categories`            | Book subject/category list                                       |
| `Authors`               | Author master records                                            |
| `BookAuthors`           | Many-to-many book-author relationship                            |
| `LibraryPolicies`       | Lending duration and borrowing limits by member type             |
| `EmailNotificationLogs` | Email type, transaction, recipient, attempts and delivery status |
| `OutboxMessages`        | Durable queue of notifications waiting to be processed           |
| `AuditLogs`             | User, action, entity, timestamp and relevant change details      |
| `AspNetUsers`           | Identity accounts                                                |
| `AspNetRoles`           | Identity roles                                                   |
| `AspNetUserRoles`       | Account-to-role mapping                                          |

For the first release, fine-related tables are intentionally omitted.

## 5. Entity relationships

\#chatgpt-mermaid-\_r_gt\_{font-family:-apple-system-body,ui-sans-serif,-apple-system,system-ui,"Segoe UI",Helvetica,"Apple Color Emoji",Arial,sans-serif,"Segoe UI Emoji","Segoe UI Symbol";font-size:16px;fill:rgb(237, 237, 237);}@keyframes edge-animation-frame{from{stroke-dashoffset:0;}}@keyframes dash{to{stroke-dashoffset:0;}}#chatgpt-mermaid-\_r_gt\_ .edge-animation-slow{stroke-dasharray:9,5!important;stroke-dashoffset:900;animation:dash 50s linear infinite;stroke-linecap:round;}#chatgpt-mermaid-\_r_gt\_ .edge-animation-fast{stroke-dasharray:9,5!important;stroke-dashoffset:900;animation:dash 20s linear infinite;stroke-linecap:round;}#chatgpt-mermaid-\_r_gt\_ .error-icon{fill:rgb(48, 48, 48);}#chatgpt-mermaid-\_r_gt\_ .error-text{fill:rgb(237, 237, 237);stroke:rgb(237, 237, 237);}#chatgpt-mermaid-\_r_gt\_ .edge-thickness-normal{stroke-width:1px;}#chatgpt-mermaid-\_r_gt\_ .edge-thickness-thick{stroke-width:3.5px;}#chatgpt-mermaid-\_r_gt\_ .edge-pattern-solid{stroke-dasharray:0;}#chatgpt-mermaid-\_r_gt\_ .edge-thickness-invisible{stroke-width:0;fill:none;}#chatgpt-mermaid-\_r_gt\_ .edge-pattern-dashed{stroke-dasharray:3;}#chatgpt-mermaid-\_r_gt\_ .edge-pattern-dotted{stroke-dasharray:2;}#chatgpt-mermaid-\_r_gt\_ .marker{fill:rgb(175, 175, 175);stroke:rgb(175, 175, 175);}#chatgpt-mermaid-\_r_gt\_ .marker.cross{stroke:rgb(175, 175, 175);}#chatgpt-mermaid-\_r_gt\_ svg{font-family:-apple-system-body,ui-sans-serif,-apple-system,system-ui,"Segoe UI",Helvetica,"Apple Color Emoji",Arial,sans-serif,"Segoe UI Emoji","Segoe UI Symbol";font-size:16px;}#chatgpt-mermaid-\_r_gt\_ p{margin:0;}#chatgpt-mermaid-\_r_gt\_ .entityBox{fill:rgb(9, 23, 44);stroke:rgb(31, 78, 148);}#chatgpt-mermaid-\_r_gt\_ .relationshipLabelBox{fill:rgb(48, 48, 48);opacity:0.7;background-color:rgb(48, 48, 48);}#chatgpt-mermaid-\_r_gt\_ .relationshipLabelBox rect{opacity:0.5;}#chatgpt-mermaid-\_r_gt\_ .labelBkg{background-color:rgba(48, 48, 48, 0.5);}#chatgpt-mermaid-\_r_gt\_ .edgeLabel{background-color:rgb(0, 0, 0);}#chatgpt-mermaid-\_r_gt\_ .edgeLabel .label rect{fill:rgb(0, 0, 0);}#chatgpt-mermaid-\_r_gt\_ .edgeLabel .label text{fill:rgb(237, 237, 237);}#chatgpt-mermaid-\_r_gt\_ .edgeLabel .label{fill:rgb(31, 78, 148);font-size:14px;}#chatgpt-mermaid-\_r_gt\_ .label{font-family:-apple-system-body,ui-sans-serif,-apple-system,system-ui,"Segoe UI",Helvetica,"Apple Color Emoji",Arial,sans-serif,"Segoe UI Emoji","Segoe UI Symbol";color:rgb(237, 237, 237);}#chatgpt-mermaid-\_r_gt\_ .edge-pattern-dashed{stroke-dasharray:8,8;}#chatgpt-mermaid-\_r_gt\_ .node rect,#chatgpt-mermaid-\_r_gt\_ .node circle,#chatgpt-mermaid-\_r_gt\_ .node ellipse,#chatgpt-mermaid-\_r_gt\_ .node polygon{fill:rgb(9, 23, 44);stroke:rgb(31, 78, 148);stroke-width:1px;}#chatgpt-mermaid-\_r_gt\_ .relationshipLine{stroke:rgb(175, 175, 175);stroke-width:1px;fill:none;}#chatgpt-mermaid-\_r_gt\_ .marker{fill:none!important;stroke:rgb(175, 175, 175)!important;stroke-width:1;}#chatgpt-mermaid-\_r_gt\_ [data-look=neo].labelBkg{background-color:rgba(48, 48, 48, 0.5);}#chatgpt-mermaid-\_r_gt\_ .node .neo-node{stroke:rgb(31, 78, 148);}#chatgpt-mermaid-\_r_gt\_ [data-look="neo"].node rect,#chatgpt-mermaid-\_r_gt\_ [data-look="neo"].cluster rect,#chatgpt-mermaid-\_r_gt\_ [data-look="neo"].node polygon{stroke:url(#chatgpt-mermaid-\_r_gt\_-gradient);filter:drop-shadow( 1px 2px 2px rgba(185,185,185,1));}#chatgpt-mermaid-\_r_gt\_ [data-look="neo"].swimlane.cluster rect{filter:none;}#chatgpt-mermaid-\_r_gt\_ [data-look="neo"].node path{stroke:url(#chatgpt-mermaid-\_r_gt\_-gradient);stroke-width:1px;}#chatgpt-mermaid-\_r_gt\_ [data-look="neo"].node .outer-path{filter:drop-shadow( 1px 2px 2px rgba(185,185,185,1));}#chatgpt-mermaid-\_r_gt\_ [data-look="neo"].node .neo-line path{stroke:rgb(31, 78, 148);filter:none;}#chatgpt-mermaid-\_r_gt\_ [data-look="neo"].node circle{stroke:url(#chatgpt-mermaid-\_r_gt\_-gradient);filter:drop-shadow( 1px 2px 2px rgba(185,185,185,1));}#chatgpt-mermaid-\_r_gt\_ [data-look="neo"].node circle .state-start{fill:#000000;}#chatgpt-mermaid-\_r_gt\_ [data-look="neo"].icon-shape .icon{fill:url(#chatgpt-mermaid-\_r_gt\_-gradient);filter:drop-shadow( 1px 2px 2px rgba(185,185,185,1));}#chatgpt-mermaid-\_r_gt\_ [data-look="neo"].icon-shape .icon-neo path{stroke:url(#chatgpt-mermaid-\_r_gt\_-gradient);filter:drop-shadow( 1px 2px 2px rgba(185,185,185,1));}#chatgpt-mermaid-\_r_gt\_ :root{--mermaid-font-family:-apple-system-body,ui-sans-serif,-apple-system,system-ui,"Segoe UI",Helvetica,"Apple Color Emoji",Arial,sans-serif,"Segoe UI Emoji","Segoe UI Symbol";}BOOKSBOOK_COPIESCATEGORIESBOOK_AUTHORSAUTHORSMEMBERSSTUDENTSDEPARTMENT_FELLOWSASP_NET_USERSLENDING_TRANSACTIONSEMAIL_NOTIFICATION_LOGSOUTBOX_MESSAGEShasclassifieslinkslinksspecializesspecializesaccountborrowshistoryprocessesgeneratesattempts

### Relationship rules

1. `Books → BookCopies`: One-to-many.
2. `Books ↔ Authors`: Many-to-many through `BookAuthors`.
3. `Members → Students/DepartmentFellows`: Category-specific one-to-one subtype relationship.
4. `Members → LendingTransactions`: One-to-many.
5. `BookCopies → LendingTransactions`: One-to-many historical transactions, but only one active transaction at a time.
6. `Members ↔ AspNetUsers`: Optional one-to-one account linkage.
7. `LendingTransactions → EmailNotificationLogs`: One-to-many notification events and attempts.

### Critical SQL Server constraint

একই physical copy যেন একসঙ্গে দুবার issue না হয়, তার জন্য filtered unique index ব্যবহার করা যায়:

```
CREATE UNIQUE INDEX UX_Lending_ActiveBookCopy
ON dbo.LendingTransactions (BookCopyId)
WHERE TransactionStatus = 1;
```

এটি ধরে নিচ্ছে `1 = Issued` এবং `dbo` schema ব্যবহার করা হয়েছে। EF Core migration-এর মাধ্যমেও একই index configure করা উচিত।

শুধু C#-এ availability check করলেই যথেষ্ট নয়—একই সময়ে দুইটি request এলে দুটিই copy-টিকে available দেখতে পারে। Database constraint এবং transactional update এই race condition ঠেকাবে।

## 6. Validation specification

Validation তিনটি স্তরে হবে: ViewModel validation → Business/service validation → Database constraints। শুধু HTML form validation-এর ওপর নির্ভর করবে না।

### 6.1 Book registration

| Field            | Validation                                                 |
| ---------------- | ---------------------------------------------------------- |
| Title            | Required, maximum 250 characters                           |
| ISBN             | Optional; supplied হলে normalized format ও duplicate check |
| Publication Year | বাস্তবসম্মত year range                                     |
| Category         | নির্বাচিত category থাকতে হবে এবং active হতে হবে            |
| Cover image      | Approved image format, size limit, safe filename           |
| Accession Number | Required, unique, trim করা value                           |
| Shelf Location   | নির্ধারিত maximum length                                   |

একটি book title register করা এবং physical copies যোগ করা আলাদা operation হতে পারে। Title তৈরি হলেই copy available ধরে নেওয়া যাবে না।

### 6.2 Member registration

- `MemberCode` required ও unique।
- Email valid format-এর হতে হবে এবং notification-এর জন্য কার্যকর হতে হবে।
- Member category শুধু অনুমোদিত enum value হতে পারবে।
- Student-এর academic session এবং Fellow-এর designation প্রয়োজন অনুযায়ী required হবে।
- Portal access দেওয়া হলে account-এর সঙ্গে সঠিক `MemberId` link করতে হবে।
- Duplicate institutional ID এবং conflicting account links প্রত্যাখ্যান করতে হবে।
- Inactive member নতুন বই borrow করতে পারবে না।

### 6.3 Issue validation

Issue service-এ এই sequence অনুসরণ করবে:

1. Member আছে এবং active কি না যাচাই।
2. Member-এর linked account বা staff operation-এর authorization যাচাই।
3. Copy আছে, active এবং `Available` কি না যাচাই।
4. Member-এর active lending count বের করা।
5. Policy অনুযায়ী borrowing limit পরীক্ষা করা।
6. Due date policy অনুযায়ী নির্ধারণ করা।
7. Transactional operation-এ copy reserve এবং lending record insert করা।
8. Database commit-এর পরে notification queue করা।

Policy অনুযায়ী সর্বোচ্চ ৩টি বই ধার দেওয়া যায়—এটি শুধু উদাহরণ; actual limit `LibraryPolicies` থেকে আসবে।

### 6.4 Return validation

- Transaction অবশ্যই `Issued` অবস্থায় থাকতে হবে।
- Return timestamp একবারই set করা যাবে।
- Receiving librarian authenticated ও authorized হতে হবে।
- Return condition নির্ধারণ করতে হবে।
- `Good` হলে copy `Available`; `Damaged` হলে `Damaged` status হবে।
- Return operation-এর transaction commit না হলে email success confirmation পাঠানো যাবে না।
- একই transaction-এ duplicate return request এলে দ্বিতীয়টি reject করতে হবে।

### 6.5 Member portal security

Portal-এর controller action-এ URL-এর member ID বিশ্বাস করবে না।

উদাহরণ: Student A যদি `/MyLibrary/History?memberId=25` পরিবর্তন করে অন্য member-এর ID দেয়, service অবশ্যই authenticated user's linked member record ব্যবহার করবে।

```
var memberId = await memberAccountService
    .GetCurrentMemberIdAsync(User);

var history = await lendingService
    .GetMemberHistoryAsync(memberId);
```

`memberAccountService` ও `lendingService` হলো প্রস্তাবিত application services; implementation-এ interface এবং dependency injection ব্যবহার করা যেতে পারে।

## 7. Email and overdue processing design

Email module-টি reliable না হলে reminder কখনো miss হতে পারে অথবা একই email বারবার চলে যেতে পারে। তাই notification-এর জন্য persisted state দরকার।

### 7.1 Reminder schedule

| Stage      | Eligibility                               | Action                |
| ---------- | ----------------------------------------- | --------------------- |
| Before due | Due date আজ থেকে ৩ দিন পরে                | Upcoming reminder     |
| Due today  | Due date আজ                               | Due-date reminder     |
| Overdue    | Due date পার হয়েছে এবং transaction active | Overdue reminder      |
| Returned   | Transaction returned                      | Stop future reminders |

Bangladesh-এর library time zone হিসেবে `Asia/Dhaka` ব্যবহার করা যায়। Database-এ timestamps UTC-তে রাখবে; due date হলো library-local calendar date।

Overdue reminder-এর repeat frequency department policy দিয়ে ঠিক করতে হবে। প্রথম সংস্করণে প্রস্তাব: প্রথম overdue email due date-এর পরের দিন, এরপর প্রতি ৭ দিনে একবার, যত দিন book unreturned থাকবে। এই repeat policy অনুমোদনসাপেক্ষ; মূল requirement-এর ৩ দিন আগে এবং due date-এর reminder এতে অপরিবর্তিত।

### 7.2 Notification log

প্রস্তাবিত fields:

- `EmailNotificationLogId`
- `LendingTransactionId`
- `NotificationType`
- `RecipientEmail`
- `ScheduledForUtc`
- `SentAtUtc`
- `DeliveryStatus`
- `AttemptCount`
- `LastError`
- `IdempotencyKey`

`NotificationType` উদাহরণ: `DueSoon`, `DueToday`, `Overdue`, `ReturnConfirmation`।

Database-এ `IdempotencyKey` unique রাখলে একই reminder event একাধিকবার queue হওয়ার ঝুঁকি কমে।

### 7.3 Recommended processing workflow

Scheduled Background Job

Find eligible lending transactions

Create Outbox Message

Persist event with idempotency key

Email Worker

Send email and handle transient failures

Update Delivery Log

Sent / Failed / Retry scheduled

একটি বাস্তব সীমাবদ্ধতা মনে রাখতে হবে: SMTP server email গ্রহণ করেছে মানেই recipient-এর inbox-এ পৌঁছেছে—এটি নিশ্চিত নয়। Delivery status-এ তাই `AcceptedByProvider` বা `Sent` ব্যবহার করা উচিত; verified delivery receipt না থাকলে `Delivered` দাবি করা যাবে না।

## 8. ASP.NET Core MVC project structure

শুরুতেই সব code `Controllers` বা `DbContext`-এ লিখবে না। এতে পরে lending, return এবং reminder logic maintain করা কঠিন হবে।

একটি practical structure:

```

CSELibraryManagement/
├── Controllers/
│   ├── BooksController.cs
│   ├── BookCopiesController.cs
│   ├── StudentsController.cs
│   ├── DepartmentFellowsController.cs
│   ├── LendingController.cs
│   ├── ReturnsController.cs
│   ├── ReportsController.cs
│   └── MyLibraryController.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   └── Configurations/
├── Models/
│   ├── Books/
│   ├── Members/
│   ├── Lending/
│   └── Notifications/
├── ViewModels/
│   ├── Books/
│   ├── Members/
│   ├── Lending/
│   └── Reports/
├── Services/
│   ├── BookService.cs
│   ├── MemberService.cs
│   ├── LendingService.cs
│   ├── ReturnService.cs
│   └── NotificationService.cs
├── BackgroundJobs/
│   ├── DueDateReminderWorker.cs
│   └── OutboxProcessor.cs
├── Views/
│   ├── Books/
│   ├── BookCopies/
│   ├── Students/
│   ├── DepartmentFellows/
│   ├── Lending/
│   ├── Returns/
│   ├── Reports/
│   └── MyLibrary/
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── images/
├── Migrations/
├── Program.cs
└── appsettings.json

```

এটি single-project MVC application-এর structure। Project বড় হলে পরে `Domain`, `Application`, `Infrastructure` এবং `Web` আলাদা projects-এ ভাগ করা যাবে।

## 9. Implementation sequence

নিচের development plan অনুসরণ করলে এক ধাপের কাজ verify করে পরের ধাপে যেতে পারবে।

## 01

Foundation and Authentication

First

- Create ASP.NET Core MVC project.
- Configure SQL Server and EF Core.
- Add Identity, Admin and Librarian roles.
- Create `ApplicationDbContext`.
- Add migrations and verify the database.

## 02

Catalog and Member CRUD

- Categories, Authors, Books, BookCopies.
- Members, Students, DepartmentFellows.
- Registration, Index, Details, Edit and Deactivate.
- Image upload and validation.

## 03

Lending and Returns

- Library policy configuration.
- Issue transaction and due date calculation.
- Member borrowing history.
- Return transaction and copy-condition handling.
- Database concurrency protection.

## 04

Member Portal

- Link member records to Identity accounts.
- Show own active loans, due dates and history.
- Enforce authorization on every query.
- Add account activation and recovery workflows.

## 05

Email and Background Jobs

- Return confirmation.
- Reminder scheduling and duplicate prevention.
- Outbox processing and retry policy.
- Notification logs and failure monitoring.

## 06

Dashboard, Reports and Deployment

- Inventory and overdue reports.
- Search, pagination and exports.
- Automated tests, backup and restore.
- Production configuration and deployment.

## 10. Acceptance criteria for the first release

Implementation শেষ হয়েছে বলা যাবে যখন নিচের গুরুত্বপূর্ণ test cases pass করবে।

### Release checklist

0 / 14

Admin এবং Librarian শুধু অনুমোদিত operations চালাতে পারে

একটি title-এর একাধিক physical copy independently track হয়

একটি active copy দ্বিতীয়বার issue করা যায় না

Inactive member-কে book issue করা যায় না

Member-এর borrowing limit অতিক্রম করা যায় না

Return করলে transaction history অক্ষত থাকে এবং copy status সঠিক হয়

একটি transaction দ্বিতীয়বার return করা যায় না

Student অন্য Student-এর history দেখতে পারে না

Return email queue হয়, কিন্তু email failure return rollback করে না

Due-date reminder সঠিক local date-এ তৈরি হয়

Overdue reminder returned books-কে বাদ দেয়

Duplicate reminder ও concurrent issue-এর বিরুদ্ধে database protection থাকে

Dashboard-এর copy counts database-এর বাস্তব অবস্থার সঙ্গে মেলে

Backup থেকে database restore করার procedure test করা হয়েছে

## 11. প্রথমে কোন কাজটি করবে?

আমার recommendation হলো, আগে database ও entity design final করো; তারপর CRUD তৈরি করো। কারণ book copy এবং lending relationship ভুল হলে পরে return, availability ও reminder logic পুনরায় লিখতে হবে।

পরবর্তী development deliverable হিসেবে সবচেয়ে উপযোগী হবে:

Deliverable 1 — Complete EF Core Entity Models

সব entity, enums, navigation properties, validation attributes এবং relationships।

Generate Entity Models

Deliverable 2 — Lending Service

Atomic issue/return operations, concurrency protection, due-date calculation এবং xUnit test cases।

Generate Lending Services

Deliverable 3 — Complete CRUD UI

Books, Students, Fellows, Index pages, registration forms, image upload ও validation।

Generate CRUD Implementation

আমরা এই project-টিকে এখন থেকে একটি real implementation project হিসেবে এগিয়ে নিতে পারি—প্রথমে database entities ও constraints, তারপর business services, এরপর MVC pages এবং সবশেষে scheduled email processing। এতে projectটি শুধু demo CRUD application না হয়ে maintainable departmental library system হবে।
