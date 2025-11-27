using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cw.Branding.Web.Data;
using Cw.Branding.Web.Models.ViewModels;
using Cw.Branding.Web.Models;
using Cw.Branding.Web.Models.Entities;
using System.Diagnostics;

namespace Cw.Branding.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel();

            try
            {
                // 1. Fetch Latest News (Giữ nguyên logic lấy 3 bài mới nhất)
                viewModel.LatestNews = await _context.News
                    .Where(n => n.IsPublished && n.PublishedAt <= DateTime.Now)
                    .OrderByDescending(n => n.IsPublished)
                    .Take(3)
                    .ToListAsync();

                // 2. Medical Categories: Đã bỏ fetch DB. 
                // Ở View sẽ hardcode link dẫn tới trang /medical-solutions hoặc /categories
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Homepage data");
            }

            return View(viewModel);
        }

        
        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}