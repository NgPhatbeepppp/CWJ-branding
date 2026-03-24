using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cw.Branding.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateContactTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsHandled",
                table: "ContactFormEntry",
                newName: "IsRead");

            migrationBuilder.RenameColumn(
                name: "HandledAt",
                table: "ContactFormEntry",
                newName: "ReadAt");

            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "ContactFormEntry",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProcessingStatus",
                table: "ContactFormEntry",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ReadByAdminId",
                table: "ContactFormEntry",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedProduct",
                table: "ContactFormEntry",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "ContactFormEntry");

            migrationBuilder.DropColumn(
                name: "ProcessingStatus",
                table: "ContactFormEntry");

            migrationBuilder.DropColumn(
                name: "ReadByAdminId",
                table: "ContactFormEntry");

            migrationBuilder.DropColumn(
                name: "SelectedProduct",
                table: "ContactFormEntry");

            migrationBuilder.RenameColumn(
                name: "ReadAt",
                table: "ContactFormEntry",
                newName: "HandledAt");

            migrationBuilder.RenameColumn(
                name: "IsRead",
                table: "ContactFormEntry",
                newName: "IsHandled");
        }
    }
}
