using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cw.Branding.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize] // <--- Tất cả controller kế thừa từ đây đều sẽ được bảo mật
    public class BaseAdminController : Controller
    {
        // Anh có thể thêm các logic dùng chung cho toàn bộ Admin ở đây nếu cần
    }
}