using Cw.Branding.Web.Data;
using Cw.Branding.Web.Helpers;
using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cw.Branding.Web.Services.Implementations;

public class BrandService(AppDbContext context) : IBrandService
{
    public async Task<List<Brand>> GetAllAsync() =>
        await context.Brands.OrderBy(b => b.DisplayOrder).ToListAsync();

    public async Task<List<Brand>> GetActiveBrandsAsync() =>
        await context.Brands.Where(b => b.IsActive).OrderBy(b => b.DisplayOrder).ToListAsync();

    public async Task<Brand?> GetByIdAsync(int id) =>
        await context.Brands.FindAsync(id);

    public async Task CreateAsync(Brand brand)
    {
        brand.Slug = SlugHelper.GenerateSlug(brand.Name);
        brand.CreatedAt = DateTime.Now;
        context.Brands.Add(brand);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Brand brand)
    {
        brand.Slug = SlugHelper.GenerateSlug(brand.Name);
        context.Brands.Update(brand);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var brand = await context.Brands.FindAsync(id);
        if (brand != null)
        {
            context.Brands.Remove(brand);
            await context.SaveChangesAsync();
        }
    }
}