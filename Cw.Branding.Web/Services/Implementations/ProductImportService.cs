using ClosedXML.Excel;
using Cw.Branding.Web.Data;
using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Models.Import;
using Cw.Branding.Web.Services.Interfaces;
using Cw.Branding.Web.Helpers; 
using Microsoft.EntityFrameworkCore;

namespace Cw.Branding.Web.Services
{
    public class ProductImportService : IProductImportService
    {
        private readonly AppDbContext _context;

        public ProductImportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProductImportResult> ValidateExcelAsync(Stream fileStream)
        {
            var result = new ProductImportResult();

            // 1. Pre-fetch master data để tối ưu hiệu năng
            var categories = await _context.Categories.ToDictionaryAsync(x => x.NameVi.Trim().ToLower(), x => x.Id);
            var brands = await _context.Brands.ToDictionaryAsync(x => x.Name.Trim().ToLower(), x => x.Id);
            var machineTypes = await _context.MachineTypes.ToDictionaryAsync(x => x.NameVi.Trim().ToLower(), x => x.Id);
            var existingCodes = await _context.Products.Select(p => p.Code.ToLower()).ToListAsync();

            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1);

            foreach (var row in rows)
            {
                var item = new ProductImportRow
                {
                    RowIndex = row.RowNumber(),
                    Code = row.Cell(1).GetValue<string>().Trim(),
                    NameVi = row.Cell(2).GetValue<string>().Trim(),
                    NameEn = row.Cell(3).GetValue<string>().Trim(),
                    CategoryName = row.Cell(4).GetValue<string>().Trim(),
                    BrandName = row.Cell(5).GetValue<string>().Trim(),
                    MachineTypeName = row.Cell(6).GetValue<string>().Trim(),
                    ShortDescriptionVi = row.Cell(7).GetValue<string>(),
                    ShortDescriptionEn = row.Cell(8).GetValue<string>(),
                    DescriptionVi = row.Cell(9).GetValue<string>(),
                    DescriptionEn = row.Cell(10).GetValue<string>(),
                    TechnicalSpecsVi = row.Cell(11).GetValue<string>(),
                    TechnicalSpecsEn = row.Cell(12).GetValue<string>(),
                    IsFeaturedStr = row.Cell(13).GetValue<string>().Trim(),
                    DisplayOrder = row.Cell(14).GetValue<int>()
                };

                // Validate dữ liệu bắt buộc
                if (string.IsNullOrEmpty(item.Code)) item.Errors.Add("Mã SP không được trống.");
                if (string.IsNullOrEmpty(item.NameVi)) item.Errors.Add("Tên (VI) không được trống.");

                // Xác định New/Update
                item.IsUpdate = existingCodes.Contains(item.Code.ToLower());

                // Lookup Category (Bắt buộc)
                if (categories.TryGetValue(item.CategoryName.ToLower(), out int catId))
                    item.CategoryId = catId;
                else
                    item.Errors.Add($"Danh mục '{item.CategoryName}' không tồn tại.");

                // Lookup Brand & MachineType (Không bắt buộc)
                if (!string.IsNullOrEmpty(item.BrandName) && brands.TryGetValue(item.BrandName.ToLower(), out int bId)) item.BrandId = bId;
                if (!string.IsNullOrEmpty(item.MachineTypeName) && machineTypes.TryGetValue(item.MachineTypeName.ToLower(), out int mId)) item.MachineTypeId = mId;

                // Parse IsFeatured
                item.IsFeatured = item.IsFeaturedStr.Equals("Yes", StringComparison.OrdinalIgnoreCase) || item.IsFeaturedStr == "1";

                result.Rows.Add(item);
            }
            return result;
        }

        public async Task<(bool Success, string Message)> CommitImportAsync(List<ProductImportRow> validRows)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var row in validRows)
                {
                    if (row.IsUpdate)
                    {
                        var existing = await _context.Products.FirstOrDefaultAsync(p => p.Code == row.Code);
                        if (existing != null)
                        {
                            MapData(row, existing);
                            existing.UpdatedAt = DateTime.UtcNow;
                            _context.Products.Update(existing);
                        }
                    }
                    else
                    {
                        var newProduct = new Product { CreatedAt = DateTime.UtcNow, IsActive = true };
                        MapData(row, newProduct);
                        _context.Products.Add(newProduct);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, $"Thành công: Đã nhập {validRows.Count} sản phẩm.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Lỗi: {ex.Message}");
            }
        }

        private void MapData(ProductImportRow source, Product target)
        {
            target.Code = source.Code;
            target.NameVi = source.NameVi;
            target.NameEn = source.NameEn;

            // Đã đổi thành GenerateSlug để khớp với Helper của anh
            target.SlugVi = SlugHelper.GenerateSlug(source.NameVi);
            target.SlugEn = SlugHelper.GenerateSlug(source.NameEn);

            target.CategoryId = source.CategoryId!.Value;
            target.BrandId = source.BrandId;
            target.MachineTypeId = source.MachineTypeId;
            target.ShortDescriptionVi = source.ShortDescriptionVi;
            target.ShortDescriptionEn = source.ShortDescriptionEn;
            target.DescriptionVi = source.DescriptionVi;
            target.DescriptionEn = source.DescriptionEn;
            target.TechnicalSpecsVi = source.TechnicalSpecsVi;
            target.TechnicalSpecsEn = source.TechnicalSpecsEn;
            target.IsFeatured = source.IsFeatured;
            target.DisplayOrder = source.DisplayOrder;
        }
    }
}