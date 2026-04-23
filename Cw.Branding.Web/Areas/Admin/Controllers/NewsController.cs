using Microsoft.AspNetCore.Mvc;
using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Services.Interfaces;

namespace Cw.Branding.Web.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Route("{lang}/Admin/[controller]/{action=Index}")]
    public class NewsController : BaseAdminController
    {
        private readonly INewsService _newsService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public NewsController(INewsService newsService, IWebHostEnvironment webHostEnvironment)
        {
            _newsService = newsService;
            _webHostEnvironment = webHostEnvironment;
        }

        // 1. LIST
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var newsList = await _newsService.GetAllAsync();
            return View(newsList);
        }

        // 2. CREATE
        [HttpGet]
        public IActionResult Create()
        {
            // Khởi tạo giá trị mặc định cho form
            var model = new News
            {
                PublishedAt = DateTime.Now,
                IsActive = true
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(News news, IFormFile? thumbnailFile)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh Thumbnail
                if (thumbnailFile != null)
                {
                    news.ThumbnailPath = await HandleThumbnailUpload(thumbnailFile);
                }

                await _newsService.CreateAsync(news);
                TempData["Success"] = "Tạo bài viết mới thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu lỗi validation thì trả lại view để sửa
            return View(news);
        }

        // 3. EDIT
        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var news = await _newsService.GetByIdAsync(id);
            if (news == null)
            {
                return NotFound();
            }
            return View(news);
        }

        [HttpPost("{id}")] 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, News news, IFormFile? thumbnailFile)
        {
            if (id != news.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {          
                    if (thumbnailFile != null)
                    {        
                        news.ThumbnailPath = await HandleThumbnailUpload(thumbnailFile);
                    }
         

                    if (thumbnailFile == null)
                    {
                        var oldNews = await _newsService.GetByIdAsync(id);
                        if (oldNews != null)
                        {
                            news.ThumbnailPath = oldNews.ThumbnailPath;
                          
                        }
                    }

                    await _newsService.UpdateAsync(news);
                    TempData["Success"] = "Cập nhật bài viết thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi cập nhật: " + ex.Message);
                }
            }
            return View(news);
        }

        // 4. DELETE
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _newsService.DeleteAsync(id);
            TempData["Success"] = "Đã xóa bài viết!";
            return RedirectToAction(nameof(Index));
        }

        // --- HELPER: Upload Image ---
        private async Task<string> HandleThumbnailUpload(IFormFile file)
        {
            // 1. Check path
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string uploadPath = Path.Combine(wwwRootPath, "uploads", "news-thumbnails");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // 2. Rename file (GUID)
            string extension = Path.GetExtension(file.FileName);
            string fileName = $"{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(uploadPath, fileName);

            // 3. Save
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // 4. Return relative path for DB
            return $"/uploads/news-thumbnails/{fileName}";
        }
    }
}