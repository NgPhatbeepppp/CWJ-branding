using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cw.Branding.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSelectedProductFromContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEDMGYr/bDfZ+9gExpfKsVqhnND+XAWkxFLhydkBZvNtlmyNKS9s7LuyTjy22NIFR7g==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEBlX/pSVUStEn3gywqeVvTjUQmqpTDNOMdWad+zUA0xMaLvmy46+/3BlJ5u8kOCwPQ==");
        }
    }
}
