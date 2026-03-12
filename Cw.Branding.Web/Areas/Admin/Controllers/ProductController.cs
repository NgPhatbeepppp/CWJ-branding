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

    // 3. CREATE (POST): Xử lý lưu DB có kèm ảnh và chọn ảnh bìa
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product, List<IFormFile> uploadImages, string? mainImageName)
    {
        ModelState.Remove(nameof(product.Category));
        ModelState.Remove(nameof(product.Brand));
        ModelState.Remove(nameof(product.MachineType));
        ModelState.Remove(nameof(product.Images));

        // Validation: Cảnh báo nếu upload file không đúng định dạng ảnh
        if (uploadImages != null && uploadImages.Count > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            foreach (var file in uploadImages)
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("", $"File '{file.FileName}' không hợp lệ. Hệ thống chỉ chấp nhận JPG, PNG, WEBP.");
                    await LoadDropdownDataAsync();
                    return View(product);
                }
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadDropdownDataAsync();
            return View(product);
        }

        product.SlugVi = SlugHelper.GenerateSlug(product.NameVi);
        product.SlugEn = string.IsNullOrEmpty(product.NameEn)
            ? product.SlugVi
            : SlugHelper.GenerateSlug(product.NameEn);

        // Gọi hàm CreateWithImagesAsync truyền thêm mainImageName vào
        var isSuccess = await _productService.CreateWithImagesAsync(product, uploadImages, mainImageName);

        if (isSuccess)
        {
            TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Có lỗi xảy ra khi lưu vào database.");
        await LoadDropdownDataAsync();
        return View(product);
    }

    // 4. EDIT (GET): Load data lên form
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null) return NotFound();

        await LoadDropdownDataAsync();
        return View(product);
    }

    // 5. EDIT (POST): Xử lý cập nhật
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product, List<IFormFile> uploadImages, List<int> deletedImageIds, int? mainImageId, string? mainImageName)
    {
        if (id != product.Id) return BadRequest();

        ModelState.Remove(nameof(product.Category));
        ModelState.Remove(nameof(product.Brand));
        ModelState.Remove(nameof(product.MachineType));
        ModelState.Remove(nameof(product.Images));

        if (!ModelState.IsValid)
        {
            await LoadDropdownDataAsync();
            var existingProduct = await _productService.GetByIdAsync(id);
            product.Images = existingProduct?.Images ?? new List<ProductImage>();
            return View(product);
        }

        product.SlugVi = SlugHelper.GenerateSlug(product.NameVi);
        product.SlugEn = string.IsNullOrEmpty(product.NameEn) ? product.SlugVi : SlugHelper.GenerateSlug(product.NameEn);

        // Truyền cả ID ảnh cũ và Tên ảnh mới xuống Service
        var isSuccess = await _productService.UpdateProductWithImagesAsync(product, uploadImages, deletedImageIds, mainImageId, mainImageName);

        if (isSuccess)
        {
            TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Lỗi khi lưu Database.");
        await LoadDropdownDataAsync();
        return View(product);
    }
    // 6. DELETE (POST): Xóa sản phẩm và dọn dẹp file
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var isSuccess = await _productService.DeleteAsync(id);

        if (isSuccess)
        {
            TempData["SuccessMessage"] = "Đã xóa sản phẩm và toàn bộ hình ảnh liên quan thành công!";
        }
        else
        {
            TempData["ErrorMessage"] = "Không tìm thấy sản phẩm hoặc có lỗi xảy ra khi xóa.";
        }

        return RedirectToAction(nameof(Index));
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