using Cw.Branding.Web.Data;
using Cw.Branding.Web.Models.Entities;
using Cw.Branding.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Cw.Branding.Web.Services.Implementations;

public class NewsService : INewsService
{
    private readonly AppDbContext _context;

    public NewsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<News>> GetAllNewsAsync()
    {
        return await _context.News
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<News?> GetNewsByIdAsync(int id)
    {
        return await _context.News.FindAsync(id);
    }

    public async Task<News?> GetNewsBySlugAsync(string slug, string lang)
    {
        return await _context.News
            .FirstOrDefaultAsync(n => 
                (lang == "en" && n.SlugEn == slug) || 
                (lang == "vi" && n.SlugVi == slug));
    }

    public async Task<News> CreateNewsAsync(News news)
    {
        news.CreatedAt = DateTime.UtcNow;
        _context.News.Add(news);
        await _context.SaveChangesAsync();
        return news;
    }

    public async Task<News> UpdateNewsAsync(News news)
    {
        news.UpdatedAt = DateTime.UtcNow;
        _context.News.Update(news);
        await _context.SaveChangesAsync();
        return news;
    }

    public async Task<bool> DeleteNewsAsync(int id)
    {
        var news = await _context.News.FindAsync(id);
        if (news == null)
            return false;

        _context.News.Remove(news);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<News>> GetPublishedNewsAsync(int count = 10)
    {
        return await _context.News
            .Where(n => n.IsPublished && n.PublishedAt <= DateTime.UtcNow)
            .OrderByDescending(n => n.PublishedAt)
            .Take(count)
            .ToListAsync();
    }
}
