using Cw.Branding.Web.Models.Entities; // Giả định bồ có bảng Contact ở đây
using System.Collections.Generic;

namespace Cw.Branding.Web.Areas.Admin.Models
{
    public class AdminDashboardViewModel
    {
        // Các con số thống kê nhanh
        public int TodayTraffic { get; set; }
        public int MonthTraffic { get; set; }
        public int NewLeadsCount { get; set; }
        public int TotalProducts { get; set; }

        // Dữ liệu cho Biểu đồ xu hướng (7 ngày gần nhất)
        public List<string> ChartLabels { get; set; } = new();
        public List<int> ChartData { get; set; } = new();

        // Dữ liệu cho Biểu đồ Top Pages
        public List<PageStatItem> TopPages { get; set; } = new();

        // Danh sách liên hệ mới nhất
        public List<ContactFormEntry> LatestLeads { get; set; } = new();
    }

    public class PageStatItem
    {
        public string PageUrl { get; set; } = string.Empty;
        public int ViewCount { get; set; }
    }
}