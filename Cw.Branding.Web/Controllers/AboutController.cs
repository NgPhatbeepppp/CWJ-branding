using Microsoft.AspNetCore.Mvc;
using Cw.Branding.Web.Models.ViewModels;

namespace Cw.Branding.Web.Controllers
{
    [Route("{lang:regex(^(en|vi)$)}")]
    public class AboutController : Controller
    {

        [HttpGet("about-us")]
        [HttpGet("ve-chung-toi")]
        public IActionResult Index(string lang) 
        {
            
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