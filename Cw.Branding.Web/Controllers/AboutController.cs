using Microsoft.AspNetCore.Mvc;
using Cw.Branding.Web.Models.ViewModels;

namespace Cw.Branding.Web.Controllers
{
    public class AboutController : Controller
    {
        // THÊM: {lang} vào trước đường dẫn để bắt được /en/ hoặc /vi/
        [Route("{lang}/about-us")]
        [Route("{lang}/ve-chung-toi")]
        public IActionResult Index(string lang) // Nhận tham số lang nếu cần dùng
        {
            // Set ngôn ngữ hiện tại để View dùng (nếu cần đổi link EN/VN)
            ViewData["CurrentLang"] = lang ?? "en";

            var viewModel = new AboutViewModel
            {
                MetaTitle = "About Us - Charles Wembley Medical",
                MetaDescription = "Discover our story, vision, and commitment to healthcare innovation."
            };

            return View(viewModel);
        }
    }
}