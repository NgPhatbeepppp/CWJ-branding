using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cw.Branding.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("{lang}/Admin/[controller]/[action]/{id?}")]
public class BrandController(IBrandService brandService, IWebHostEnvironment env) : BaseAdminController
{
    public async Task<IActionResult> Index()
    {
        var brands = await brandService.GetAllAsync();
        return View(brands);
    }

    public IActionResult Create() => View(new Brand { IsActive = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Brand brand, IFormFile? logoFile)
    {
        if (ModelState.IsValid)
        {
            if (logoFile != null)
            {
                brand.LogoPath = await SaveImage(logoFile);
            }
            await brandService.CreateAsync(brand);
            return RedirectToAction(nameof(Index));
        }
        return View(brand);
    }

    // GET: Admin/Brand/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var brand = await brandService.GetByIdAsync(id);
        if (brand == null) return NotFound();
        return View(brand);
    }

    // POST: Admin/Brand/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Brand brand, IFormFile? logoFile)
    {
        if (id != brand.Id) return NotFound();

        if (ModelState.IsValid)
        {
            if (logoFile != null)
            {
                // Nếu có upload ảnh mới -> Lưu ảnh mới và cập nhật Path
                brand.LogoPath = await SaveImage(logoFile);
            }
            // Nếu không upload ảnh mới, EF Core sẽ giữ nguyên giá trị cũ nếu ta không can thiệp

            await brandService.UpdateAsync(brand);
            return RedirectToAction(nameof(Index));
        }
        return View(brand);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await brandService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    // Helper xử lý lưu ảnh
    private async Task<string> SaveImage(IFormFile file)
    {
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var path = Path.Combine(env.WebRootPath, "uploads", "brands", fileName);

        using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/brands/{fileName}";
    }

}