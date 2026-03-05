using System.ComponentModel.DataAnnotations;

namespace Cw.Branding.Web.Models.Entities
{
    public class MachineType
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string NameVi { get; set; }

        [Required, StringLength(200)]
        public string NameEn { get; set; }

        public string? SlugVi { get; set; }
        public string? SlugEn { get; set; }

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; }

        // Navigation property
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}