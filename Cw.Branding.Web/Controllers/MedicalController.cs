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

        // =========================================================
        // 1. ACTION INDEX (Trang chủ Medical)
        // Mục tiêu: URL đẹp cho SEO (medical-solutions / giai-phap-y-te)
        // =========================================================

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

        // =========================================================
        // 2. ACTION API (AJAX gọi vào đây)
        // Mục tiêu: Đường dẫn rõ ràng, kỹ thuật, dễ gọi
        // URL: /en/Medical/FilterProducts?categoryId=...
        // =========================================================

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
            catch
            {
                return StatusCode(500);
            }
        }
        // =========================================================
        // 3. CHI TIẾT SẢN PHẨM (DETAIL)
        // URL: /en/medical/product/{slug} (Đây là cái bạn đang thiếu/sai)
        // =========================================================
        [HttpGet("{lang}/medical/product/{slug}")]
        public async Task<IActionResult> Detail(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return NotFound();

            // Tìm sản phẩm theo Slug (Chấp nhận cả SlugEn và SlugVi)
            var product = await _context.Products
                .Include(p => p.Category)      // Load danh mục để làm Breadcrumb
                .Include(p => p.Images)        // Load ảnh
                .FirstOrDefaultAsync(p => p.SlugEn == slug || p.SlugVi == slug);

            // Nếu không tìm thấy sản phẩm trong DB -> Trả về 404
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

    }
}