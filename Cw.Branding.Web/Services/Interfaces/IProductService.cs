using Cw.Branding.Web.Models.Entities;

namespace Cw.Branding.Web.Services.Interfaces;

public interface IProductService
{
    // --- ADMIN ---
    Task<List<Product>> GetAllForAdminAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<bool> CreateAsync(Product product);
    Task<bool> UpdateAsync(Product product);
    Task<bool> DeleteAsync(int id);

    // --- CLIENT ---
    Task<List<Product>> GetFeaturedProductsAsync(int limit = 8); // Cho trang chủ
    Task<List<Product>> GetActiveByCategoryAsync(int categoryId); // Lọc theo chuyên khoa
    Task<Product?> GetBySlugAsync(string slug, string lang); // Chi tiết sản phẩm
}