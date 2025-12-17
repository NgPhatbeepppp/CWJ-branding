using Microsoft.AspNetCore.Mvc;
using Cw.Branding.Web.Models.ViewModels;

namespace Cw.Branding.Web.Controllers
{
    public class ContactController : Controller
    {
        [Route("{lang}/contact")]
        [Route("contact")] // Fallback
        public IActionResult Index()
        {
            // Set SEO Title
            ViewData["Title"] = "Contact Us";
            return View();
        }

        // Action xử lý Submit form sẽ làm ở bước sau (POST)
    }
}