using Microsoft.AspNetCore.Mvc;

using Cw.Branding.Web.Models;


namespace Cw.Branding.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
