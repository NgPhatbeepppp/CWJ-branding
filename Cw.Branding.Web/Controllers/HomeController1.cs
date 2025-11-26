using Microsoft.AspNetCore.Mvc;

namespace Cw.Branding.Web.Controllers
{
    public class HomeController1 : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
