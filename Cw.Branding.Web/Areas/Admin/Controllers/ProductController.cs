using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Cw.Branding.Web.Helpers; 
namespace Cw.Branding.Web.Areas.Admin.Controllers;

[Area("Admin")]
// Nhắc lại Context Ngày 2: Không dùng [Route] ở đây để tránh xung đột, 
// đã xử lý routing chung tại Program.cs
public class ProductController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IBrandService _brandService;
    private readonly IMachineTypeService _machineTypeService;

    public ProductController(
        IProductService productService,
        ICategoryService categoryService,
        IBrandService brandService,
        IMachineTypeService machineTypeService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _brandService = brandService;
        _machineTypeService = machineTypeService;
    }

    // 1. INDEX: Danh sách sản phẩm
    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllForAdminAsync();
        return View(products);
    }

    // 2. CREATE (GET): Hiển thị Form thêm mới kèm Dropdown
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadDropdownDataAsync();
        return View(new Product { IsActive = true, DisplayOrder = 1 });
    }

    // 3. CREATE (POST): Xử lý lưu DB
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        // 1. Bỏ qua việc validate các object quan hệ (Vì form chỉ gửi Id)
        ModelState.Remove(nameof(product.Category));
        ModelState.Remove(nameof(product.Brand));
        ModelState.Remove(nameof(product.MachineType));
        ModelState.Remove(nameof(product.Images));

        // 2. Kiểm tra lại dữ liệu
        if (!ModelState.IsValid)
        {
            await LoadDropdownDataAsync(); // Load lại dropdown nếu form lỗi
            return View(product);
        }

        // 3. Tự tạo URL SEO thân thiện
        // Lưu ý: Đảm bảo bạn đã using Cw.Branding.Web.Helpers; (chứa SlugHelper) ở đầu file
        product.SlugVi = SlugHelper.GenerateSlug(product.NameVi);
        product.SlugEn = string.IsNullOrEmpty(product.NameEn)
            ? product.SlugVi
            : SlugHelper.GenerateSlug(product.NameEn);

        // 4. Lưu vào DB
        var isSuccess = await _productService.CreateAsync(product);
        if (isSuccess)
        {
            TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Có lỗi xảy ra khi lưu vào database.");
        await LoadDropdownDataAsync();
        return View(product);
    }

    // --- Helper function để tái sử dụng ---
    private async Task LoadDropdownDataAsync()
    {
        // Giả định IBrandService và IMachineTypeService có hàm GetAllAsync() (Dựa theo Ngày 2)
        var categories = await _categoryService.GetAllAsync();
        var brands = await _brandService.GetAllAsync();
        var machineTypes = await _machineTypeService.GetAllAsync();

        ViewBag.Categories = new SelectList(categories, "Id", "NameVi");
        ViewBag.Brands = new SelectList(brands, "Id", "Name");
        ViewBag.MachineTypes = new SelectList(machineTypes, "Id", "NameVi");
    }
}