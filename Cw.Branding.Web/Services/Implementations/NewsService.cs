using Cw.Branding.Web.Data;
using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Cw.Branding.Web.Services.Implementations
{
    public class NewsService : INewsService
    {
        private readonly AppDbContext _context;

        public NewsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<News>> GetAllAsync()
        {
            return await _context.News
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<News?> GetByIdAsync(int id)
        {
            return await _context.News.FindAsync(id);
        }

        public async Task CreateAsync(News news)
        {
            news.CreatedAt = DateTime.Now;

            // Auto-generate Slugs if empty
            if (string.IsNullOrWhiteSpace(news.SlugVi))
            {
                news.SlugVi = await GenerateUniqueSlugAsync(news.TitleVi, isVi: true);
            }
            if (string.IsNullOrWhiteSpace(news.SlugEn))
            {
                news.SlugEn = await GenerateUniqueSlugAsync(news.TitleEn, isVi: false);
            }

            _context.News.Add(news);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(News news)
        {
            var existingNews = await _context.News.FindAsync(news.Id);
            if (existingNews == null) return;

            // Update simple fields
            existingNews.TitleVi = news.TitleVi;
            existingNews.TitleEn = news.TitleEn;
            existingNews.SummaryVi = news.SummaryVi;
            existingNews.SummaryEn = news.SummaryEn;
            existingNews.ContentVi = news.ContentVi;
            existingNews.ContentEn = news.ContentEn;
            existingNews.ThumbnailPath = news.ThumbnailPath;
            existingNews.PublishedAt = news.PublishedAt;
            existingNews.IsActive = news.IsActive;

            // Update SEO fields
            existingNews.MetaTitleVi = news.MetaTitleVi;
            existingNews.MetaTitleEn = news.MetaTitleEn;
            existingNews.MetaDescVi = news.MetaDescVi;
            existingNews.MetaDescEn = news.MetaDescEn;

            // Handle Slug regeneration only if requested or changed
            // Note: Normally we don't auto-change slug on update to avoid broken links, 
            // unless explicit or empty. Here logic: if provided slug is different, check unique.
            if (news.SlugVi != existingNews.SlugVi)
            {
                existingNews.SlugVi = await GenerateUniqueSlugAsync(news.SlugVi ?? news.TitleVi, true, news.Id);
            }
            if (news.SlugEn != existingNews.SlugEn)
            {
                existingNews.SlugEn = await GenerateUniqueSlugAsync(news.SlugEn ?? news.TitleEn, false, news.Id);
            }

            existingNews.UpdatedAt = DateTime.Now;

            _context.News.Update(existingNews);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news != null)
            {
                _context.News.Remove(news); // Hard delete as per MVP spec
                await _context.SaveChangesAsync();
            }
        }

        // --- Helper: Slug Generator ---
        private async Task<string> GenerateUniqueSlugAsync(string title, bool isVi, int? ignoreId = null)
        {
            if (string.IsNullOrEmpty(title)) return "";

            // 1. Convert logic (Simple normalization)
            string slug = title.ToLower().Trim();

            // Remove Vietnamese accents
            slug = RemoveDiacritics(slug);

            // Remove invalid chars
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

            // Convert multiple spaces into one space   
            slug = Regex.Replace(slug, @"\s+", " ").Trim();

            // Replace space with hyphen
            slug = slug.Replace(" ", "-");

            // 2. Check Uniqueness
            string originalSlug = slug;
            int counter = 1;
            bool exists;

            do
            {
                if (isVi)
                {
                    exists = await _context.News.AnyAsync(n => n.SlugVi == slug && n.Id != ignoreId);
                }
                else
                {
                    exists = await _context.News.AnyAsync(n => n.SlugEn == slug && n.Id != ignoreId);
                }

                if (exists)
                {
                    slug = $"{originalSlug}-{counter}";
                    counter++;
                }
            } while (exists);

            return slug;
        }

        private string RemoveDiacritics(string text)
        {
            string normalizedString = text.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
        // Thêm vào class NewsService
        public async Task<(IEnumerable<News> Items, int TotalCount)> GetPagedNewsAsync(int page, int pageSize)
        {
            // 1. Query cơ bản: Chỉ lấy Active và Đã đến ngày publish
            var query = _context.News
                .Where(n => n.IsActive && n.PublishedAt <= DateTime.Now);

            // 2. Đếm tổng số record (để tính TotalPages)
            var totalCount = await query.CountAsync();

            // 3. Lấy dữ liệu phân trang
            var items = await query
                .OrderByDescending(n => n.PublishedAt) // Mới nhất lên đầu
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
        public async Task<News?> GetBySlugAsync(string slug, string lang)
{
        // Chỉ lấy bài Active và đã Publish
            var query = _context.News.Where(n => n.IsActive && n.PublishedAt <= DateTime.Now);

            // Tìm theo Slug tương ứng với ngôn ngữ
            if (lang == "en")
            {
                return await query.FirstOrDefaultAsync(n => n.SlugEn == slug);
            }
            else
            {
                return await query.FirstOrDefaultAsync(n => n.SlugVi == slug);
            }
        }

        public async Task<IEnumerable<News>> GetRelatedNewsAsync(int currentId, int count)
        {
            return await _context.News
                .Where(n => n.IsActive 
                            && n.PublishedAt <= DateTime.Now 
                            && n.Id != currentId) // Loại trừ bài hiện tại
                .OrderByDescending(n => n.PublishedAt)
                .Take(count)
                .ToListAsync();
        }
    }

}