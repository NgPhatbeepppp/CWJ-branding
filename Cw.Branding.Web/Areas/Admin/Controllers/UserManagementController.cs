using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Services.Interfaces;
using Cw.Branding.Web.Data;

namespace Cw.Branding.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
  
    public class UserManagementController : BaseAdminController
    {
        private readonly AppDbContext _context;
        private readonly IAccountService _accountService;

        public UserManagementController(AppDbContext context, IAccountService accountService)
        {
            _context = context;
            _accountService = accountService;
        }

        // Task 3.2: Danh sách User
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.Include(u => u.Role).ToListAsync();
            return View(users);
        }

        // Task 3.2: View Tạo mới
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppUser user, string Password)
        {
            // 1. Kiểm tra trùng Username
            bool isExisted = await _context.Users.AnyAsync(u => u.Username == user.Username);
            if (isExisted)
            {
                ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại.");
            }

            // 2. LOẠI BỎ VALIDATION CHO CÁC TRƯỜNG KHÔNG GỬI TỪ FORM
            ModelState.Remove(nameof(user.Role));
            ModelState.Remove(nameof(user.PasswordHash));
            ModelState.Remove(nameof(user.Username)); // Nếu model có [Required] nhưng bị lỗi format

            // 3. GÁN PASSWORD HASH TRƯỚC KHI CHECK VALID
            if (!string.IsNullOrEmpty(Password))
            {
                user.PasswordHash = _accountService.HashPassword(Password);
            }
            else
            {
                ModelState.AddModelError("Password", "Vui lòng nhập mật khẩu.");
            }

            if (ModelState.IsValid)
            {
                user.CreatedAt = DateTime.Now;
                _context.Add(user);
                await _context.SaveChangesAsync();

                TempData["Message"] = $"Đã tạo thành công tài khoản: {user.Username}";
                return RedirectToAction(nameof(Index));
            }

            // Nếu lỗi, load lại Roles để hiển thị lại Form
            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "Id", "Name", user.RoleId);
            return View(user);
        }

        // Task 3.3: Logic Reset Password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Reset về mật khẩu mặc định
            user.PasswordHash = _accountService.HashPassword("Wembley@123");
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Đã reset mật khẩu cho {user.Username} về mặc định: Wembley@123";
            return RedirectToAction(nameof(Index));
        }
        // [HttpGet] Edit
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "Id", "Name", user.RoleId);
            return View(user);
        }

        // [HttpPost] Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppUser user)
        {
            if (id != user.Id) return NotFound();

            // Loại bỏ kiểm tra Username ở đây vì ta đã để Username là Hidden/Read-only
            // Nhưng để chắc chắn, ta lấy lại dữ liệu gốc từ DB
            var originalUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (originalUser == null) return NotFound();

            if (ModelState.IsValid)
            {
                // Luôn giữ nguyên Username và Password cũ
                user.Username = originalUser.Username;
                user.PasswordHash = originalUser.PasswordHash;
                user.CreatedAt = originalUser.CreatedAt;

                _context.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "Id", "Name", user.RoleId);
            return View(user);
        }
        [HttpGet]
        public async Task<JsonResult> CheckUsername(string username)
        {
            bool isExisted = await _context.Users.AnyAsync(u => u.Username == username);
            return Json(new { isAvailable = !isExisted });
        }

    }
}