using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cw.Branding.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHeroAndHomeSlide : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HeroSection",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BackgroundImage = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleVi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HighlightEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HighlightVi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DescriptionEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DescriptionVi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Cta1TextEn = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Cta1TextVi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Cta1Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cta2TextEn = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Cta2TextVi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Cta2Url = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeroSection", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HomeSlide",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeSlide", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "HeroSection",
                columns: new[] { "Id", "BackgroundImage", "Cta1TextEn", "Cta1TextVi", "Cta1Url", "Cta2TextEn", "Cta2TextVi", "Cta2Url", "DescriptionEn", "DescriptionVi", "HighlightEn", "HighlightVi", "TitleEn", "TitleVi" },
                values: new object[] { 1, "/images/Hero illstration.png", "Explore Solutions", "Khám phá Giải pháp", "/en/medical", "Contact Us", "Liên hệ", "/en/contact", "Providing innovative and efficient solutions tailored to contemporary medical needs.", "Cung cấp các giải pháp sáng tạo và hiệu quả, được tinh chỉnh theo nhu cầu y tế đương đại.", "Healthcare", "Y tế Hiện đại", "Trusted Medical Solutions For Modern", "Giải pháp Y tế Tin cậy cho" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HeroSection");

            migrationBuilder.DropTable(
                name: "HomeSlide");
        }
    }
}
