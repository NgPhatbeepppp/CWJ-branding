using Microsoft.AspNetCore.Mvc;
using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Services.Interfaces;

namespace Cw.Branding.Web.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Route("{lang}/Admin/[controller]/[action]/{id?}")]
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
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var news = await _newsService.GetByIdAsync(id);
            if (news == null)
            {
                return NotFound();
            }
            return View(news);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, News news, IFormFile? thumbnailFile)
        {
            if (id != news.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Nếu user upload ảnh mới -> thay thế ảnh cũ
                    if (thumbnailFile != null)
                    {
                        // (Optional: Xóa ảnh cũ nếu cần, nhưng MVP thì cứ ghi đè path)
                        news.ThumbnailPath = await HandleThumbnailUpload(thumbnailFile);
                    }
                    // Nếu không upload ảnh mới -> logic Service sẽ giữ nguyên Path cũ (cần check lại Service một chút để chắc chắn)
                    // Ở Service Task 2: chúng ta đã code: existingNews.ThumbnailPath = news.ThumbnailPath;
                    // -> Vấn đề: Nếu ở View Edit không gửi lại path cũ, nó sẽ thành null.
                    // -> FIX nhanh ở đây: Lấy bài cũ ra để giữ path nếu file mới null.

                    if (thumbnailFile == null)
                    {
                        var oldNews = await _newsService.GetByIdAsync(id);
                        if (oldNews != null)
                        {
                            news.ThumbnailPath = oldNews.ThumbnailPath;
                            // Entity Framework tracking issue workaround: 
                            // Vì Service sẽ fetch lại, nên ở đây chỉ cần gán path vào object gửi đi là đủ.
                            // Tuy nhiên để an toàn nhất, logic giữ ảnh cũ nên nằm trong Service hoặc View phải có hidden field.
                            // Ở đây ta chọn giải pháp: View sẽ có Hidden Input cho ThumbnailPath.
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
        [HttpPost]
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