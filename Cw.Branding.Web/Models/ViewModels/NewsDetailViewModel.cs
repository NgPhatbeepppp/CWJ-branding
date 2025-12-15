using Cw.Branding.Web.Models.Entities;

namespace Cw.Branding.Web.Models.ViewModels;

public class NewsDetailViewModel
{
    public News CurrentNews { get; set; } = null!;
    public IEnumerable<News> RelatedNews { get; set; } = new List<News>();
    public string CurrentLang { get; set; } = "en";
}