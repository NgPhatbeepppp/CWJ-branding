using Cw.Branding.Web.Models.Entities;

namespace Cw.Branding.Web.Services.Interfaces;

public interface IBrandService
{
    Task<List<Brand>> GetAllAsync();
    Task<List<Brand>> GetActiveBrandsAsync();
    Task<Brand?> GetByIdAsync(int id);
    Task CreateAsync(Brand brand);
    Task UpdateAsync(Brand brand);
    Task DeleteAsync(int id);
}