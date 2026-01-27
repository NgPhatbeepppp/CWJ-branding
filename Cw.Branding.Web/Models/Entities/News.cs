using System.ComponentModel.DataAnnotations;

namespace Cw.Branding.Web.Models.Entities
{
    public class News
    {
        public int Id { get; set; }

        // --- Content (Bilingual) ---
        [MaxLength(255)]
        [Required(ErrorMessage = "Tiêu đề tiếng Việt là bắt buộc")]
        public string TitleVi { get; set; }

        [MaxLength(255)]
        [Required(ErrorMessage = "English Title is required")]
        public string TitleEn { get; set; }

        [MaxLength(500)]
        public string? SummaryVi { get; set; }

        [MaxLength(500)]
        public string? SummaryEn { get; set; }

        // HTML Content (Output từ TinyMCE) - Allow null for draft
        public string? ContentVi { get; set; }
        public string? ContentEn { get; set; }

        // --- Media & SEO ---
        [MaxLength(255)]
        public string? ThumbnailPath { get; set; } // Ảnh đại diện list

        [MaxLength(255)]
        public string? SlugVi { get; set; } // Index Unique

        [MaxLength(255)]
        public string? SlugEn { get; set; } // Index Unique

        [MaxLength(255)]
        public string? MetaTitleVi { get; set; }

        [MaxLength(255)]
        public string? MetaTitleEn { get; set; }

        [MaxLength(500)]
        public string? MetaDescVi { get; set; }

        [MaxLength(500)]
        public string? MetaDescEn { get; set; }

        // --- System ---
        public DateTime PublishedAt { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}