using Cw.Branding.Web.Areas.Admin.Models;

namespace Cw.Branding.Web.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<AdminDashboardViewModel> GetDashboardStatsAsync();
    }
}