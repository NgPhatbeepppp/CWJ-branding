using System.ComponentModel.DataAnnotations; 
using System.ComponentModel.DataAnnotations.Schema;

namespace Cw.Branding.Web.Models.Entities;

public class Category
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string NameVi { get; set; } = null!;
    public string NameEn { get; set; } = null!;

    public string? SlugVi { get; set; }
    public string? SlugEn { get; set; }

   
    // Đường dẫn ảnh icon (VD: /images/icons/cardiology.png)
    [StringLength(255)]
    public string? IconPath { get; set; }
    // ----------------

    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}