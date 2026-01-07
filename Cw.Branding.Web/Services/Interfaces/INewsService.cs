using Cw.Branding.Web.Models.Entities;

namespace Cw.Branding.Web.Services.Interfaces;

public interface INewsService
{
    Task<IEnumerable<News>> GetAllNewsAsync();
    Task<News?> GetNewsByIdAsync(int id);
    Task<News?> GetNewsBySlugAsync(string slug, string lang);
    Task<News> CreateNewsAsync(News news);
    Task<News> UpdateNewsAsync(News news);
    Task<bool> DeleteNewsAsync(int id);
    Task<IEnumerable<News>> GetPublishedNewsAsync(int count = 10);
}
