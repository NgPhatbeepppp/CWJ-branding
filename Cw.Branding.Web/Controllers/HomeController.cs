using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cw.Branding.Web.Data;
using Cw.Branding.Web.Models.ViewModels;
using Cw.Branding.Web.Models;
using Cw.Branding.Web.Models.Entities;
using System.Diagnostics;
using Cw.Branding.Web.Models.Enums;

namespace Cw.Branding.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel();

            try
            {
                // Fetch Latest News
                viewModel.LatestNews = await _context.News
                     // 1. Đổi n.IsPublished -> n.IsActive
                     .Where(n => n.IsActive && n.PublishedAt <= DateTime.Now)
                     // 2. Sắp xếp theo ngày đăng mới nhất (thay vì sort boolean)
                     .OrderByDescending(n => n.PublishedAt)
                     .Take(3)
                     .ToListAsync();


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Homepage data");
            }

            return View(viewModel);
        }

        [HttpPost("{lang}/Home/Submit")] // Thêm {lang} để bắt đúng ngôn ngữ từ form
        public async Task<IActionResult> Submit(string lang, ContactFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var contactEntry = new ContactFormEntry
                    {
                        Name = model.Name,
                        Company = model.Company,        // Lưu công ty
                        Email = model.Email,
                        Phone = model.Phone,            // Lưu số điện thoại
                        SelectedProduct = model.SelectedProduct,
                        Region = model.Region,          // Lưu khu vực (Bắc/Trung/Nam)
                        Message = model.Message,
                        CreatedAt = DateTime.UtcNow,

                        // Các trường quản trị mới (Ticket CWJ-501)
                        IsRead = false,
                        ProcessingStatus = ContactStatus.New
                    };

                    _context.ContactFormEntries.Add(contactEntry);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Contact from {Email} in {Region}", model.Email, model.Region);

                    // Phản hồi đa ngôn ngữ [cite: 83]
                    TempData["SuccessMessage"] = (lang == "vi")
                        ? "Cảm ơn bạn! Thông tin của bạn đã được gửi đến chi nhánh phù hợp."
                        : "Thank you! Your inquiry has been sent to the relevant regional office.";

                    return RedirectToAction("Index", "Home", new { lang = lang });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving contact form");
                    TempData["ErrorMessage"] = "Error! Please try again.";
                }
            }

            return RedirectToAction("Index", "Home", new { lang = lang });
        }
        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}