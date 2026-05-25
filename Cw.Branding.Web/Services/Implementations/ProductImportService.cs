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

            // Pre-fetch dữ liệu để so khớp nhanh
            var categories = await _context.Categories.ToDictionaryAsync(x => x.NameVi.Trim().ToLower(), x => x.Id);
            var brands = await _context.Brands.ToDictionaryAsync(x => x.Name.Trim().ToLower(), x => x.Id);
            var machineTypes = await _context.MachineTypes.ToDictionaryAsync(x => x.NameVi.Trim().ToLower(), x => x.Id);
            var existingCodes = await _context.Products.Select(p => p.Code.ToLower()).ToListAsync();

            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(1); // Bỏ qua header

            foreach (var row in rows)
            {
                var item = MapRowToModel(row);

                // 1. Strict Validation cho Category (Theo UXD-02)
                if (categories.TryGetValue(item.CategoryName.ToLower(), out int catId))
                    item.CategoryId = catId;
                else
                    item.Errors.Add($"Danh mục '{item.CategoryName}' không tồn tại. Vui lòng tạo danh mục trước.");

                // 2. Auto-create detection cho Brand[cite: 1]
                if (!string.IsNullOrEmpty(item.BrandName))
                {
                    if (brands.TryGetValue(item.BrandName.ToLower(), out int bId))
                        item.BrandId = bId;
                    else
                    {
                        item.IsNewBrand = true;
                        item.Warnings.Add($"Hãng '{item.BrandName}' sẽ được tạo mới.");
                    }
                }

                // 3. Auto-create detection cho MachineType[cite: 1]
                if (!string.IsNullOrEmpty(item.MachineTypeName))
                {
                    if (machineTypes.TryGetValue(item.MachineTypeName.ToLower(), out int mId))
                        item.MachineTypeId = mId;
                    else
                    {
                        item.IsNewMachineType = true;
                        item.Warnings.Add($"Loại máy '{item.MachineTypeName}' sẽ được tạo mới.");
                    }
                }

                item.IsUpdate = existingCodes.Contains(item.Code.ToLower());
                result.Rows.Add(item);
            }
            return result;
        }

        public async Task<(bool Success, string Message)> CommitImportAsync(List<ProductImportRow> validRows)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Bước 1: Xử lý tạo mới Brand/MachineType trước để lấy ID
                foreach (var row in validRows)
                {
                    if (row.IsNewBrand && !string.IsNullOrEmpty(row.BrandName))
                    {
                        var newBrand = await _context.Brands.FirstOrDefaultAsync(b => b.Name.ToLower() == row.BrandName.ToLower());
                        if (newBrand == null)
                        {
                            newBrand = new Brand { Name = row.BrandName, CreatedAt = DateTime.UtcNow };
                            _context.Brands.Add(newBrand);
                            await _context.SaveChangesAsync(); // Lưu để lấy ID ngay
                        }
                        row.BrandId = newBrand.Id;
                    }

                    if (row.IsNewMachineType && !string.IsNullOrEmpty(row.MachineTypeName))
                    {
                        var newMt = await _context.MachineTypes.FirstOrDefaultAsync(m => m.NameVi.ToLower() == row.MachineTypeName.ToLower());
                        if (newMt == null)
                        {
                            newMt = new MachineType
                            {
                                NameVi = row.MachineTypeName,
                                NameEn = row.MachineTypeName,
                                // Đã sửa row.NameVi và row.NameEn thành row.MachineTypeName để tránh lỗi (trừ khi model của bạn thực sự có row.NameVi)
                                SlugVi = SlugHelper.GenerateSlug(row.MachineTypeName), // SEO cho Việt
                                SlugEn = SlugHelper.GenerateSlug(row.MachineTypeName), // SEO cho Anh
                                IsActive = true
                            };
                            _context.MachineTypes.Add(newMt);
                            await _context.SaveChangesAsync();
                        }
                        row.MachineTypeId = newMt.Id;
                    }
                }

                // ==========================================
                // BƯỚC 2: NHẬP SẢN PHẨM (UPSERT - CẬP NHẬT HOẶC THÊM MỚI)
                // ==========================================

                // 2.1: Lọc bỏ các dòng trùng lặp (Duplicate) NGAY BÊN TRONG file Excel
                // Nếu file Excel có 2 dòng cùng Code, ta chỉ lấy dòng cuối cùng để xử lý
                var uniqueRows = validRows
                    .GroupBy(r => r.Code.ToLower())
                    .Select(g => g.Last()) // Lấy dòng cấu hình mới nhất nếu trùng
                    .ToList();

                // 2.2: Lấy danh sách tất cả mã Code từ file
                var incomingCodes = uniqueRows.Select(r => r.Code.ToLower()).ToList();

                // 2.3: Truy vấn DUY NHẤT 1 LẦN để kéo tất cả sản phẩm đang có trong DB lên Memory
                var existingProducts = await _context.Products
                    .Where(p => incomingCodes.Contains(p.Code.ToLower()))
                    .ToDictionaryAsync(p => p.Code.ToLower(), p => p); // Lưu thành Dictionary để tìm siêu tốc (O(1))

                // 2.4: Phân loại dữ liệu vào 2 rổ (Insert và Update)
                var productsToInsert = new List<Product>();
                var productsToUpdate = new List<Product>();

                foreach (var row in uniqueRows)
                {
                    var codeKey = row.Code.ToLower();

                    if (existingProducts.TryGetValue(codeKey, out var existingProduct))
                    {
                        // TH1: Đã tồn tại trong DB -> Cập nhật thông tin
                        MapData(row, existingProduct);
                        existingProduct.UpdatedAt = DateTime.UtcNow;
                        productsToUpdate.Add(existingProduct);
                    }
                    else
                    {
                        // TH2: Chưa có trong DB -> Thêm mới
                        var newProduct = new Product { CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, IsActive = true };
                        MapData(row, newProduct);
                        productsToInsert.Add(newProduct);
                    }
                }

                // 2.5: Thực thi Bulk Operations với Entity Framework
                if (productsToInsert.Any())
                {
                    _context.Products.AddRange(productsToInsert); // Insert hàng loạt
                }

                if (productsToUpdate.Any())
                {
                    _context.Products.UpdateRange(productsToUpdate); // Update hàng loạt
                }

                // 2.6: Lưu tất cả thay đổi xuống DB trong 1 Transaction duy nhất
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Trả về báo cáo chi tiết cho UI
                return (true, $"Thành công: Đã thêm mới {productsToInsert.Count} và cập nhật {productsToUpdate.Count} sản phẩm.");
            }
            catch (Exception ex)
            {
                // Phải có catch để Rollback nếu có lỗi xảy ra giữa chừng
                await transaction.RollbackAsync();

                // Bạn có thể cân nhắc thêm _logger.LogError(ex, "Lỗi Import") ở đây
                return (false, $"Lỗi trong quá trình import: {ex.Message}");
            }
        }




        private void MapData(ProductImportRow source, Product target)
        {
            target.Code = source.Code;
            target.NameVi = source.NameVi;
            target.NameEn = source.NameEn;

            // Generate Slug chuẩn SEO (UXD-02)
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

        private ProductImportRow MapRowToModel(IXLRow row)
        {
            var isFeaturedStr = row.Cell(13).GetValue<string>()?.Trim().ToLower();

            // 1. Xử lý an toàn cho cột DisplayOrder (Cột 14)
            int displayOrder = 0; // Giá trị mặc định nếu bỏ trống hoặc nhập sai
            var displayOrderCell = row.Cell(14);
            if (!displayOrderCell.IsEmpty())
            {
                // TryGetValue sẽ trả về false nếu không parse được (chứa chữ), không gây crash
                displayOrderCell.TryGetValue<int>(out displayOrder);
            }

            // 2. Thêm toán tử ?. và ?? "" để chống NullReferenceException ở các cột bắt buộc
            return new ProductImportRow
            {
                RowIndex = row.RowNumber(),
                Code = row.Cell(1).GetValue<string>()?.Trim() ?? "",
                NameVi = row.Cell(2).GetValue<string>()?.Trim() ?? "",
                NameEn = row.Cell(3).GetValue<string>()?.Trim() ?? "",
                CategoryName = row.Cell(4).GetValue<string>()?.Trim() ?? "",
                BrandName = row.Cell(5).GetValue<string>()?.Trim(),
                MachineTypeName = row.Cell(6).GetValue<string>()?.Trim(),
                ShortDescriptionVi = row.Cell(7).GetValue<string>(),
                ShortDescriptionEn = row.Cell(8).GetValue<string>(),
                DescriptionVi = row.Cell(9).GetValue<string>(),
                DescriptionEn = row.Cell(10).GetValue<string>(),
                TechnicalSpecsVi = row.Cell(11).GetValue<string>(),
                TechnicalSpecsEn = row.Cell(12).GetValue<string>(),
                IsFeaturedStr = isFeaturedStr ?? "no",
                // Parse logic: chấp nhận "yes", "1", "true"
                IsFeatured = isFeaturedStr == "yes" || isFeaturedStr == "1" || isFeaturedStr == "true",

                DisplayOrder = displayOrder // Gán giá trị đã parse an toàn
            };
        }
    }
}