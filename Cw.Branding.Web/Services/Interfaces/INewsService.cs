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
    }
}