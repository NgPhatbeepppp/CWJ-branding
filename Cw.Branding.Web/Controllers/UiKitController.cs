using Microsoft.AspNetCore.Mvc;
using Cw.Branding.Web.Models.Components;

namespace Cw.Branding.Web.Controllers
{
    public class UiKitController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}