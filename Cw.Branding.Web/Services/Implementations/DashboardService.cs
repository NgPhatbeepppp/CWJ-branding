using Cw.Branding.Web.Areas.Admin.Models;
using Cw.Branding.Web.Data;
using Cw.Branding.Web.Models.Enums;
using Cw.Branding.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cw.Branding.Web.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardViewModel> GetDashboardStatsAsync()
        {
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            var model = new AdminDashboardViewModel();

            // 1. Thống kê nhanh (Quick Stats)
            model.TodayTraffic = await _context.VisitorLogs
                .CountAsync(x => x.ViewedAt.Date == today);

            model.MonthTraffic = await _context.VisitorLogs
                .CountAsync(x => x.ViewedAt >= firstDayOfMonth);

            // Giả định bồ có bảng Contacts và Products
            model.NewLeadsCount = await _context.ContactFormEntries
            .CountAsync(x => x.ProcessingStatus == ContactStatus.New);
            model.TotalProducts = await _context.Products.CountAsync();

            // 2. Dữ liệu biểu đồ 7 ngày gần nhất (Traffic Trend)
            for (int i = 6; i >= 0; i--)
            {
                var date = today.AddDays(-i);
                var count = await _context.VisitorLogs.CountAsync(x => x.ViewedAt.Date == date);

                model.ChartLabels.Add(date.ToString("dd/MM"));
                model.ChartData.Add(count);
            }

            // 3. Top 5 trang được xem nhiều nhất
            model.TopPages = await _context.VisitorLogs
                .GroupBy(x => x.PageUrl)
                .Select(g => new PageStatItem
                {
                    PageUrl = g.Key ?? "/",
                    ViewCount = g.Count()
                })
                .OrderByDescending(x => x.ViewCount)
                .Take(5)
                .ToListAsync();

            // 4. Lấy 5 liên hệ mới nhất
            model.LatestLeads = await _context.ContactFormEntries
                .OrderByDescending(x => x.CreatedAt)
                .Take(5)
                .ToListAsync();

            return model;
        }
    }
}