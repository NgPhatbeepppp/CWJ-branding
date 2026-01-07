// Controllers/NewsController.cs
using Microsoft.AspNetCore.Mvc;
using Cw.Branding.Web.Models.ViewModels;
using Cw.Branding.Web.Services.Interfaces;

namespace Cw.Branding.Web.Controllers;

public class NewsController : Controller
{
    private readonly INewsService _newsService;

    public NewsController(INewsService newsService)
    {
        _newsService = newsService;
    }
    [Route("{lang=en}/news")]
    public async Task<IActionResult> Index(string lang = "en", int page = 1)
    {
        // Setup UI flags cho Layout
        ViewData["IsTransparentHeader"] = true;
        ViewData["Title"] = lang == "en" ? "News & Updates" : "Tin tức & Sự kiện";

        // Get published news from database
        var allNews = await _newsService.GetPublishedNewsAsync(100);
        
        var model = new NewsListViewModel
        {
            NewsItems = allNews,
            CurrentPage = page,
            TotalPages = 1,
            CurrentLang = lang
        };

        return View(model);
    }
    // Trong Controllers/NewsController.cs

    [Route("{lang=en}/news/{slug}")]
    public async Task<IActionResult> Detail(string slug, string lang = "en")
    {
        // 1. Setup UI (Header trong suốt)
        ViewData["IsTransparentHeader"] = true;

        // 2. Get news by slug from database
        var currentItem = await _newsService.GetNewsBySlugAsync(slug, lang);
        
        if (currentItem == null)
        {
            return NotFound();
        }

        // 3. Get related news (3 most recent published news excluding current)
        var allNews = await _newsService.GetPublishedNewsAsync(10);
        var relatedItems = allNews
            .Where(n => n.Id != currentItem.Id)
            .Take(3)
            .ToList();

        // 4. Setup SEO Meta
        var title = lang == "en" ? currentItem.TitleEn : currentItem.TitleVi;
        ViewData["Title"] = title;
        // ViewData["MetaDescription"] = ...

        var model = new NewsDetailViewModel
        {
            CurrentNews = currentItem,
            RelatedNews = relatedItems,
            CurrentLang = lang
        };

        return View(model);
    }
}