using Cw.Branding.Web.Models.Entities;

namespace Cw.Branding.Web.Services.Interfaces;

public interface ICategoryService
{
    // Cho Admin
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<bool> CreateAsync(Category category);
    Task<bool> UpdateAsync(Category category);
    Task<bool> DeleteAsync(int id);

    // Cho Client (Chỉ lấy cái Active và sắp xếp theo DisplayOrder)
    Task<List<Category>> GetActiveCategoriesAsync();
    Task<Category?> GetBySlugAsync(string slug, string lang);
}