using Cw.Branding.Web.Models.Enums;
using Cw.Branding.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cw.Branding.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("{lang}/Admin/[controller]/[action]/{id?}")]
    public class ContactController : BaseAdminController
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        public async Task<IActionResult> Index(ContactStatus? status)
        {
            ViewBag.CurrentStatus = status;

            // 1. Lấy thông tin Role và Region của người dùng hiện tại từ Claims
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userRegionClaim = User.FindFirst("UserRegion")?.Value;

            // 2. Xác định vùng cần lọc
            ContactRegion? filterRegion = null;

            // Nếu là nhân viên (không phải Admin) và có thông tin vùng trong Claim
            if (userRole != "Admin" && Enum.TryParse<ContactRegion>(userRegionClaim, out var region))
            {
                filterRegion = region;
            }
            // Note: Nếu là Admin, filterRegion vẫn là null -> service sẽ lấy tất cả các miền.

            // 3. Gọi service với tham số đã tính toán
            var leads = await _contactService.GetContactsAsync(status, filterRegion);

            return View(leads);
        }
       
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // Giả định dùng User.Identity.Name để lưu vết người đọc. 
            // Nếu chưa có Identity thì tạm để "SystemAdmin"
            var adminId = User.Identity?.Name ?? "SystemAdmin";

            var lead = await _contactService.GetDetailsAndMarkAsReadAsync(id, adminId);

            if (lead == null) return NotFound();

            return View(lead);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ContactStatus status, string? adminNotes)
        {
            var result = await _contactService.UpdateStatusAsync(id, status, adminNotes);

            if (result)
            {
                TempData["SuccessMessage"] = "Status updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update status.";
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        [HttpGet]
        public async Task<IActionResult> Export(ContactStatus? status)
        {
            var fileContent = await _contactService.ExportToExcelAsync(status);
            string fileName = $"Leads_Report_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

            return File(
                fileContent,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }
    }
}