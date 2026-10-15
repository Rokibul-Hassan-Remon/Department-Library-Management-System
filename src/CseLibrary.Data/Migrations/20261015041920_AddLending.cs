using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CseLibrary.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLending : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "Loans_HiLo",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "Members_HiLo",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    MemberCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MemberType = table.Column<byte>(type: "tinyint", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                    table.CheckConstraint("CK_Members_MemberType", "[MemberType] IN (1, 2)");
                    table.CheckConstraint("CK_Members_Status", "[Status] IN (1, 2, 3)");
                });

            migrationBuilder.CreateTable(
                name: "Loans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    BookCopyId = table.Column<int>(type: "int", nullable: false),
                    IssuedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ReturnedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RenewCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loans", x => x.Id);
                    table.CheckConstraint("CK_Loans_Status", "[Status] IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_Loans_BookCopies_BookCopyId",
                        column: x => x.BookCopyId,
                        principalTable: "BookCopies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Loans_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Loans_DueDate",
                table: "Loans",
                column: "DueDate",
                filter: "[ReturnedAtUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_MemberId_ReturnedAtUtc",
                table: "Loans",
                columns: new[] { "MemberId", "ReturnedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "UX_Loans_ActiveCopy",
                table: "Loans",
                column: "BookCopyId",
                unique: true,
                filter: "[ReturnedAtUtc] IS NULL AND [Status] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_Members_FullName",
                table: "Members",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_Members_MemberCode",
                table: "Members",
                column: "MemberCode",
                unique: true,
                filter: "[Status] <> 3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Loans");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropSequence(
                name: "Loans_HiLo");

            migrationBuilder.DropSequence(
                name: "Members_HiLo");
        }
    }
}
