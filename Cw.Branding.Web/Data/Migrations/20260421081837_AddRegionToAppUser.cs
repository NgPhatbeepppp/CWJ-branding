using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cw.Branding.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRegionToAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Region",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash", "Region" },
                values: new object[] { new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "AQAAAAIAAYagAAAAEOKnYKC5Yh36RV/tRN5jhIaesvmRCTjd2v3/HPaOs94eonYu0APkJzmDlfdL/tNyZA==", 2 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Region",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "PasswordHash" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "AQAAAAIAAYagAAAAEMrh6Ebnav9RVcliTukHoaey8lv1Rcp+VcmZ7in+PW0ziD72FFEtqYq8Ivxi3CX7Ww==" });
        }
    }
}
