using Cw.Branding.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Cw.Branding.Web.Controllers
{
    public class SeoController : Controller
    {
        private readonly AppDbContext _context;

        public SeoController(AppDbContext context)
        {
            _context = context;
        }

        [Route("sitemap.xml")]
        public async Task<IActionResult> Sitemap()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var products = await _context.Products.Where(p => p.IsActive).ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

            // 1. Trang chủ (EN & VI)
            AddUrl(sb, $"{baseUrl}/en/home", DateTime.Now, "1.0");
            AddUrl(sb, $"{baseUrl}/vi/trang-chu", DateTime.Now, "1.0");

            // 2. Danh sách Sản phẩm y tế
            foreach (var p in products)
            {
                // URL Tiếng Anh
                if (!string.IsNullOrEmpty(p.SlugEn))
                    AddUrl(sb, $"{baseUrl}/en/medical/product/{p.SlugEn}", DateTime.Now, "0.8");

                // URL Tiếng Việt
                if (!string.IsNullOrEmpty(p.SlugVi))
                    AddUrl(sb, $"{baseUrl}/vi/medical/product/{p.SlugVi}", DateTime.Now, "0.8");
            }

            sb.AppendLine("</urlset>");

            return Content(sb.ToString(), "application/xml", Encoding.UTF8);
        }

        private void AddUrl(StringBuilder sb, string url, DateTime lastMod, string priority)
        {
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{url}</loc>");
            sb.AppendLine($"    <lastmod>{lastMod:yyyy-MM-dd}</lastmod>");
            sb.AppendLine($"    <priority>{priority}</priority>");
            sb.AppendLine("  </url>");
        }
    }
}