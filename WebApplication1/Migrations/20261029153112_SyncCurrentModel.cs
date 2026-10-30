using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class SyncCurrentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditEntry_Users_AdminUserID",
                table: "AuditEntry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AuditEntry",
                table: "AuditEntry");

            migrationBuilder.RenameTable(
                name: "AuditEntry",
                newName: "AuditEntries");

            migrationBuilder.RenameIndex(
                name: "IX_AuditEntry_AdminUserID",
                table: "AuditEntries",
                newName: "IX_AuditEntries_AdminUserID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuditEntries",
                table: "AuditEntries",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditEntries_Users_AdminUserID",
                table: "AuditEntries",
                column: "AdminUserID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditEntries_Users_AdminUserID",
                table: "AuditEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AuditEntries",
                table: "AuditEntries");

            migrationBuilder.RenameTable(
                name: "AuditEntries",
                newName: "AuditEntry");

            migrationBuilder.RenameIndex(
                name: "IX_AuditEntries_AdminUserID",
                table: "AuditEntry",
                newName: "IX_AuditEntry_AdminUserID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AuditEntry",
                table: "AuditEntry",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditEntry_Users_AdminUserID",
                table: "AuditEntry",
                column: "AdminUserID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
