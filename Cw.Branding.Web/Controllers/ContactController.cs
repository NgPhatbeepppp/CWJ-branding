using Microsoft.AspNetCore.Mvc;
using Cw.Branding.Web.Models.ViewModels;

namespace Cw.Branding.Web.Controllers
{
    [Route("{lang:regex(^(en|vi)$)}")]
    public class ContactController : Controller
    {
        [Route("contact")]
        [Route("lien-he")]
        public IActionResult Index()
        {
            var lang = RouteData.Values["lang"]?.ToString();
            ViewData["Title"] = lang == "vi" ? "Liên hệ" : "Contact Us";
            return View();
        }
       
    }
}