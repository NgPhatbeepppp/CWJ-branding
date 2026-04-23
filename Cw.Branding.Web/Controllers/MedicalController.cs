using Cw.Branding.Web.Data;
using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Models.ViewModels;
using Cw.Branding.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cw.Branding.Web.Controllers
{
    [Route("{lang:regex(^(en|vi)$)}")]
    public class MedicalController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService; // Thêm Service mới

        public MedicalController(
            AppDbContext context,
            IProductService productService,
            ICategoryService categoryService)
        {
            _context = context;
            _productService = productService;
            _categoryService = categoryService;
        }

        // GET: /vi/giai-phap-y-te hoặc /en/medical-solutions
        [HttpGet("medical-solutions")]
        [HttpGet("giai-phap-y-te")]
        public async Task<IActionResult> Index()
        {
            // Gọi hàm trả về danh sách Active cho trang chủ Medical
            var categories = await _categoryService.GetActiveCategoriesAsync();

            var viewModel = new MedicalIndexViewModel
            {
                Categories = categories,
                InitialProducts = new List<Product>()
            };

            return View(viewModel);
        }

        // GET: /vi/medical/product/{slug}
        [HttpGet("medical/product/{slug}")]
        public async Task<IActionResult> Detail(string lang, string slug)
        {
            if (string.IsNullOrEmpty(slug)) return NotFound();

            // Lấy sản phẩm và dữ liệu liên quan (Eager Loading)
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Brand)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => (p.SlugEn == slug || p.SlugVi == slug) && p.IsActive);

            if (product == null) return NotFound();

            // --- SEO AUTOMATION LOGIC ---
            var isEn = lang == "en";
            string metaDesc = isEn
                ? (product.ShortDescriptionEn ?? product.NameEn)
                : (product.ShortDescriptionVi ?? product.NameVi);

            string? ogImage = product.Images?.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.FilePath;

            ViewBag.SeoData = new SeoViewModel
            {
                Title = $"{(isEn ? product.NameEn : product.NameVi)} | Charles Wembley Medical",
                Description = metaDesc,
                ImageUrl = ogImage,
                Type = "product",
                CanonicalUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}"
            };

            // Lấy 3 sản phẩm liên quan cùng Category
            ViewBag.RelatedProducts = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Category)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .Take(3)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.CurrentLang = lang;

            return View(product);
        }

        // --- AJAX FILTER: Phục vụ loadProducts() trong Index.cshtml ---
        [HttpGet("medical/filter")]
        public async Task<IActionResult> FilterProducts(string lang, int? categoryId, int? brandId, int? machineTypeId, string? searchTerm, int page = 1)
        {
            try
            {
                int pageSize = 8; // Theo yêu cầu MVP

                // Gọi Service xử lý filter và phân trang
                var result = await _productService.GetFilteredProductsPaginatedAsync(searchTerm, categoryId, brandId, machineTypeId, page, pageSize);

                // Load dữ liệu cho các dropdown bộ lọc
                ViewBag.Brands = await _context.Brands.Where(b => b.IsActive).OrderBy(b => b.Name).AsNoTracking().ToListAsync();

                var machineTypesQuery = _context.MachineTypes.Where(m => m.IsActive).AsNoTracking();
                ViewBag.MachineTypes = lang == "vi"
                    ? await machineTypesQuery.OrderBy(m => m.NameVi).ToListAsync()
                    : await machineTypesQuery.OrderBy(m => m.NameEn).ToListAsync();

                // Truyền trạng thái hiện tại xuống Partial View
                ViewBag.CurrentCategoryId = categoryId;
                ViewBag.CurrentBrandId = brandId;
                ViewBag.CurrentMachineTypeId = machineTypeId;
                ViewBag.CurrentSearch = searchTerm;
                ViewBag.CurrentLang = lang;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize);
                ViewBag.TotalCount = result.TotalCount;

                return PartialView("_ProductListPartial", result.Items);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error filtering products");
            }
        }
    }
}