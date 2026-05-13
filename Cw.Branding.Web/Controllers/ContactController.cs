using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Models.Enums;
using Cw.Branding.Web.Models.ViewModels;
using Cw.Branding.Web.Services;
using Cw.Branding.Web.Services.Interfaces;
using Cw.Branding.Web.Data; // 1. Đảm bảo có namespace chứa DbContext của bạn
using Microsoft.AspNetCore.Mvc;
using System.Globalization; // Cần thiết cho CultureInfo

namespace Cw.Branding.Web.Controllers
{
    [Route("{lang:regex(^(en|vi)$)}")]
    public class ContactController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly IContactService _contactService;
        private readonly AppDbContext _context; 
        private readonly ILogger<ContactController> _logger; 

        // 4. Cập nhật Constructor để nhận đủ 4 tham số
        public ContactController(
            IEmailService emailService,
            IContactService contactService,
            AppDbContext context,
            ILogger<ContactController> logger)
        {
            _emailService = emailService;
            _contactService = contactService;
            _context = context;
            _logger = logger;
        }

        [Route("contact")]
        [Route("lien-he")]
        [HttpGet]
        public IActionResult Index()
        {
            var lang = RouteData.Values["lang"]?.ToString();
            ViewData["Title"] = lang == "vi" ? "Liên hệ" : "Contact Us";
            return View(new ContactFormViewModel());
        }

        [HttpPost("contact/submit")]
        [HttpPost("lien-he/gui")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(ContactFormViewModel model)
        {
            var currentLang = RouteData.Values["lang"]?.ToString() ?? "en";

            if (!ModelState.IsValid) return View("Index", model);

            try
            {
                // 1. Lưu Database (Giống logic bạn đã làm ở Home)
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

                // 2. Gửi Email thông báo
                try
                {
                    string subject = (currentLang == "vi")
                        ? $"[CW Website] Liên hệ mới từ {model.Name}"
                        : $"[CW Website] New Lead: {model.Name}";

                    string body = $@"
                        <h3>Thông tin khách hàng mới:</h3>
                        <p><b>Họ tên:</b> {model.Name}</p>
                        <p><b>Email:</b> {model.Email}</p>
                        <p><b>Công ty:</b> {model.Company}</p>
                        <p><b>Nội dung:</b> {model.Message}</p>";

                    await _emailService.SendEmailAsync(subject, body);
                }
                catch (Exception emailEx)
                {
                    // Nếu lỗi mail thì chỉ log, không làm khách bị lỗi trang
                    _logger.LogWarning(emailEx, "Lưu DB thành công nhưng gửi mail thất bại.");
                }

                TempData["SuccessMessage"] = (currentLang == "vi") ? "Gửi thành công!" : "Submitted successfully!";
                return RedirectToAction("Index", new { lang = currentLang });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Contact Page Submit Error");
                ModelState.AddModelError("", "Error saving data.");
                return View("Index", model);
            }
        }
    }
}