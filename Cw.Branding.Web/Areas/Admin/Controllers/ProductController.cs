using Cw.Branding.Web.Helpers; 
using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Models.Import;
using Cw.Branding.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Cw.Branding.Web.Areas.Admin.Controllers;


[Area("Admin")]
[Route("{lang}/Admin/[controller]/{action=Index}")]
public class ProductController : BaseAdminController
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IBrandService _brandService;
    private readonly IMachineTypeService _machineTypeService;
    private readonly IProductImportService _importService;
    public ProductController(
        IProductService productService,
        ICategoryService categoryService,
        IBrandService brandService,
        IMachineTypeService machineTypeService,
        IProductImportService importService)
        
        
    {
        _productService = productService;
        _categoryService = categoryService;
        _brandService = brandService;
        _machineTypeService = machineTypeService;
        _importService = importService;

    }

    // 1. INDEX: Danh sách sản phẩm
    // Areas/Admin/Controllers/ProductController.cs
    public async Task<IActionResult> Index(string? searchTerm, int? categoryId, int? brandId, bool? isActive)
    {
        // 1. Lấy danh sách sản phẩm theo tất cả điều kiện lọc
        var products = await _productService.GetAllForAdminAsync(searchTerm, categoryId, brandId, isActive);

        // 2. Load lại Dropdown nhưng phải truyền kèm "Selected Value"
        var categories = await _categoryService.GetAllAsync();
        var brands = await _brandService.GetAllAsync();
        var machineTypes = await _machineTypeService.GetAllAsync();

        // Tham số thứ 4 là giá trị đang được chọn
        ViewBag.Categories = new SelectList(categories, "Id", "NameVi", categoryId);
        ViewBag.Brands = new SelectList(brands, "Id", "Name", brandId);

        // Lưu lại searchTerm và isActive để hiển thị lại trên View
        ViewBag.SearchTerm = searchTerm;
        ViewBag.IsActive = isActive;

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
        // 1. Kiểm tra trùng mã ngay lập tức
        if (await _productService.IsCodeExistsAsync(product.Code))
        {
            ModelState.AddModelError("Code", "Mã sản phẩm này đã tồn tại trên hệ thống. Vui lòng nhập mã khác.");
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
    [HttpGet("{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null) return NotFound();

        await LoadDropdownDataAsync();
        return View(product);
    }

    // 5. EDIT (POST): Xử lý cập nhật
    [HttpPost("{id}")]
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
    [HttpPost("{id}")]
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
    private async Task LoadDropdownDataAsync(Product? product = null)
    {
        var categories = await _categoryService.GetAllAsync();
        var brands = await _brandService.GetAllAsync();
        var machineTypes = await _machineTypeService.GetAllAsync();

        // Truyền thêm product.Id vào tham số thứ 4 để SelectList biết cái nào đang được chọn
        ViewBag.Categories = new SelectList(categories, "Id", "NameVi", product?.CategoryId);
        ViewBag.Brands = new SelectList(brands, "Id", "Name", product?.BrandId);
        ViewBag.MachineTypes = new SelectList(machineTypes, "Id", "NameVi", product?.MachineTypeId);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateOrder(int id, int order)
    {
        var result = await _productService.UpdateDisplayOrderAsync(id, order);
        if (result)
        {
            return Json(new { success = true });
        }
        return Json(new { success = false, message = "Lỗi lưu DB" });
    }
    // --- LUỒNG BULK IMPORT ---

    // 1. Trang Upload: /vi/Admin/Product/Import
    [HttpGet]
    public IActionResult Import()
    {
        return View();
    }

    // 2. Xử lý Validate file (Dry Run)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Validate(IFormFile excelFile)
    {
        if (excelFile == null || excelFile.Length == 0)
        {
            TempData["Error"] = "Vui lòng chọn một file Excel (.xlsx) hợp lệ.";
            return RedirectToAction(nameof(Import));
        }

        // Đọc và kiểm tra dữ liệu từ Excel
        using var stream = excelFile.OpenReadStream();
        var result = await _importService.ValidateExcelAsync(stream);

        // Nếu có lỗi (dù chỉ 1 dòng): Quay lại trang Import và hiển thị bảng lỗi
        if (result.ErrorRows > 0)
        {
            TempData["Error"] = $"Phát hiện {result.ErrorRows} dòng dữ liệu không hợp lệ. Vui lòng sửa lại file.";
            return View("Import", result);
        }

        // Nếu 100% hợp lệ: Chuyển sang màn hình Review để Admin xem lại lần cuối
        return View("Review", result);
    }

    // 3. Xác nhận lưu dữ liệu (Final Commit)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(string jsonData)
    {
        if (string.IsNullOrEmpty(jsonData))
        {
            TempData["Error"] = "Dữ liệu xác nhận bị trống. Vui lòng thử lại.";
            return RedirectToAction(nameof(Import));
        }

        // Giải mã JSON ngược lại thành List Model
        var validRows = JsonConvert.DeserializeObject<List<ProductImportRow>>(jsonData);

        if (validRows == null || !validRows.Any())
        {
            TempData["Error"] = "Không tìm thấy dữ liệu hợp lệ để Import.";
            return RedirectToAction(nameof(Import));
        }

        // Thực hiện lưu vào DB thông qua Transaction
        var (success, message) = await _importService.CommitImportAsync(validRows);

        if (success)
        {
            TempData["SuccessMessage"] = message; 

            // Lấy ngôn ngữ hiện tại từ Route để redirect về đúng /vi/ hoặc /en/
            var currentLang = RouteData.Values["lang"] ?? "vi";

            return RedirectToAction("Index", "Product", new { lang = currentLang, area = "Admin" });
        }
        else
        {
            TempData["Error"] = message;
            return RedirectToAction(nameof(Import), new { lang = RouteData.Values["lang"] ?? "vi", area = "Admin" });
        }
    }
}