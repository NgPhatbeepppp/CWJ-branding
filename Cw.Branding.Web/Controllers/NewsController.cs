// Controllers/NewsController.cs
using Microsoft.AspNetCore.Mvc;
using Cw.Branding.Web.Models.ViewModels;
using Cw.Branding.Web.Models.Entities;

namespace Cw.Branding.Web.Controllers;

public class NewsController : Controller
{
    [Route("{lang=en}/news")]
    public IActionResult Index(string lang = "en", int page = 1)
    {
        // Setup UI flags cho Layout (xem phần hướng dẫn bên dưới)
        ViewData["IsTransparentHeader"] = true;
        ViewData["Title"] = lang == "en" ? "News & Updates" : "Tin tức & Sự kiện";

        // Dữ liệu giả lập (Mock data)
        var mockNews = new List<News>();
        for (int i = 1; i <= 6; i++)
        {
            mockNews.Add(new News
            {
                Id = i,
                TitleEn = $"Charles Wembley Expands Medical Solutions Series {i}",
                TitleVi = $"Charles Wembley mở rộng giải pháp y tế đợt {i}",
                SummaryEn = "Explore our latest innovations in diagnostic imaging and how we are transforming healthcare standards.",
                SummaryVi = "Khám phá những cải tiến mới nhất của chúng tôi trong chẩn đoán hình ảnh và cách nâng cao tiêu chuẩn chăm sóc sức khỏe.",
                PublishedAt = DateTime.Now.AddDays(-i),
                ThumbnailPath = "/images/news-opening.png", 
                SlugEn = $"news-series-{i}",
                SlugVi = $"tin-tuc-dot-{i}"
            });
        }

        var model = new NewsListViewModel
        {
            NewsItems = mockNews,
            CurrentPage = page,
            TotalPages = 5,
            CurrentLang = lang
        };

        return View(model);
    }
    // Trong Controllers/NewsController.cs

    [Route("{lang=en}/news/{slug}")]
    public IActionResult Detail(string slug, string lang = "en")
    {
        // 1. Setup UI (Header trong suốt)
        ViewData["IsTransparentHeader"] = true;

        // 2. Mock Data: Tìm bài viết theo Slug (Giả lập)
        // Trong thực tế sẽ là: _newsService.GetBySlugAsync(slug, lang);
        var currentItem = new News
        {
            Id = 1,
            TitleEn = "Charles Wembley Expands Medical Solutions Series 1",
            TitleVi = "Charles Wembley mở rộng giải pháp y tế đợt 1",
            ThumbnailPath = "/images/sample-news.jpg",
            PublishedAt = DateTime.Now.AddDays(-5),
            SlugEn = slug,
            SlugVi = slug,
            // Giả lập nội dung HTML từ Editor
            ContentEn = @"
            <p>Charles Wembley continues to pioneer in the medical field by introducing a new range of diagnostic imaging equipment.</p>
            <h2>Innovation in Healthcare</h2>
            <p>Our goal is to provide hospitals with state-of-the-art technology that ensures accurate diagnosis and patient safety.</p>
            <ul>
                <li>High-resolution MRI scanners</li>
                <li>Portable Ultrasound devices</li>
                <li>Digital X-Ray systems</li>
            </ul>
            <p>With these advancements, we hope to serve the community better.</p>
            <blockquote>""Technology is best when it brings people together."" - Matt Mullenweg</blockquote>
            <p>Contact us for more details.</p>
        ",
            ContentVi = @"
            <p>Charles Wembley tiếp tục tiên phong trong lĩnh vực y tế bằng việc giới thiệu loạt thiết bị chẩn đoán hình ảnh mới.</p>
            <h2>Đổi mới trong Chăm sóc sức khỏe</h2>
            <p>Mục tiêu của chúng tôi là cung cấp cho các bệnh viện công nghệ hiện đại nhất đảm bảo chẩn đoán chính xác và an toàn cho bệnh nhân.</p>
            <ul>
                <li>Máy chụp MRI độ phân giải cao</li>
                <li>Thiết bị siêu âm cầm tay</li>
                <li>Hệ thống X-Quang kỹ thuật số</li>
            </ul>
            <p>Với những tiến bộ này, chúng tôi hy vọng sẽ phục vụ cộng đồng tốt hơn.</p>
        "
        };

        // 3. Mock Data: Tin liên quan
        var relatedItems = new List<News>();
        for (int i = 2; i <= 4; i++)
        {
            relatedItems.Add(new News
            {
                Id = i,
                TitleEn = $"Related Article Update {i}",
                TitleVi = $"Bài viết liên quan {i}",
                ThumbnailPath = "/images/news-placeholder.jpg",
                PublishedAt = DateTime.Now.AddDays(-i * 2),
                SlugEn = $"related-news-{i}",
                SlugVi = $"tin-lien-quan-{i}"
            });
        }

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