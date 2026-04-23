using Cw.Branding.Web.Data;
using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cw.Branding.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("{lang}/Admin/[controller]/{action=Index}")]
    public class HomeSlideController : BaseAdminController
    {
        private readonly AppDbContext _context;
        private readonly IFileService _fileService;

        public HomeSlideController(AppDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // 1. Trang danh sách
        public async Task<IActionResult> Index()
        {
            var slides = await _context.HomeSlides.OrderByDescending(x => x.Id).ToListAsync();
            return View(slides);
        }

        // 2. Xử lý Upload nhiều file hoặc 1 file (Post)
        [HttpPost]
        public async Task<IActionResult> Upload(List<IFormFile> imageFiles, string? note)
        {
            if (imageFiles != null && imageFiles.Any())
            {
                int uploadCount = 0;
                foreach (var file in imageFiles)
                {
                    if (file.Length > 0)
                    {
                        // Gọi service upload từng file
                        var path = await _fileService.UploadImageAsync(file, "slides");

                        var slide = new HomeSlide
                        {
                            ImageUrl = path,
                            // Nếu admin nhập ghi chú thì dùng ghi chú đó, 
                            // không thì lấy tên file gốc để admin dễ phân biệt
                            Note = !string.IsNullOrEmpty(note) ? note : file.FileName,
                            IsActive = true,
                            DisplayOrder = 0
                        };

                        _context.HomeSlides.Add(slide);
                        uploadCount++;
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã tải lên thành công {uploadCount} ảnh!";
            }
            else
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một tệp ảnh.";
            }

            return RedirectToAction(nameof(Index));
        }

        // 3. Bật/Tắt hiển thị
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var slide = await _context.HomeSlides.FindAsync(id);
            if (slide != null)
            {
                slide.IsActive = !slide.IsActive;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // 4. Xóa ảnh
        [HttpPost("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var slide = await _context.HomeSlides.FindAsync(id);
            if (slide != null)
            {
                _fileService.DeleteImage(slide.ImageUrl);
                _context.HomeSlides.Remove(slide);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã xóa ảnh.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
