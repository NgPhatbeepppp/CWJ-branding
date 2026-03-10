using Cw.Branding.Web.Data;
using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cw.Branding.Web.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    // Lấy tất cả cho Admin, kèm theo thông tin quan hệ để hiển thị lên bảng
    public async Task<List<Product>> GetAllForAdminAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.MachineType)
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Images) // Load luôn ảnh
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    // Cho trang chủ (Chỉ lấy sản phẩm nổi bật & đang active)
    public async Task<List<Product>> GetFeaturedProductsAsync(int limit = 8)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.IsActive && p.IsFeatured)
            .OrderBy(p => p.DisplayOrder)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();
    }

    // Cho trang danh sách theo Chuyên khoa
    public async Task<List<Product>> GetActiveByCategoryAsync(int categoryId)
    {
        return await _context.Products
            .Include(p => p.Brand)
            .Include(p => p.MachineType)
            .Include(p => p.Images)
            .Where(p => p.IsActive && p.CategoryId == categoryId)
            .OrderBy(p => p.DisplayOrder)
            .AsNoTracking()
            .ToListAsync();
    }

    // Lấy chi tiết bằng Slug (hỗ trợ cả tiếng Anh và tiếng Việt)
    public async Task<Product?> GetBySlugAsync(string slug, string lang)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.MachineType)
            .Include(p => p.Images)
            .Where(p => p.IsActive);

        if (lang.ToLower() == "vi")
        {
            return await query.FirstOrDefaultAsync(p => p.SlugVi == slug);
        }

        return await query.FirstOrDefaultAsync(p => p.SlugEn == slug);
    }

    public async Task<bool> CreateAsync(Product product)
    {
        try
        {
            product.CreatedAt = DateTime.UtcNow;
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            // Note: Thực tế nên dùng ILogger để log lỗi ở đây
            return false;
        }
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        try
        {
            var existing = await _context.Products.FindAsync(product.Id);
            if (existing == null) return false;

            // Mapping data (Có thể dùng AutoMapper, nhưng MVP mình map tay cho gọn và kiểm soát tốt)
            existing.Code = product.Code;
            existing.NameVi = product.NameVi;
            existing.NameEn = product.NameEn;
            existing.ShortDescriptionVi = product.ShortDescriptionVi;
            existing.ShortDescriptionEn = product.ShortDescriptionEn;
            existing.DescriptionVi = product.DescriptionVi;
            existing.DescriptionEn = product.DescriptionEn;
            existing.TechnicalSpecsVi = product.TechnicalSpecsVi;
            existing.TechnicalSpecsEn = product.TechnicalSpecsEn;
            existing.SlugVi = product.SlugVi;
            existing.SlugEn = product.SlugEn;
            existing.IsFeatured = product.IsFeatured;
            existing.IsActive = product.IsActive;
            existing.DisplayOrder = product.DisplayOrder;

            // Cập nhật FK
            existing.CategoryId = product.CategoryId;
            existing.BrandId = product.BrandId;
            existing.MachineTypeId = product.MachineTypeId;

            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }
}