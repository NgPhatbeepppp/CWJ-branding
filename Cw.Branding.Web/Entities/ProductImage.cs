namespace Cw.Branding.Web.Entities;

public class ProductImage
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string FilePath { get; set; } = null!;    // /uploads/products/xxx.jpg
    public string? AltVi { get; set; }
    public string? AltEn { get; set; }

    public bool IsMain { get; set; } = false;
    public int DisplayOrder { get; set; }
}
