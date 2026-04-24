using Cw.Branding.Web.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Cw.Branding.Web.Models.Entities
{
    public class ContactFormEntry
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(200)]
        public string? Company { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = null!;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }

        // Lưu sản phẩm khách quan tâm (tên hoặc mã sản phẩm)
        public string? SelectedProduct { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung tin nhắn")]
        public string Message { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- NEW: Truy vết nguồn gốc Lead (Dùng cho Dashboard) ---
        // Biết được khách gửi form từ trang nào (ví dụ: /vi/medical/máy-siêu-am)
        public string? SourceUrl { get; set; }

        // --- EXTENDED FIELDS (Ticket CWJ-501) --- 

        public bool IsRead { get; set; } = false;

        public string? ReadByAdminId { get; set; }

        public DateTime? ReadAt { get; set; }

        public ContactStatus ProcessingStatus { get; set; } = ContactStatus.New;

        public string? AdminNotes { get; set; }

        public ContactRegion Region { get; set; }
    }
}