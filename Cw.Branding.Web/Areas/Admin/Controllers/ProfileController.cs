using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Services.Interfaces;
using Cw.Branding.Web.Data;

namespace Cw.Branding.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("{lang}/Admin/[controller]/{action=Index}")]
    public class ProfileController : BaseAdminController
    {
        private readonly AppDbContext _context;
        private readonly IAccountService _accountService;

        public ProfileController(AppDbContext context, IAccountService accountService)
        {
            _context = context;
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            // 1. Kiểm tra khớp mật khẩu mới
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu mới và xác nhận mật khẩu không khớp.");
                return View();
            }

            // 2. Lấy UserId từ Claims đã lưu khi Login
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();

            int userId = int.Parse(userIdClaim);
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound();

            // 3. Xác minh mật khẩu hiện tại
            if (!_accountService.VerifyPassword(user.PasswordHash, currentPassword))
            {
                ModelState.AddModelError("", "Mật khẩu hiện tại không chính xác.");
                return View();
            }

            // 4. Cập nhật mật khẩu mới
            user.PasswordHash = _accountService.HashPassword(newPassword);
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Đổi mật khẩu thành công!";
            return View();
        }
    }
}