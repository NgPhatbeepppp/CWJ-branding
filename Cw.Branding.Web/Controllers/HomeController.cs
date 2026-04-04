using Cw.Branding.Web.Data;
using Cw.Branding.Web.Models;
using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Models.Enums;
using Cw.Branding.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;

namespace Cw.Branding.Web.Controllers
{
    [Route("{lang:regex(^(en|vi)$)}")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        [HttpGet]
        [Route("", Name = "HomeIndex")]
        public async Task<IActionResult> Index()
        {
            var currentLang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLower();
            var viewModel = new HomeViewModel(); // Đã có sẵn default bên trong

            try
            {
                // Lấy Hero từ DB
                var dbHero = await _context.HeroSections.AsNoTracking().FirstOrDefaultAsync();
                if (dbHero != null) viewModel.Hero = dbHero;

                // Lấy Slides - Nếu DB trống, mình sẽ tự add 1 slide mặc định để không hỏng Ken Burns
                var dbSlides = await _context.HomeSlides.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ToListAsync();
                if (dbSlides.Any()) viewModel.Slides = dbSlides;
                else viewModel.Slides.Add(new HomeSlide { ImageUrl = "/images/healthcare-hero.png" });

                // Lấy News (Cần thiết để không mất mục News ở dưới)
                viewModel.LatestNews = await _context.News
                    .Where(n => n.IsActive)
                    .OrderByDescending(n => n.PublishedAt)
                    .Take(3).ToListAsync();

                // SEO
                viewModel.MetaTitle = (currentLang == "vi") ? "Trang chủ - Charles Wembley" : "Home - Charles Wembley";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi load trang chủ, đang dùng dữ liệu dự phòng.");
            }

            return View(viewModel);
        }

        [HttpPost("Home/Submit")] 
        public async Task<IActionResult> Submit(string lang, ContactFormViewModel model)
        {
            var currentLang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

            if (ModelState.IsValid)
            {
                try
                {
                    var contactEntry = new ContactFormEntry
                    {
                        Name = model.Name,
                        Company = model.Company,
                        Email = model.Email,
                        Phone = model.Phone,
                        SelectedProduct = model.SelectedProduct,
                        Region = model.Region,
                        Message = model.Message,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false,
                        ProcessingStatus = ContactStatus.New
                    };

                    _context.ContactFormEntries.Add(contactEntry);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = (currentLang == "vi")
                        ? "Cảm ơn bạn! Thông tin đã được gửi thành công."
                        : "Thank you! Your inquiry has been submitted successfully.";

                    return RedirectToAction("Index", "Home", new { lang = currentLang });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving contact form");
                    TempData["ErrorMessage"] = "Error! Please try again.";
                }
            }

            return RedirectToAction("Index", "Home", new { lang = currentLang });
        }
        [Route("privacy")]
        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("error")]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    
    }
}