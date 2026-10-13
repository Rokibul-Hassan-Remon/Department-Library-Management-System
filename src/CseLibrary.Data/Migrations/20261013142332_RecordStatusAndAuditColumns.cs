using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CseLibrary.Data.Migrations
{
    /// <summary>
    /// D-51: IsDeleted / IsActive become one Status column (1 Active, 2 Inactive, 3 Deleted), converting the
    /// existing rows; unique indexes skip deleted rows. Hand-edited: the scaffold dropped the old columns
    /// before reading them.
    /// </summary>
    public partial class RecordStatusAndAuditColumns : Migration
    {
        private static readonly string[] SoftDeleteTables = ["Permissions", "GeneralConfigs"];
        private static readonly string[] StatusTables = ["Permissions", "GeneralConfigs", "AspNetUsers"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Permissions_Name", table: "Permissions");
            migrationBuilder.DropIndex(name: "IX_GeneralConfigs_Key", table: "GeneralConfigs");

            foreach (var table in StatusTables)
                migrationBuilder.AddColumn<byte>(name: "Status", table: table, type: "tinyint", nullable: true);

            foreach (var table in SoftDeleteTables)
                migrationBuilder.Sql($"UPDATE [{table}] SET [Status] = CASE WHEN [IsDeleted] = 1 THEN 3 ELSE 1 END;");
            migrationBuilder.Sql("UPDATE [AspNetUsers] SET [Status] = CASE WHEN [IsActive] = 1 THEN 1 ELSE 2 END;");

            foreach (var table in StatusTables)
            {
                migrationBuilder.AlterColumn<byte>(
                    name: "Status", table: table, type: "tinyint", nullable: false,
                    oldClrType: typeof(byte), oldType: "tinyint", oldNullable: true);
                migrationBuilder.AddCheckConstraint(name: $"CK_{table}_Status", table: table, sql: "[Status] IN (1, 2, 3)");
            }

            foreach (var table in SoftDeleteTables)
                migrationBuilder.DropColumn(name: "IsDeleted", table: table);
            migrationBuilder.DropColumn(name: "IsActive", table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Name", table: "Permissions", column: "Name", unique: true, filter: "[Status] <> 3");
            migrationBuilder.CreateIndex(
                name: "IX_GeneralConfigs_Key", table: "GeneralConfigs", column: "Key", unique: true, filter: "[Status] <> 3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Permissions_Name", table: "Permissions");
            migrationBuilder.DropIndex(name: "IX_GeneralConfigs_Key", table: "GeneralConfigs");

            foreach (var table in SoftDeleteTables)
                migrationBuilder.AddColumn<bool>(name: "IsDeleted", table: table, type: "bit", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<bool>(name: "IsActive", table: "AspNetUsers", type: "bit", nullable: false, defaultValue: false);

            foreach (var table in SoftDeleteTables)
                migrationBuilder.Sql($"UPDATE [{table}] SET [IsDeleted] = CASE WHEN [Status] = 3 THEN 1 ELSE 0 END;");
            migrationBuilder.Sql("UPDATE [AspNetUsers] SET [IsActive] = CASE WHEN [Status] = 1 THEN 1 ELSE 0 END;");

            foreach (var table in StatusTables)
            {
                migrationBuilder.DropCheckConstraint(name: $"CK_{table}_Status", table: table);
                migrationBuilder.DropColumn(name: "Status", table: table);
            }

            // Fails if a deleted row and a live row share a key: resolve those rows by hand before going down.
            migrationBuilder.CreateIndex(name: "IX_Permissions_Name", table: "Permissions", column: "Name", unique: true);
            migrationBuilder.CreateIndex(name: "IX_GeneralConfigs_Key", table: "GeneralConfigs", column: "Key", unique: true);
        }
    }
}
