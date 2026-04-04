using System.ComponentModel.DataAnnotations;

namespace Cw.Branding.Web.Models.Entities
{
    public class HeroSection
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string BackgroundImage { get; set; } = "/images/Hero illstration.png";

        // Main Headline (Dòng 1)
        [Required][MaxLength(200)] public string TitleEn { get; set; } = "Trusted Medical Solutions For Modern";
        [Required][MaxLength(200)] public string TitleVi { get; set; } = "Giải pháp Y tế Tin cậy cho";

        // Highlight Text (Dòng 2 - healthcare)
        [Required][MaxLength(100)] public string HighlightEn { get; set; } = "Healthcare";
        [Required][MaxLength(100)] public string HighlightVi { get; set; } = "Y tế Hiện đại";

        // Sub-headline
        [MaxLength(500)] public string DescriptionEn { get; set; } = "Providing innovative and efficient solutions tailored to contemporary medical needs.";
        [MaxLength(500)] public string DescriptionVi { get; set; } = "Cung cấp các giải pháp sáng tạo và hiệu quả, được tinh chỉnh theo nhu cầu y tế đương đại.";

        // CTA 1
        [MaxLength(50)] public string Cta1TextEn { get; set; } = "Explore Solutions";
        [MaxLength(50)] public string Cta1TextVi { get; set; } = "Khám phá Giải pháp";
        public string Cta1Url { get; set; } = "/en/medical";

        // CTA 2
        [MaxLength(50)] public string Cta2TextEn { get; set; } = "Contact Us";
        [MaxLength(50)] public string Cta2TextVi { get; set; } = "Liên hệ";
        public string Cta2Url { get; set; } = "/en/contact";
    }
}