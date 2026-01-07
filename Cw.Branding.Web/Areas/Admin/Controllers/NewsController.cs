using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cw.Branding.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(AuthenticationSchemes = "AdminCookie")]
public class NewsController : Controller
{
    private readonly INewsService _newsService;
    private readonly IWebHostEnvironment _env;

    public NewsController(INewsService newsService, IWebHostEnvironment env)
    {
        _newsService = newsService;
        _env = env;
    }

    // GET: Admin/News
    public async Task<IActionResult> Index()
    {
        var newsList = await _newsService.GetAllNewsAsync();
        return View(newsList);
    }

    // GET: Admin/News/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Admin/News/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(News news, IFormFile? thumbnail)
    {
        if (ModelState.IsValid)
        {
            // Handle thumbnail upload
            if (thumbnail != null && thumbnail.Length > 0)
            {
                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(thumbnail.FileName).ToLowerInvariant();
                
                if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("thumbnail", "Only image files (.jpg, .jpeg, .png, .gif, .webp) are allowed.");
                    return View(news);
                }
                
                // Validate file size (max 5MB)
                if (thumbnail.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("thumbnail", "File size must not exceed 5MB.");
                    return View(news);
                }
                
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "news");
                Directory.CreateDirectory(uploadsFolder);
                
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await thumbnail.CopyToAsync(fileStream);
                }
                
                news.ThumbnailPath = $"/uploads/news/{uniqueFileName}";
            }

            await _newsService.CreateNewsAsync(news);
            TempData["Success"] = "News created successfully!";
            return RedirectToAction(nameof(Index));
        }

        return View(news);
    }

    // GET: Admin/News/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var news = await _newsService.GetNewsByIdAsync(id);
        if (news == null)
        {
            return NotFound();
        }
        return View(news);
    }

    // POST: Admin/News/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, News news, IFormFile? thumbnail)
    {
        if (id != news.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            // Handle thumbnail upload
            if (thumbnail != null && thumbnail.Length > 0)
            {
                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(thumbnail.FileName).ToLowerInvariant();
                
                if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("thumbnail", "Only image files (.jpg, .jpeg, .png, .gif, .webp) are allowed.");
                    return View(news);
                }
                
                // Validate file size (max 5MB)
                if (thumbnail.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("thumbnail", "File size must not exceed 5MB.");
                    return View(news);
                }
                
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "news");
                Directory.CreateDirectory(uploadsFolder);
                
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await thumbnail.CopyToAsync(fileStream);
                }
                
                // Delete old thumbnail if exists
                if (!string.IsNullOrEmpty(news.ThumbnailPath))
                {
                    var oldFilePath = Path.Combine(_env.WebRootPath, news.ThumbnailPath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                
                news.ThumbnailPath = $"/uploads/news/{uniqueFileName}";
            }

            await _newsService.UpdateNewsAsync(news);
            TempData["Success"] = "News updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        return View(news);
    }

    // GET: Admin/News/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var news = await _newsService.GetNewsByIdAsync(id);
        if (news == null)
        {
            return NotFound();
        }
        return View(news);
    }

    // POST: Admin/News/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var news = await _newsService.GetNewsByIdAsync(id);
        
        // Delete thumbnail if exists
        if (news != null && !string.IsNullOrEmpty(news.ThumbnailPath))
        {
            var filePath = Path.Combine(_env.WebRootPath, news.ThumbnailPath.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        var result = await _newsService.DeleteNewsAsync(id);
        if (result)
        {
            TempData["Success"] = "News deleted successfully!";
        }
        else
        {
            TempData["Error"] = "Failed to delete news.";
        }

        return RedirectToAction(nameof(Index));
    }
}
