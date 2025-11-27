using Cw.Branding.Web.Models.Entities;

namespace Cw.Branding.Web.Models.ViewModels
{
    public class HomeViewModel
    {
       

       
        public List<News> LatestNews { get; set; } = new List<News>();

        // SEO Meta
        public string MetaTitle { get; set; } = "Charles Wembley - Corporate Branding";
        public string MetaDescription { get; set; } = "Leading Medical & F&B Solutions Provider";
    }
}