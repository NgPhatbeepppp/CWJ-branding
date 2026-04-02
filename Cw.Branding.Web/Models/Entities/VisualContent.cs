using System.ComponentModel.DataAnnotations;

namespace Cw.Branding.Web.Models.Entities
{
    public enum VisualContentType
    {
        Slider = 0,     // Chạy ở trang chủ
        PageBanner = 1  // Ảnh header các trang con
    }

    public class VisualContent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public VisualContentType ContentType { get; set; }

        // Dùng để định danh banner cho trang nào (Vd: "About", "Medical", "Contact")
        // Với Slider thì field này có thể để trống hoặc để "Home"
        [MaxLength(50)]
        public string? PageCode { get; set; }

        [Required]
        [MaxLength(255)]
        public string ImageUrl { get; set; } = string.Empty;

        // Nội dung song ngữ
        [MaxLength(200)]
        public string? TitleEn { get; set; }
        [MaxLength(200)]
        public string? TitleVi { get; set; }

        [MaxLength(500)]
        public string? DescriptionEn { get; set; }
        [MaxLength(500)]
        public string? DescriptionVi { get; set; }

        [MaxLength(255)]
        public string? LinkUrl { get; set; }

        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}