using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cw.Branding.Web.Areas.Admin.Controllers;


public class DashboardController : BaseAdminController
{
    public IActionResult Index()
    {
        return View();
    }
}
