using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Services.Interfaces;
using Cw.Branding.Web.Data;
using Cw.Branding.Web.Models.Enums;


namespace Cw.Branding.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("{lang}/Admin/[controller]/{action=Index}")]
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
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Cần lấy danh sách Role để đổ vào Dropdown trong view
            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "Id", "Name");
            return View(new AppUser());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppUser user, string Password)
        {
            // 1. Kiểm tra trùng Username trong Database
            bool isExisted = await _context.Users.AnyAsync(u => u.Username == user.Username);
            if (isExisted)
            {
                // Gắn lỗi trực tiếp vào trường Username
                ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại. Vui lòng kiểm tra và nhập tên khác.");
            }

            // 2. Xử lý các validation khác (như password, roles...)
            ModelState.Remove(nameof(user.Role));
            ModelState.Remove(nameof(user.PasswordHash));

            if (string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError("Password", "Vui lòng nhập mật khẩu.");
            }

            // 3. Nếu mọi thứ ổn (không trùng tên, đủ mật khẩu)
            if (ModelState.IsValid)
            {
                user.PasswordHash = _accountService.HashPassword(Password);
                user.CreatedAt = DateTime.Now;

                _context.Add(user);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Tạo nhân viên thành công!";
                return RedirectToAction(nameof(Index));
            }

            // 4. Nếu có lỗi (trùng tên hoặc lỗi khác), load lại danh sách Role để hiển thị lại Form
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
        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "Id", "Name", user.RoleId);
            return View(user);
        }

        // [HttpPost] Edit
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppUser user)
        {
            if (id != user.Id) return NotFound();

            var originalUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (originalUser == null) return NotFound();

            // Loại bỏ các trường không update trong form này
            ModelState.Remove(nameof(user.Role));
            ModelState.Remove(nameof(user.PasswordHash));

            if (ModelState.IsValid)
            {
                // Giữ nguyên các giá trị không cho phép sửa ở màn hình này
                user.Username = originalUser.Username;
                user.PasswordHash = originalUser.PasswordHash;
                user.CreatedAt = originalUser.CreatedAt;

                // user.Region đã được bind từ form nên sẽ được lưu mới
                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Cập nhật thông tin nhân viên thành công.";
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