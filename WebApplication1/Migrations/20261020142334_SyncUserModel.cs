using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class SyncUserModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Users",
                newName: "UserEmail");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Users",
                newName: "UserAddress");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Users",
                newName: "UserPhone");

            migrationBuilder.RenameColumn(
                name: "PathToPicture",
                table: "Users",
                newName: "UserPfpPath");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Users",
                newName: "UserPasswordHash");

            migrationBuilder.RenameColumn(
                name: "PasswordSalted",
                table: "Users",
                newName: "UserPasswordSalt");

            migrationBuilder.RenameColumn(
                name: "Active",
                table: "Users",
                newName: "UserActiveFlag");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserEmail",
                table: "Users",
                column: "UserEmail",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_UserEmail",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "UserEmail",
                table: "Users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "UserAddress",
                table: "Users",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "UserPhone",
                table: "Users",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "UserPfpPath",
                table: "Users",
                newName: "PathToPicture");

            migrationBuilder.RenameColumn(
                name: "UserPasswordHash",
                table: "Users",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "UserPasswordSalt",
                table: "Users",
                newName: "PasswordSalted");

            migrationBuilder.RenameColumn(
                name: "UserActiveFlag",
                table: "Users",
                newName: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }
    }
}
