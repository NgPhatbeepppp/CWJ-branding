using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Cw.Branding.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("{lang}/Admin/[controller]/{action=Index}")]
public class CategoryController : BaseAdminController
{
    private readonly ICategoryService _categoryService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public CategoryController(ICategoryService categoryService, IWebHostEnvironment webHostEnvironment)
    {
        _categoryService = categoryService;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.GetAllAsync();
        return View(categories);
    }

   
    [HttpGet]
    public IActionResult Create()
    {
        return View(new Category { IsActive = true, DisplayOrder = 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category, IFormFile? iconFile)
    {
        // Lấy giá trị lang hiện tại từ route để truyền vào Redirect
        var currentLang = RouteData.Values["lang"]?.ToString() ?? "vi";

        if (ModelState.IsValid)
        {
            if (iconFile != null)
            {
                category.IconPath = await HandleUploadIcon(iconFile);
            }

            var result = await _categoryService.CreateAsync(category);
            if (result)
            {
                TempData["Success"] = "Thêm danh mục mới thành công!";
                // Quan trọng: Phải truyền lang vào để RedirectToAction sinh đúng URL
                return RedirectToAction(nameof(Index), new { lang = currentLang });
            }
            ModelState.AddModelError("", "Có lỗi xảy ra khi lưu dữ liệu.");
        }
        return View(category);
    }

    // Đường dẫn: /vi/Admin/Category/Edit/5
    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null) return NotFound();
        return View(category);
    }

    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category, IFormFile? iconFile)
    {
        var currentLang = RouteData.Values["lang"]?.ToString() ?? "vi";

        if (id != category.Id) return NotFound();

        if (ModelState.IsValid)
        {
            if (iconFile != null)
            {
                category.IconPath = await HandleUploadIcon(iconFile);
            }

            var result = await _categoryService.UpdateAsync(category);
            if (result)
            {
                TempData["Success"] = "Cập nhật thành công!";
                return RedirectToAction(nameof(Index), new { lang = currentLang });
            }
            ModelState.AddModelError("", "Không thể cập nhật danh mục.");
        }
        return View(category);
    }

    // Đường dẫn AJAX: /vi/Admin/Category/Delete/5
    [HttpPost("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoryService.DeleteAsync(id);
        if (result)
        {
            return Json(new { success = true, message = "Xóa thành công!" });
        }
        return Json(new { success = false, message = "Không thể xóa danh mục này." });
    }

    private async Task<string> HandleUploadIcon(IFormFile file)
    {
        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/categories");
        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        string filePath = Path.Combine(uploadsFolder, fileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return "/uploads/categories/" + fileName;
    }
}