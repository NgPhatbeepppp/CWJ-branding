using Cw.Branding.Web.Models.Entities;
using Microsoft.AspNetCore.Http;
namespace Cw.Branding.Web.Services.Interfaces;

public interface IProductService
{
    // --- ADMIN ---
    Task<List<Product>> GetAllForAdminAsync(string? searchTerm = null, int? categoryId = null, int? brandId = null, bool? isActive = null);
    Task<Product?> GetByIdAsync(int id);
    Task<bool> CreateAsync(Product product);
    Task<bool> UpdateAsync(Product product);
    Task<bool> IsCodeExistsAsync(string code);
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateProductWithImagesAsync(Product product, List<IFormFile> newImages, List<int> deletedImageIds, int? mainImageId = null, string? mainImageName = null);
    Task<bool> CreateWithImagesAsync(Product product, List<IFormFile> newImages, string? mainImageName = null);
    // --- CLIENT ---
    Task<List<Product>> GetFeaturedProductsAsync(int limit = 8); // Cho trang chủ
    Task<List<Product>> GetActiveByCategoryAsync(int categoryId); // Lọc theo chuyên khoa
    Task<Product?> GetBySlugAsync(string slug, string lang); // Chi tiết sản phẩm
    Task<List<Product>> GetFilteredProductsClientAsync(string? searchTerm, int? categoryId, int? brandId, int? machineTypeId);
    Task<bool> UpdateDisplayOrderAsync(int id, int displayOrder); 

}