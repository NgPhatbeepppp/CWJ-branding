using Cw.Branding.Web.Data;
using Cw.Branding.Web.Helpers; 
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

    // Phục vụ Dropdown trong ProductController
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
        var isVi = lang.ToLower() == "vi";
        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => (isVi ? x.SlugVi == slug : x.SlugEn == slug) && x.IsActive);
    }

    public async Task<bool> CreateAsync(Category category)
    {
        try
        {
            // Tích hợp Auto-slug từ SlugHelper anh đã gửi
            if (string.IsNullOrWhiteSpace(category.SlugVi))
                category.SlugVi = SlugHelper.GenerateSlug(category.NameVi);
            if (string.IsNullOrWhiteSpace(category.SlugEn))
                category.SlugEn = SlugHelper.GenerateSlug(category.NameEn);

            category.CreatedAt = DateTime.UtcNow;
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return true;
        }
        catch { return false; }
    }

    public async Task<bool> UpdateAsync(Category category)
    {
        try
        {
            var existing = await _context.Categories.FindAsync(category.Id);
            if (existing == null) return false;

            existing.Code = category.Code;
            existing.NameVi = category.NameVi;
            existing.NameEn = category.NameEn;
            existing.IconPath = category.IconPath;
            existing.IsActive = category.IsActive;
            existing.DisplayOrder = category.DisplayOrder;
            existing.UpdatedAt = DateTime.UtcNow;

            // Cập nhật Slug nếu cần
            existing.SlugVi = string.IsNullOrWhiteSpace(category.SlugVi)
                ? SlugHelper.GenerateSlug(category.NameVi) : category.SlugVi;
            existing.SlugEn = string.IsNullOrWhiteSpace(category.SlugEn)
                ? SlugHelper.GenerateSlug(category.NameEn) : category.SlugEn;

            await _context.SaveChangesAsync();
            return true;
        }
        catch { return false; }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null || category.Products.Any()) return false;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
}