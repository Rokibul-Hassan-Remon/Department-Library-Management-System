using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CseLibrary.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLibraryCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "Authors_HiLo",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "BookCopies_HiLo",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "Books_HiLo",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "Categories_HiLo",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "EmailNotificationLogs_HiLo",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "LendingTransactions_HiLo",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "LibraryMembers_HiLo",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "LibraryPolicies_HiLo",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                    table.CheckConstraint("CK_Authors_Status", "[Status] IN (1, 2, 3)");
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.CheckConstraint("CK_Categories_Status", "[Status] IN (1, 2, 3)");
                });

            migrationBuilder.CreateTable(
                name: "LibraryMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    MemberCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MemberType = table.Column<byte>(type: "tinyint", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    IdentityUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Approval = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryMembers", x => x.Id);
                    table.CheckConstraint("CK_LibraryMembers_Approval", "[Approval] IN (1, 2, 3)");
                    table.CheckConstraint("CK_LibraryMembers_MemberType", "[MemberType] IN (1, 2)");
                    table.CheckConstraint("CK_LibraryMembers_Status", "[Status] IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_LibraryMembers_AspNetUsers_IdentityUserId",
                        column: x => x.IdentityUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LibraryPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    MemberCategory = table.Column<byte>(type: "tinyint", nullable: false),
                    MaxActiveLoans = table.Column<int>(type: "int", nullable: false),
                    LoanDays = table.Column<int>(type: "int", nullable: false),
                    MaxRenewals = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryPolicies", x => x.Id);
                    table.CheckConstraint("CK_LibraryPolicies_Status", "[Status] IN (1, 2, 3)");
                    table.CheckConstraint("CK_LibraryPolicies_Values", "[MaxActiveLoans] >= 0 AND [LoanDays] >= 1 AND [MaxRenewals] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Isbn = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Publisher = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Edition = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PublicationYear = table.Column<short>(type: "smallint", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                    table.CheckConstraint("CK_Books_PublicationYear", "[PublicationYear] IS NULL OR [PublicationYear] BETWEEN 1000 AND 9999");
                    table.CheckConstraint("CK_Books_Status", "[Status] IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_Books_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BookAuthors",
                columns: table => new
                {
                    BookId = table.Column<int>(type: "int", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookAuthors", x => new { x.BookId, x.AuthorId });
                    table.ForeignKey(
                        name: "FK_BookAuthors_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookAuthors_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BookCopies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    AccessionNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ShelfLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AcquisitionDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CopyStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookCopies", x => x.Id);
                    table.CheckConstraint("CK_BookCopies_CopyStatus", "[CopyStatus] BETWEEN 1 AND 6");
                    table.CheckConstraint("CK_BookCopies_Status", "[Status] IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_BookCopies_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LendingTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    LibraryMemberId = table.Column<int>(type: "int", nullable: false),
                    BookCopyId = table.Column<int>(type: "int", nullable: false),
                    IssueDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ReturnDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TransactionStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    IssuedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ReturnedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ReturnCondition = table.Column<byte>(type: "tinyint", nullable: true),
                    RenewCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LendingTransactions", x => x.Id);
                    table.CheckConstraint("CK_LendingTransactions_ReturnCondition", "[ReturnCondition] IS NULL OR [ReturnCondition] IN (1, 2)");
                    table.CheckConstraint("CK_LendingTransactions_Status", "[Status] IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_LendingTransactions_AspNetUsers_IssuedByUserId",
                        column: x => x.IssuedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LendingTransactions_AspNetUsers_ReturnedByUserId",
                        column: x => x.ReturnedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LendingTransactions_BookCopies_BookCopyId",
                        column: x => x.BookCopyId,
                        principalTable: "BookCopies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LendingTransactions_LibraryMembers_LibraryMemberId",
                        column: x => x.LibraryMemberId,
                        principalTable: "LibraryMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmailNotificationLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    LendingTransactionId = table.Column<int>(type: "int", nullable: false),
                    EmailType = table.Column<byte>(type: "tinyint", nullable: false),
                    Recipient = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    DeliveryStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailNotificationLogs", x => x.Id);
                    table.CheckConstraint("CK_EmailNotificationLogs_Status", "[Status] IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_EmailNotificationLogs_LendingTransactions_LendingTransactionId",
                        column: x => x.LendingTransactionId,
                        principalTable: "LendingTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Authors_Name",
                table: "Authors",
                column: "Name",
                unique: true,
                filter: "[Status] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_BookAuthors_AuthorId",
                table: "BookAuthors",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_BookCopies_AccessionNumber",
                table: "BookCopies",
                column: "AccessionNumber",
                unique: true,
                filter: "[Status] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_BookCopies_BookId",
                table: "BookCopies",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_CategoryId",
                table: "Books",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_Isbn",
                table: "Books",
                column: "Isbn",
                unique: true,
                filter: "[Status] <> 3 AND [Isbn] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Books_Title",
                table: "Books",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true,
                filter: "[Status] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotificationLogs_IdempotencyKey",
                table: "EmailNotificationLogs",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailNotificationLogs_LendingTransactionId",
                table: "EmailNotificationLogs",
                column: "LendingTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_LendingTransactions_DueDate",
                table: "LendingTransactions",
                column: "DueDate",
                filter: "[TransactionStatus] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_LendingTransactions_IssuedByUserId",
                table: "LendingTransactions",
                column: "IssuedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LendingTransactions_LibraryMemberId_TransactionStatus",
                table: "LendingTransactions",
                columns: new[] { "LibraryMemberId", "TransactionStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_LendingTransactions_ReturnedByUserId",
                table: "LendingTransactions",
                column: "ReturnedByUserId");

            migrationBuilder.CreateIndex(
                name: "UX_Lending_ActiveBookCopy",
                table: "LendingTransactions",
                column: "BookCopyId",
                unique: true,
                filter: "[TransactionStatus] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryMembers_FullName",
                table: "LibraryMembers",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryMembers_IdentityUserId",
                table: "LibraryMembers",
                column: "IdentityUserId",
                unique: true,
                filter: "[IdentityUserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryMembers_MemberCode",
                table: "LibraryMembers",
                column: "MemberCode",
                unique: true,
                filter: "[Status] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryPolicies_MemberCategory",
                table: "LibraryPolicies",
                column: "MemberCategory",
                unique: true,
                filter: "[Status] <> 3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookAuthors");

            migrationBuilder.DropTable(
                name: "EmailNotificationLogs");

            migrationBuilder.DropTable(
                name: "LibraryPolicies");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "LendingTransactions");

            migrationBuilder.DropTable(
                name: "BookCopies");

            migrationBuilder.DropTable(
                name: "LibraryMembers");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropSequence(
                name: "Authors_HiLo");

            migrationBuilder.DropSequence(
                name: "BookCopies_HiLo");

            migrationBuilder.DropSequence(
                name: "Books_HiLo");

            migrationBuilder.DropSequence(
                name: "Categories_HiLo");

            migrationBuilder.DropSequence(
                name: "EmailNotificationLogs_HiLo");

            migrationBuilder.DropSequence(
                name: "LendingTransactions_HiLo");

            migrationBuilder.DropSequence(
                name: "LibraryMembers_HiLo");

            migrationBuilder.DropSequence(
                name: "LibraryPolicies_HiLo");
        }
    }
}
