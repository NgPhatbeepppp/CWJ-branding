using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Cw.Branding.Web.Services.Interfaces;

namespace Cw.Branding.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    // Route này giúp đồng bộ với hệ thống đa ngôn ngữ hiện tại của site
    [Route("{lang}/Admin/[controller]/[action]")]
    public class AuthController : Controller
    {
        private readonly IAccountService _accountService;

        public AuthController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string returnUrl = null)
        {
            var user = await _accountService.LoginAsync(username, password);

            if (user == null)
            {
                ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không đúng.");
                return View();
            }

            // 1. Tạo danh sách Claims (Quyền hạn)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Username), // Lưu username vào NameIdentifier
                new Claim(ClaimTypes.Name, user.FullName ?? ""),    // Lưu FullName vào Name
                new Claim(ClaimTypes.Role, user.Role.Name),
                new Claim("UserRegion", ((int)user.Region).ToString()),
                new Claim("UserId", user.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // 2. Đăng nhập vào hệ thống Cookie
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = true });

            // 3. Redirect về trang yêu cầu hoặc trang chủ Admin
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        [HttpPost] // Thêm cái này để nhận lệnh từ Form POST
        [ValidateAntiForgeryToken] // Bảo mật chống giả mạo request
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Về trang Login sau khi thoát
            return RedirectToAction("Login", "Auth", new { area = "Admin" });
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}