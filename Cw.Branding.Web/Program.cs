using Cw.Branding.Web.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Serilog (Logging)
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Add services to the container.
builder.Services.AddControllersWithViews();

// 3. DbContext (Kết nối SQL Server)
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// 4. Auth (Cookie Authentication cho Admin)
builder.Services.AddAuthentication("AdminCookie")
    .AddCookie("AdminCookie", options =>
    {
        options.Cookie.Name = "CwAdminAuth";
        options.LoginPath = "/en/admin/auth/login"; // Đổi path login có /en/
        options.LogoutPath = "/en/admin/auth/logout";
        options.AccessDeniedPath = "/en/admin/auth/access-denied";
    });

builder.Services.AddAuthorization();

// TODO: Đăng ký Services (ICategoryService, IProductService...) tại đây sau này
// builder.Services.AddScoped<ICategoryService, CategoryService>();

var app = builder.Build();

// 5. Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSerilogRequestLogging();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// =========================================================
// 6. CẤU HÌNH ROUTING (QUAN TRỌNG: Thứ tự Admin -> Default)
// =========================================================

// 1. Route cho Admin Area (Phải dùng MapAreaControllerRoute)
// Cấu trúc: /en/admin/dashboard
app.MapAreaControllerRoute(
    name: "admin",
    areaName: "Admin", // Tên Area folder
    pattern: "{lang=en}/admin/{controller=Dashboard}/{action=Index}/{id?}");

// 2. Route cho Client (Mặc định)
// Cấu trúc: /en/medical/index
app.MapControllerRoute(
    name: "default",
    pattern: "{lang=en}/{controller=Home}/{action=Index}/{id?}");

app.Run();