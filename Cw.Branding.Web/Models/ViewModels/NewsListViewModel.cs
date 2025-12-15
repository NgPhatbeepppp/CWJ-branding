// Models/ViewModels/NewsListViewModel.cs
using Cw.Branding.Web.Models.Entities;

namespace Cw.Branding.Web.Models.ViewModels;

public class NewsListViewModel
{
    public IEnumerable<News> NewsItems { get; set; } = new List<News>();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public string CurrentLang { get; set; } = "en"; // "en" hoặc "vi"
}