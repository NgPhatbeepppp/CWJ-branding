using Cw.Branding.Web.Models.Entities;

namespace Cw.Branding.Web.Models.ViewModels
{
    public class HomeViewModel
    {
        // Khởi tạo object mặc định để tránh NullReferenceException
        public HeroSection Hero { get; set; } = new HeroSection
        {
            TitleEn = "Trusted Medical Solutions For Modern",
            TitleVi = "Giải pháp Y tế Tin cậy cho",
            HighlightEn = "Healthcare",
            HighlightVi = "Y tế Hiện đại",
            BackgroundImage = "/images/Hero illstration.png", // Ảnh cứng ban đầu
            Cta1TextEn = "Explore Solutions",
            Cta1TextVi = "Khám phá Giải pháp",
            Cta2TextEn = "Contact Us",
            Cta2TextVi = "Liên hệ"
        };

        public List<HomeSlide> Slides { get; set; } = new List<HomeSlide>();
        public List<News> LatestNews { get; set; } = new List<News>();
        public List<Product> FeaturedProducts { get; set; } = new List<Product>();

        public string MetaTitle { get; set; } = "Home - Charles Wembley";
        public string MetaDescription { get; set; } = "Trusted Medical Solutions";
    }
}