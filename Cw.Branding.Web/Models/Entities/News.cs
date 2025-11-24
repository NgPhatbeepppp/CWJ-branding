namespace Cw.Branding.Web.Models.Entities;

public class News
{
    public int Id { get; set; }

    public string TitleVi { get; set; } = null!;
    public string TitleEn { get; set; } = null!;

    public string? SummaryVi { get; set; }
    public string? SummaryEn { get; set; }

    public string? ContentVi { get; set; }    // HTML từ editor
    public string? ContentEn { get; set; }

    public string? SlugVi { get; set; }
    public string? SlugEn { get; set; }

    public bool IsPublished { get; set; } = false;
    public DateTime? PublishedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
