using Cw.Branding.Web.Data;
using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cw.Branding.Web.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _context.Categories
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();
    }

    public async Task<List<Category>> GetActiveCategoriesAsync()
    {
        return await _context.Categories
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task<Category?> GetBySlugAsync(string slug, string lang)
    {
        if (lang.ToLower() == "vi")
        {
            return await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.SlugVi == slug && x.IsActive);
        }
        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SlugEn == slug && x.IsActive);
    }

    public async Task<bool> CreateAsync(Category category)
    {
        try
        {
            category.CreatedAt = DateTime.UtcNow;
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> UpdateAsync(Category category)
    {
        try
        {
            var existing = await _context.Categories.FindAsync(category.Id);
            if (existing == null) return false;

            // Map data
            existing.Code = category.Code;
            existing.NameVi = category.NameVi;
            existing.NameEn = category.NameEn;
            existing.SlugVi = category.SlugVi;
            existing.SlugEn = category.SlugEn;
            existing.IconPath = category.IconPath;
            existing.IsActive = category.IsActive;
            existing.DisplayOrder = category.DisplayOrder;
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
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return false;

        // Lưu ý: DbContext đã cấu hình Restrict, 
        // nên nếu có sản phẩm thuộc Category này, EF sẽ tự ném lỗi nếu xóa.
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
}