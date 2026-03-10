using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cw.Branding.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductWithFeaturedAndRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_Products_ProductId",
                table: "ProductImages");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Brand_BrandId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_MachineType_MachineTypeId",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductImages",
                table: "ProductImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContactFormEntries",
                table: "ContactFormEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Product");

            migrationBuilder.RenameTable(
                name: "ProductImages",
                newName: "ProductImage");

            migrationBuilder.RenameTable(
                name: "ContactFormEntries",
                newName: "ContactFormEntry");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "Category");

            migrationBuilder.RenameIndex(
                name: "IX_Products_SlugVi",
                table: "Product",
                newName: "IX_Product_SlugVi");

            migrationBuilder.RenameIndex(
                name: "IX_Products_SlugEn",
                table: "Product",
                newName: "IX_Product_SlugEn");

            migrationBuilder.RenameIndex(
                name: "IX_Products_MachineTypeId",
                table: "Product",
                newName: "IX_Product_MachineTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_Code",
                table: "Product",
                newName: "IX_Product_Code");

            migrationBuilder.RenameIndex(
                name: "IX_Products_CategoryId",
                table: "Product",
                newName: "IX_Product_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Products_BrandId",
                table: "Product",
                newName: "IX_Product_BrandId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImage",
                newName: "IX_ProductImage_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ContactFormEntries_CreatedAt",
                table: "ContactFormEntry",
                newName: "IX_ContactFormEntry_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_SlugVi",
                table: "Category",
                newName: "IX_Category_SlugVi");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_SlugEn",
                table: "Category",
                newName: "IX_Category_SlugEn");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_Code",
                table: "Category",
                newName: "IX_Category_Code");

            migrationBuilder.AddColumn<int>(
                name: "BrandId1",
                table: "Product",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Product",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MachineTypeId1",
                table: "Product",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Product",
                table: "Product",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductImage",
                table: "ProductImage",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContactFormEntry",
                table: "ContactFormEntry",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Category",
                table: "Category",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Product_BrandId1",
                table: "Product",
                column: "BrandId1");

            migrationBuilder.CreateIndex(
                name: "IX_Product_MachineTypeId1",
                table: "Product",
                column: "MachineTypeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Brand_BrandId",
                table: "Product",
                column: "BrandId",
                principalTable: "Brand",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Brand_BrandId1",
                table: "Product",
                column: "BrandId1",
                principalTable: "Brand",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Category_CategoryId",
                table: "Product",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_MachineType_MachineTypeId",
                table: "Product",
                column: "MachineTypeId",
                principalTable: "MachineType",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_MachineType_MachineTypeId1",
                table: "Product",
                column: "MachineTypeId1",
                principalTable: "MachineType",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImage_Product_ProductId",
                table: "ProductImage",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_Brand_BrandId",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_Brand_BrandId1",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_Category_CategoryId",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_MachineType_MachineTypeId",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_MachineType_MachineTypeId1",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImage_Product_ProductId",
                table: "ProductImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductImage",
                table: "ProductImage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Product",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_BrandId1",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_MachineTypeId1",
                table: "Product");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ContactFormEntry",
                table: "ContactFormEntry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Category",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "BrandId1",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "MachineTypeId1",
                table: "Product");

            migrationBuilder.RenameTable(
                name: "ProductImage",
                newName: "ProductImages");

            migrationBuilder.RenameTable(
                name: "Product",
                newName: "Products");

            migrationBuilder.RenameTable(
                name: "ContactFormEntry",
                newName: "ContactFormEntries");

            migrationBuilder.RenameTable(
                name: "Category",
                newName: "Categories");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImage_ProductId",
                table: "ProductImages",
                newName: "IX_ProductImages_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_Product_SlugVi",
                table: "Products",
                newName: "IX_Products_SlugVi");

            migrationBuilder.RenameIndex(
                name: "IX_Product_SlugEn",
                table: "Products",
                newName: "IX_Products_SlugEn");

            migrationBuilder.RenameIndex(
                name: "IX_Product_MachineTypeId",
                table: "Products",
                newName: "IX_Products_MachineTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Product_Code",
                table: "Products",
                newName: "IX_Products_Code");

            migrationBuilder.RenameIndex(
                name: "IX_Product_CategoryId",
                table: "Products",
                newName: "IX_Products_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Product_BrandId",
                table: "Products",
                newName: "IX_Products_BrandId");

            migrationBuilder.RenameIndex(
                name: "IX_ContactFormEntry_CreatedAt",
                table: "ContactFormEntries",
                newName: "IX_ContactFormEntries_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Category_SlugVi",
                table: "Categories",
                newName: "IX_Categories_SlugVi");

            migrationBuilder.RenameIndex(
                name: "IX_Category_SlugEn",
                table: "Categories",
                newName: "IX_Categories_SlugEn");

            migrationBuilder.RenameIndex(
                name: "IX_Category_Code",
                table: "Categories",
                newName: "IX_Categories_Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductImages",
                table: "ProductImages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                table: "Products",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContactFormEntries",
                table: "ContactFormEntries",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_Products_ProductId",
                table: "ProductImages",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Brand_BrandId",
                table: "Products",
                column: "BrandId",
                principalTable: "Brand",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_MachineType_MachineTypeId",
                table: "Products",
                column: "MachineTypeId",
                principalTable: "MachineType",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
