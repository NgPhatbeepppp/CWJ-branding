namespace Cw.Branding.Web.Entities;

public class Category
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string NameVi { get; set; } = null!;
    public string NameEn { get; set; } = null!;

    public string? SlugVi { get; set; }
    public string? SlugEn { get; set; }

    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
