using System;
using System.ComponentModel.DataAnnotations;

namespace Cw.Branding.Web.Models.Entities
{
    public class VisitorLog
    {
        [Key]
        public int Id { get; set; }

        // Lưu IP để có thể đếm số lượng khách truy cập duy nhất (Unique Visitors)
        [StringLength(50)]
        public string? IpAddress { get; set; }

        // Lưu URL mà khách đã xem (ví dụ: /vi/medical/ultrasound-x2000)
        [StringLength(500)]
        public string? PageUrl { get; set; }

        // Lưu thông tin thiết bị/trình duyệt
        public string? UserAgent { get; set; }

        // Thời điểm truy cập
        public DateTime ViewedAt { get; set; } = DateTime.Now;
    }
}