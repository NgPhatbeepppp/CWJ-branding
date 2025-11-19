using Microsoft.AspNetCore.Mvc;

namespace Cw.Branding.Web.Controllers;

public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index(string lang = "en")
    {
        ViewBag.Lang = lang;
        ViewData["Title"] = lang == "vi" ? "Trang chủ" : "Home";
        return View();
    }
}
