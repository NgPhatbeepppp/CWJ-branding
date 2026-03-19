namespace Cw.Branding.Web.Models.ViewModels
{
    public class SeoViewModel
    {
        public string Title { get; set; } = "Charles Wembley";
        public string Description { get; set; } = "";
        public string? ImageUrl { get; set; }
        public string? CanonicalUrl { get; set; }
        public string Type { get; set; } = "website"; // "product" hoặc "article"
    }
}