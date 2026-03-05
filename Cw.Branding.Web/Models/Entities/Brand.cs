using System.ComponentModel.DataAnnotations;

namespace Cw.Branding.Web.Models.Entities
{
    public class Brand
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } // Giữ nguyên tên hãng (VD: GE, Siemens)

        public string? Slug { get; set; }

        public string? LogoPath { get; set; }

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}