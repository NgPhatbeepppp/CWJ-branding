using Cw.Branding.Web.Data;
using Cw.Branding.Web.Models;
using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Models.Enums;
using Cw.Branding.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;
using Cw.Branding.Web.Services.Interfaces; 
using Cw.Branding.Web.Services; 
namespace Cw.Branding.Web.Controllers
{
    [Route("{lang:regex(^(en|vi)$)}")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public HomeController(ILogger<HomeController> logger, AppDbContext context, IEmailService emailService)
        {
            _logger = logger;
            _context = context;
            _emailService = emailService;
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

        // Đừng quên inject IEmailService vào Constructor của HomeController nhé!
        [HttpPost("Home/Submit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(string lang, ContactFormViewModel model)
        {
            var currentLang = lang ?? CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Lưu Database (Logic cũ của bạn)
                    var contactEntry = new ContactFormEntry
                    {
                        Name = model.Name,
                        Company = model.Company,
                        Email = model.Email,
                        Phone = model.Phone,                        
                        Region = model.Region,
                        Message = model.Message,
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false,
                        ProcessingStatus = ContactStatus.New
                    };

                    _context.ContactFormEntries.Add(contactEntry);
                    await _context.SaveChangesAsync();

                    // 2. Gửi Email thông báo (Cập nhật mới theo RFC-001)[cite: 1]
                    try
                    {
                        string subject = $"[CW Website] New Inquiry from {model.Name} ({model.Region})";
                        string emailBody = $@"
                    <h3>Thông tin khách hàng liên hệ:</h3>
                    <p><b>Họ tên:</b> {model.Name}</p>
                    <p><b>Công ty:</b> {model.Company}</p>
                    <p><b>Email:</b> {model.Email}</p>
                    <p><b>Điện thoại:</b> {model.Phone}</p>                    
                    <p><b>Khu vực:</b> {model.Region}</p>
                    <p><b>Nội dung:</b> {model.Message}</p>
                    <hr/>
                    <p><i>Vui lòng đăng nhập Admin để xử lý yêu cầu.</i></p>";

                        await _emailService.SendEmailAsync(subject, emailBody);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Lỗi gửi mail nhưng vẫn tiếp tục flow thành công cho khách");
                    }

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