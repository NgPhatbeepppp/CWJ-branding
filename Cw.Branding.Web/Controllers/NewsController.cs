using Microsoft.AspNetCore.Mvc;
using Cw.Branding.Web.Models.ViewModels;
using Cw.Branding.Web.Services.Interfaces;

namespace Cw.Branding.Web.Controllers
{
    [Route("{lang:regex(^(en|vi)$)}")]
    public class NewsController : Controller
    {
        private readonly INewsService _newsService;

        public NewsController(INewsService newsService)
        {
            _newsService = newsService;
        }

        // --- TRANG DANH SÁCH (INDEX) ---
        [HttpGet("news", Name = "NewsIndexEn")]
        [HttpGet("tin-tuc", Name = "NewsIndexVi")]
        public async Task<IActionResult> Index(string lang = "en", int page = 1)
        {
            // Setup UI
            ViewData["IsTransparentHeader"] = true;
            ViewData["Title"] = lang == "en" ? "News & Updates" : "Tin tức & Sự kiện";

            int pageSize = 6;
            if (page < 1) page = 1;

            // Lấy dữ liệu thật từ Service
            var (items, totalCount) = await _newsService.GetPagedNewsAsync(page, pageSize);

            // Tính toán phân trang
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var model = new NewsListViewModel
            {
                NewsItems = items,
                CurrentPage = page,
                TotalPages = totalPages,
                CurrentLang = lang
            };

            return View(model);
        }

        // --- TRANG CHI TIẾT (DETAIL) ---
        [HttpGet("news/{slug}")]
        [HttpGet("tin-tuc/{slug}")]
        public async Task<IActionResult> Detail(string slug, string lang = "en")
        {
            // Setup UI
            ViewData["IsTransparentHeader"] = true;

            // 1. Lấy bài viết theo Slug
            var newsItem = await _newsService.GetBySlugAsync(slug, lang);

            // Nếu không tìm thấy hoặc bài viết chưa active -> Báo lỗi 404
            if (newsItem == null)
            {
                return NotFound();
            }

            // 2. Lấy tin liên quan (trừ bài hiện tại)
            var relatedNews = await _newsService.GetRelatedNewsAsync(newsItem.Id, 3);

            // 3. Setup SEO Meta
            var metaTitle = lang == "en" ? newsItem.MetaTitleEn : newsItem.MetaTitleVi;
            var pageTitle = !string.IsNullOrEmpty(metaTitle)
                            ? metaTitle
                            : (lang == "en" ? newsItem.TitleEn : newsItem.TitleVi);

            var metaDesc = lang == "en" ? newsItem.MetaDescEn : newsItem.MetaDescVi;
            var pageDesc = !string.IsNullOrEmpty(metaDesc)
                           ? metaDesc
                           : (lang == "en" ? newsItem.SummaryEn : newsItem.SummaryVi);

            ViewData["Title"] = pageTitle;
            ViewData["MetaDescription"] = pageDesc;

            // 4. Build Model
            var model = new NewsDetailViewModel
            {
                CurrentNews = newsItem,
                RelatedNews = relatedNews,
                CurrentLang = lang
            };

            return View(model);
        }
    }
}