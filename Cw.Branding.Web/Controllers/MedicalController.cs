using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cw.Branding.Web.Data;
using Cw.Branding.Web.Models.ViewModels;
using Cw.Branding.Web.Models.Entities;

namespace Cw.Branding.Web.Controllers
{
    public class MedicalController : Controller
    {
        private readonly AppDbContext _context;

        public MedicalController(AppDbContext context)
        {
            _context = context;
        }

       

        [HttpGet("{lang}/medical-solutions")] // URL Tiếng Anh
        [HttpGet("{lang}/giai-phap-y-te")]    // URL Tiếng Việt
        public async Task<IActionResult> Index()
        {
            // Lấy danh mục
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var viewModel = new MedicalIndexViewModel
            {
                Categories = categories,
                InitialProducts = new List<Product>() // Khởi tạo rỗng để tránh null
            };

            return View(viewModel);
        }

       

        [HttpGet("{lang}/Medical/FilterProducts")]
        public async Task<IActionResult> FilterProducts(int categoryId)
        {
            try
            {
                var products = await _context.Products
                    .Include(p => p.Images)
                    .Where(p => p.CategoryId == categoryId && p.IsActive)
                    .OrderBy(p => p.DisplayOrder)
                    .ToListAsync();

                return PartialView("_ProductListPartial", products);
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Error loading products: {ex.Message}");
                return StatusCode(500, new { error = "Failed to load products" });
            }
        }

        [HttpGet("{lang}/medical/product/{slug}")]
        public async Task<IActionResult> Detail(string lang, string slug)
        {
            if (string.IsNullOrEmpty(slug)) return NotFound();

            // 1. Tìm sản phẩm hiện tại
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.SlugEn == slug || p.SlugVi == slug);

            if (product == null) return NotFound();

            // 2. Lấy 3 sản phẩm liên quan (cùng CategoryId, khác Id hiện tại)
            var relatedProducts = await _context.Products
                .Include(p => p.Images)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.IsActive)
                .OrderByDescending(p => p.DisplayOrder) // Hoặc dùng Guid.NewGuid() để lấy ngẫu nhiên
                .Take(3)
                .ToListAsync();

            // Gán vào ViewBag để View sử dụng cho gọn, không cần tạo thêm ViewModel phức tạp
            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }

    }
}