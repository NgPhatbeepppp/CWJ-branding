namespace Cw.Branding.Web.Entities;

public class Product
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string NameVi { get; set; } = null!;
    public string NameEn { get; set; } = null!;

    public string? ShortDescriptionVi { get; set; }
    public string? ShortDescriptionEn { get; set; }

    public string? DescriptionVi { get; set; }      // HTML từ editor
    public string? DescriptionEn { get; set; }      // HTML từ editor

    public string? SlugVi { get; set; }
    public string? SlugEn { get; set; }

    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
