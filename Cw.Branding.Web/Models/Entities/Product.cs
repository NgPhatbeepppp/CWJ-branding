namespace Cw.Branding.Web.Models.Entities;

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

    // Bổ sung Foreign Keys cho Filter
    public int? BrandId { get; set; }
    public Brand? Brand { get; set; }

    public int? MachineTypeId { get; set; }
    public MachineType? MachineType { get; set; }

    // Thông số kỹ thuật (Rich Text) - Như anh yêu cầu
    public string? TechnicalSpecsVi { get; set; }
    public string? TechnicalSpecsEn { get; set; }

    // PDF Datasheet (Path)
    public string? DatasheetPath { get; set; }
}
