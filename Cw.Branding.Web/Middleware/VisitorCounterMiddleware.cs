using Cw.Branding.Web.Data; // Thay bằng namespace thực tế của DbContext bồ
using Cw.Branding.Web.Models.Entities;

namespace Cw.Branding.Web.Middleware
{
    public class VisitorCounterMiddleware
    {
        private readonly RequestDelegate _next;

        public VisitorCounterMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            // Cho request đi tiếp trước để biết kết quả StatusCode
            await _next(context);

            // Logic lọc:
            // 1. Chỉ đếm nếu Response là OK (200)
            // 2. Không đếm các trang trong khu vực Admin (Bao gồm cả các trang có prefix ngôn ngữ như /vi/admin hoặc /en/admin)
            // 3. Không đếm các file tĩnh (có dấu chấm trong path như .jpg, .css, .js)
            // 4. Không đếm các request favicon

            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Cập nhật logic: Kiểm tra xem path có chứa "/admin" để loại trừ 
            // vì route của mình có dạng /{lang}/admin/...
            bool isAdminPath = path.Contains("/admin/") || path.EndsWith("/admin");

            if (context.Response.StatusCode == 200 &&
                !isAdminPath &&
                !path.Contains(".") &&
                !path.Contains("favicon"))
            {
                try
                {
                    var log = new VisitorLog
                    {
                        IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                        PageUrl = path,
                        UserAgent = context.Request.Headers["User-Agent"].ToString(),
                        ViewedAt = DateTime.Now
                    };

                    dbContext.VisitorLogs.Add(log);
                    await dbContext.SaveChangesAsync();
                }
                catch
                {
                    // Fail-safe: Nếu lưu log lỗi thì cũng không được làm sập trang web của khách
                }
            }
        }
    }

    // Extension method để đăng ký middleware cho gọn trong Program.cs
    public static class VisitorCounterMiddlewareExtensions
    {
        public static IApplicationBuilder UseVisitorCounter(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<VisitorCounterMiddleware>();
        }
    }
}