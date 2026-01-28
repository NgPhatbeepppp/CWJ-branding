using Cw.Branding.Web.Models.Entities;

namespace Cw.Branding.Web.Services.Interfaces
{
    public interface INewsService
    {
        Task<IEnumerable<News>> GetAllAsync();
        Task<News?> GetByIdAsync(int id);
        Task CreateAsync(News news);
        Task UpdateAsync(News news);
        Task DeleteAsync(int id);
        // Lấy danh sách tin tức active, sort theo ngày, có phân trang
        Task<(IEnumerable<News> Items, int TotalCount)> GetPagedNewsAsync(int page, int pageSize);

        Task<News?> GetBySlugAsync(string slug, string lang);
        Task<IEnumerable<News>> GetRelatedNewsAsync(int currentId, int count);
    }
}