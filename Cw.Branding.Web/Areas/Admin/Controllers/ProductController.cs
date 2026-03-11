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
    // 3. EDIT (GET): Hiển thị Form sửa với dữ liệu cũ
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        // Giả định trong IProductService anh đã có hàm GetByIdAsync
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        await LoadDropdownDataAsync();
        return View(product);
    }

    // 4. EDIT (POST): Lưu dữ liệu cập nhật
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            // Giả định IProductService có hàm UpdateAsync
            await _productService.UpdateAsync(model);

            // Có thể thêm TempData["Success"] = "Cập nhật thành công"; ở đây
            return RedirectToAction(nameof(Index));
        }

        // Nếu có lỗi validation, load lại dropdown và trả về form
        await LoadDropdownDataAsync();
        return View(model);
    }
    [HttpPost]
    [Route("Admin/Product/UploadEditorImage")]
    public async Task<IActionResult> UploadEditorImage(IFormFile file, [FromServices] IWebHostEnvironment env)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Invalid file.");
        }

        // Đảm bảo thư mục tồn tại
        var uploadsFolder = Path.Combine(env.WebRootPath, "uploads", "products");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // Tạo tên file an toàn
        var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Trả về URL ảnh để TinyMCE hiển thị
        var fileUrl = $"/uploads/products/{uniqueFileName}";
        return Json(new { location = fileUrl });
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