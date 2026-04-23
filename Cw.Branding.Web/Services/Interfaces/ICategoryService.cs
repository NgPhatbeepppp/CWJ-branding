using Cw.Branding.Web.Models.Entities;

namespace Cw.Branding.Web.Services.Interfaces;

public interface ICategoryService
{
    // Giữ nguyên tên hàm cũ để không lỗi ProductController
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<bool> CreateAsync(Category category);
    Task<bool> UpdateAsync(Category category);
    Task<bool> DeleteAsync(int id);

    // Cho Client
    Task<List<Category>> GetActiveCategoriesAsync();
    Task<Category?> GetBySlugAsync(string slug, string lang);
}