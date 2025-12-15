namespace Cw.Branding.Web.Models.Entities;

public class News
{
    public int Id { get; set; }

    // --- Thông tin chính (Song ngữ) ---
    public string TitleVi { get; set; } = null!;
    public string TitleEn { get; set; } = null!;

    public string? SummaryVi { get; set; }
    public string? SummaryEn { get; set; }

    public string? ContentVi { get; set; }    // HTML từ editor
    public string? ContentEn { get; set; }

    public string? SlugVi { get; set; }
    public string? SlugEn { get; set; }

    // --- Media ---
    public string? ThumbnailPath { get; set; } // Đường dẫn ảnh đại diện

    // --- SEO Meta Tags (Quan trọng cho Branding) ---
    public string? MetaTitleVi { get; set; }
    public string? MetaTitleEn { get; set; }
    public string? MetaDescVi { get; set; }
    public string? MetaDescEn { get; set; }

    // --- Trạng thái bài viết ---
    public bool IsPublished { get; set; } = false;
    public DateTime? PublishedAt { get; set; }

    // --- Audit ---
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}