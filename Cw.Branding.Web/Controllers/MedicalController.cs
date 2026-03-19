using Cw.Branding.Web.Data;
using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Models.ViewModels;
using Cw.Branding.Web.Services;
using Cw.Branding.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cw.Branding.Web.Controllers
{
    public class MedicalController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IProductService _productService;
        public MedicalController(AppDbContext context, IProductService productService)
        {
            _context = context;
            _productService = productService;
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
       
        [HttpGet("{lang}/medical/product/{slug}")]
        public async Task<IActionResult> Detail(string lang, string slug)
        {
            if (string.IsNullOrEmpty(slug)) return NotFound();

            // 1. Lấy sản phẩm (Eager Loading đầy đủ để hiện Detail và Gallery)
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Brand)
                .AsNoTracking() // Thêm AsNoTracking để tối ưu tốc độ load trang client
                .FirstOrDefaultAsync(p => (p.SlugEn == slug || p.SlugVi == slug) && p.IsActive);

            if (product == null) return NotFound();

            // --- BẮT ĐẦU: SEO AUTOMATION LOGIC ---
            var isEn = lang == "en";

            // Tự động lấy Meta Description từ ShortDescription, nếu trống thì lấy tên SP
            string metaDesc = isEn
                ? (product.ShortDescriptionEn ?? product.NameEn)
                : (product.ShortDescriptionVi ?? product.NameVi);

            // Lấy ảnh chính từ Gallery để làm ảnh preview khi share link (OpenGraph Image)
            string? ogImage = product.Images?.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.FilePath;

            ViewBag.SeoData = new Cw.Branding.Web.Models.ViewModels.SeoViewModel
            {
                Title = $"{(isEn ? product.NameEn : product.NameVi)} | Charles Wembley Medical",
                Description = metaDesc,
                ImageUrl = ogImage,
                Type = "product",
                CanonicalUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}"
            };
            // --- KẾT THÚC: SEO AUTOMATION LOGIC ---

            // 2. Lấy 3 sản phẩm liên quan
            var relatedProducts = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Category)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .Take(3)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;
            ViewBag.CurrentLang = lang; // Truyền xuống để view xử lý logic ngôn ngữ nếu cần

            return View(product);
        }
        [HttpGet("{lang}/Medical/FilterProducts")]
        public async Task<IActionResult> FilterProducts(string lang, int? categoryId, int? brandId, int? machineTypeId, string? searchTerm, int page = 1)
        {
            try
            {
                int pageSize = 8; // Fix 8 sản phẩm/trang theo yêu cầu

                // Gọi hàm phân trang mới
                var result = await _productService.GetFilteredProductsPaginatedAsync(searchTerm, categoryId, brandId, machineTypeId, page, pageSize);

                // Giữ nguyên logic lấy Dropdowns như cũ
                ViewBag.Brands = await _context.Brands.Where(b => b.IsActive).OrderBy(b => b.Name).AsNoTracking().ToListAsync();
                var machineTypesQuery = _context.MachineTypes.Where(m => m.IsActive).AsNoTracking();
                ViewBag.MachineTypes = lang == "vi"
                    ? await machineTypesQuery.OrderBy(m => m.NameVi).ToListAsync()
                    : await machineTypesQuery.OrderBy(m => m.NameEn).ToListAsync();

                // Đẩy trạng thái xuống View
                ViewBag.CurrentCategoryId = categoryId;
                ViewBag.CurrentBrandId = brandId;
                ViewBag.CurrentMachineTypeId = machineTypeId;
                ViewBag.CurrentSearch = searchTerm;
                ViewBag.CurrentLang = lang;

                // PAGING DATA
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize);
                ViewBag.TotalCount = result.TotalCount;

                // Trả về Items thay vì biến products cũ
                return PartialView("_ProductListPartial", result.Items);
            }
            catch (Exception)
            {
                return StatusCode(500, "Error filtering products");
            }
        }
    }

}