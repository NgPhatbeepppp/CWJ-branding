using Cw.Branding.Web.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Cw.Branding.Web.Models.Entities
{
    public class ContactFormEntry
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;
        public string? Company { get; set; }
        [Required]
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }

        // Trường này lưu sản phẩm khách hàng quan tâm từ form 
        public string? SelectedProduct { get; set; }

        [Required]
        public string Message { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- EXTENDED FIELDS (Ticket CWJ-501) --- 

        public bool IsRead { get; set; } = false; // Đánh dấu đã xem 

        public string? ReadByAdminId { get; set; } // ID nhân viên đầu tiên mở xem 

        public DateTime? ReadAt { get; set; } // Thời điểm xem lần đầu 

        public ContactStatus ProcessingStatus { get; set; } = ContactStatus.New; // Trạng thái xử lý 

        public string? AdminNotes { get; set; } // Ghi chú nội bộ 

        public ContactRegion Region { get; set; }
    }
}