using System.ComponentModel.DataAnnotations;

namespace Cw.Branding.Web.Models.Entities
{
    public class HomeSlide
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string ImageUrl { get; set; } = string.Empty;

        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        // Ghi chú nội bộ cho Admin dễ nhớ ảnh này là gì
        [MaxLength(100)]
        public string? Note { get; set; }
    }
}