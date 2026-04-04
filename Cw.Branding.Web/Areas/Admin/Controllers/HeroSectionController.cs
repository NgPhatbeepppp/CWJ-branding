using Cw.Branding.Web.Data;
using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Cw.Branding.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HeroSectionController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IFileService _fileService;

        public HeroSectionController(AppDbContext db, IFileService fileService)
        {
            _db = db;
            _fileService = fileService;
        }

        public async Task<IActionResult> Edit()
        {
            var hero = await _db.HeroSections.FirstOrDefaultAsync();
            if (hero == null) hero = new HeroSection(); // Nếu chưa có thì tạo object mới
            return View(hero);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(HeroSection model, IFormFile? uploadBg)
        {
            if (uploadBg != null)
            {
                if (!string.IsNullOrEmpty(model.BackgroundImage)) _fileService.DeleteImage(model.BackgroundImage);
                model.BackgroundImage = await _fileService.UploadImageAsync(uploadBg, "hero");
            }

            if (model.Id == 0) _db.HeroSections.Add(model);
            else _db.HeroSections.Update(model);

            await _db.SaveChangesAsync();
            TempData["Success"] = "Cập nhật Hero Banner thành công!";
            return RedirectToAction(nameof(Edit));
        }
    }
}
